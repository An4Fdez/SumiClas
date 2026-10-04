import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { BrowserRouter } from 'react-router-dom';
import App from './App.tsx';
import { ApiClientProvider } from '@/context/ApiClientProvider';
import { OnboardingRepositoriesProvider } from '@/context/OnboardingRepositoriesProvider';
import { StudentRepositoryProvider } from '@/context/StudentRepositoryProvider';
import { UserRepositoryProvider } from '@/context/UserRepositoryProvider';
import './index.css';

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <ApiClientProvider>
      <UserRepositoryProvider>
        <StudentRepositoryProvider>
          <OnboardingRepositoriesProvider>
            <BrowserRouter>
              <App />
            </BrowserRouter>
          </OnboardingRepositoriesProvider>
        </StudentRepositoryProvider>
      </UserRepositoryProvider>
    </ApiClientProvider>
  </StrictMode>
);
