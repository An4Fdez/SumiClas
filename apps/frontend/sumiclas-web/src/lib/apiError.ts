type ApiErrorShape = {
  status?: unknown;
  code?: unknown;
  detail?: unknown;
};

const MESSAGE_BY_CODE: Record<string, string> = {
  'User.EmailAlreadyRegistered': 'Ya existe una cuenta con este correo electrónico.',
  'Authentication.Unauthenticated': 'No hemos podido identificar tu sesión. Vuelve a crear tu cuenta.',
  'Enrollment.AlreadyExists': 'Tu hijo/a ya está matriculado/a en una clase de este curso.',
  'User.NotFound': 'No encontramos tu cuenta. Vuelve a crear tu cuenta.',
};

const MESSAGE_BY_STATUS: Record<number, string> = {
  400: 'Algún dato no es válido. Revisa el formulario e inténtalo de nuevo.',
  401: 'Tu sesión no es válida. Vuelve a crear tu cuenta.',
  403: 'No tienes permiso para hacer esto.',
  404: 'No hemos encontrado lo que buscas.',
  409: 'Esta operación entra en conflicto con datos que ya existen.',
};

const NETWORK_MESSAGE = 'No hemos podido conectar con el servidor. Comprueba tu conexión e inténtalo de nuevo.';
const SERVER_MESSAGE = 'El servidor ha tenido un problema. Inténtalo de nuevo en unos minutos.';
const GENERIC_MESSAGE = 'Ha ocurrido un error inesperado. Inténtalo de nuevo.';

function asShape(error: unknown): ApiErrorShape | null {
  return typeof error === 'object' && error !== null ? (error as ApiErrorShape) : null;
}

/**
 * The generated client throws the ProblemDetails object itself when the error response has a body, an
 * ApiException when it has none, and a TypeError when the request never reached the server.
 */
export function getApiErrorStatus(error: unknown): number | undefined {
  const status = asShape(error)?.status;
  return typeof status === 'number' ? status : undefined;
}

/** An error whose message is already written for the user (in Spanish) and is shown as is. */
export class UserFacingError extends Error {}

export function getApiErrorMessage(error: unknown): string {
  if (error instanceof UserFacingError) return error.message;
  if (error instanceof TypeError) return NETWORK_MESSAGE;

  const shape = asShape(error);
  if (!shape) return GENERIC_MESSAGE;

  if (typeof shape.code === 'string') {
    const byCode = MESSAGE_BY_CODE[shape.code];
    if (byCode) return byCode;
    if (typeof shape.detail === 'string' && shape.detail) return shape.detail;
  }

  const status = getApiErrorStatus(error);
  if (status === undefined) return GENERIC_MESSAGE;
  if (status >= 500) return SERVER_MESSAGE;
  return MESSAGE_BY_STATUS[status] ?? GENERIC_MESSAGE;
}
