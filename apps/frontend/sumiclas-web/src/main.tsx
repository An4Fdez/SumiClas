import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { BrowserRouter } from 'react-router-dom';
import App from './App.tsx';
import { AnnouncementRepositoryProvider } from '@/context/AnnouncementRepositoryProvider';
import { ApiClientProvider } from '@/context/ApiClientProvider';
import { LostItemRepositoryProvider } from '@/context/LostItemRepositoryProvider';
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
            <AnnouncementRepositoryProvider>
              <LostItemRepositoryProvider>
                <BrowserRouter>
                  <App />
                </BrowserRouter>
              </LostItemRepositoryProvider>
            </AnnouncementRepositoryProvider>
          </OnboardingRepositoriesProvider>
        </StudentRepositoryProvider>
      </UserRepositoryProvider>
    </ApiClientProvider>
  </StrictMode>
);
