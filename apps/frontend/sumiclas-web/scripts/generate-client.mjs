import { spawn, spawnSync } from 'node:child_process';
import { existsSync, mkdtempSync, readFileSync, writeFileSync } from 'node:fs';
import { rm } from 'node:fs/promises';
import { createServer } from 'node:net';
import { tmpdir } from 'node:os';
import { dirname, join, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';

const webRoot = resolve(dirname(fileURLToPath(import.meta.url)), '..');
const clientDir = join(webRoot, 'src', 'modules', 'Shared', 'Infrastructure');
const nswagConfig = join(clientDir, 'GeneratedApiClient.nswag');
const apiProject = resolve(webRoot, '..', '..', 'backend', 'api', 'SumiClas.Api.csproj');
const startupTimeoutMs = 120_000;
const defaultApiServer = 'https://localhost:7064/';

const fail = (message) => {
  throw new Error(message);
};

const freePort = () =>
  new Promise((resolvePort, reject) => {
    const server = createServer();
    server.once('error', reject);
    server.listen(0, '127.0.0.1', () => {
      const { port } = server.address();
      server.close(() => resolvePort(port));
    });
  });

const waitForDocument = async (url, apiProcess, getLog) => {
  const deadline = Date.now() + startupTimeoutMs;
  while (Date.now() < deadline) {
    if (apiProcess.exitCode !== null) {
      fail(`The API stopped while starting (exit code ${apiProcess.exitCode}). Is the database up (docker compose up -d)?\n${getLog()}`);
    }
    try {
      const response = await fetch(url, { signal: AbortSignal.timeout(5000) });
      if (response.ok) return;
    } catch {
      // API not listening yet
    }
    await new Promise((r) => setTimeout(r, 1000));
  }
  fail(`The API did not serve ${url} within ${startupTimeoutMs / 1000}s.\n${getLog()}`);
};

const stop = async (child) => {
  if (!child || child.exitCode !== null) return;
  const exited = new Promise((resolveExit) => child.once('exit', resolveExit));
  if (process.platform === 'win32') {
    spawnSync('taskkill', ['/pid', String(child.pid), '/T', '/F'], { stdio: 'ignore' });
  } else {
    child.kill('SIGTERM');
  }
  await exited;
};

const startTemporaryApi = async (workDir) => {
  const buildDir = join(workDir, 'build');
  console.log('[generate:client] Building the API from the current source...');
  const build = spawnSync('dotnet', ['build', apiProject, '-o', buildDir, '--nologo', '-v', 'q', '--disable-build-servers'], {
    encoding: 'utf8',
  });
  if (build.status !== 0) fail(`dotnet build failed:\n${build.stdout}${build.stderr}`);

  const port = await freePort();
  let log = '';
  const api = spawn('dotnet', [join(buildDir, 'SumiClas.Api.dll')], {
    cwd: buildDir,
    env: { ...process.env, ASPNETCORE_ENVIRONMENT: 'Development', ASPNETCORE_URLS: `http://localhost:${port}` },
  });
  api.stdout.on('data', (chunk) => (log += chunk));
  api.stderr.on('data', (chunk) => (log += chunk));

  const documentUrl = `http://localhost:${port}/openapi/v1.json`;
  console.log(`[generate:client] Starting a temporary API on port ${port}...`);
  await waitForDocument(documentUrl, api, () => log.slice(-1500));
  return { api, documentUrl };
};

if (!existsSync(nswagConfig)) fail(`Missing ${nswagConfig}`);

const workDir = mkdtempSync(join(tmpdir(), 'sumiclas-client-'));
const tempConfig = join(clientDir, '.GeneratedApiClient.tmp.nswag');
let api = null;

const cleanup = async () => {
  await stop(api);
  await rm(tempConfig, { force: true });
  try {
    await rm(workDir, { recursive: true, force: true, maxRetries: 20, retryDelay: 500 });
  } catch {
    console.warn(`[generate:client] Could not remove the temporary folder ${workDir}; it is safe to delete.`);
  }
};
process.on('SIGINT', async () => {
  await cleanup();
  process.exit(130);
});

const readExternalUrl = () => {
  const flag = process.argv.indexOf('--url');
  if (flag === -1) return null;
  const value = process.argv[flag + 1];
  try {
    return new URL(value).href;
  } catch {
    return fail('--url needs a valid URL, for example --url https://localhost:7064/openapi/v1.json');
  }
};

const downloadDocument = async (documentUrl) => {
  let document;
  try {
    const response = await fetch(documentUrl, { signal: AbortSignal.timeout(15_000) });
    if (!response.ok) fail(`The API answered ${response.status} for ${documentUrl}`);
    document = await response.json();
  } catch (error) {
    return fail(`Could not read the OpenAPI document from ${documentUrl}: ${error.message}`);
  }
  document.servers = [{ url: defaultApiServer }];
  const documentPath = join(workDir, 'openapi.json');
  writeFileSync(documentPath, JSON.stringify(document));
  return documentPath;
};

try {
  let documentUrl = readExternalUrl();
  if (!documentUrl) {
    ({ api, documentUrl } = await startTemporaryApi(workDir));
  }
  const documentPath = await downloadDocument(documentUrl);

  const config = JSON.parse(readFileSync(nswagConfig, 'utf8').replace(/^﻿/, ''));
  config.documentGenerator.fromDocument.json = '';
  config.documentGenerator.fromDocument.url = documentPath;
  writeFileSync(tempConfig, JSON.stringify(config, null, 2));

  console.log(`[generate:client] Generating the client from ${documentUrl}`);
  const nswagCli = join(webRoot, 'node_modules', 'nswag', 'bin', 'nswag.js');
  if (!existsSync(nswagCli)) fail('NSwag CLI not found. Run "pnpm install" first.');
  const nswag = spawnSync(process.execPath, [nswagCli, 'run', tempConfig, '/runtime:Net100'], {
    cwd: webRoot,
    stdio: 'inherit',
  });
  if (nswag.status !== 0) fail('NSwag failed (see the output above).');

  console.log('[generate:client] Done: GeneratedApiClient.ts regenerated.');
} catch (error) {
  console.error(`\n[generate:client] ${error.message}`);
  process.exitCode = 1;
} finally {
  await cleanup();
}
