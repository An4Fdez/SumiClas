import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { BrowserRouter } from 'react-router-dom';
import App from './App.tsx';
import { ApiClientProvider } from '@/context/ApiClientProvider';
import './index.css';

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <ApiClientProvider>
      <BrowserRouter>
        <App />
      </BrowserRouter>
    </ApiClientProvider>
  </StrictMode>
);
