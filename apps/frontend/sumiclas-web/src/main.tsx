import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { BrowserRouter } from 'react-router-dom';
import App from './App.tsx';
import { AnnouncementRepositoryProvider } from '@/context/AnnouncementRepositoryProvider';
import { ApiClientProvider } from '@/context/ApiClientProvider';
import { SessionRepositoryProvider } from '@/context/SessionRepositoryProvider';
import { CalendarEventRepositoryProvider } from '@/context/CalendarEventRepositoryProvider';
import { ClassExpenseRepositoryProvider } from '@/context/ClassExpenseRepositoryProvider';
import { HomeworkRepositoriesProvider } from '@/context/HomeworkRepositoriesProvider';
import { LocalityRepositoryProvider } from '@/context/LocalityRepositoryProvider';
import { LostItemRepositoryProvider } from '@/context/LostItemRepositoryProvider';
import { OnboardingRepositoriesProvider } from '@/context/OnboardingRepositoriesProvider';
import { ProvinceRepositoryProvider } from '@/context/ProvinceRepositoryProvider';
import { StudentRepositoryProvider } from '@/context/StudentRepositoryProvider';
import { UserRepositoryProvider } from '@/context/UserRepositoryProvider';
import { AuthProvider } from '@/context/AuthProvider';
import { removeLegacySessionKeys } from '@/lib/auth';
import './index.css';

removeLegacySessionKeys();

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <ApiClientProvider>
      <SessionRepositoryProvider>
        <UserRepositoryProvider>
          <StudentRepositoryProvider>
            <ProvinceRepositoryProvider>
              <LocalityRepositoryProvider>
                <OnboardingRepositoriesProvider>
                  <AnnouncementRepositoryProvider>
                    <LostItemRepositoryProvider>
                      <CalendarEventRepositoryProvider>
                        <HomeworkRepositoriesProvider>
                          <ClassExpenseRepositoryProvider>
                            <BrowserRouter>
                              <AuthProvider>
                                <App />
                              </AuthProvider>
                            </BrowserRouter>
                          </ClassExpenseRepositoryProvider>
                        </HomeworkRepositoriesProvider>
                      </CalendarEventRepositoryProvider>
                    </LostItemRepositoryProvider>
                  </AnnouncementRepositoryProvider>
                </OnboardingRepositoriesProvider>
              </LocalityRepositoryProvider>
            </ProvinceRepositoryProvider>
          </StudentRepositoryProvider>
        </UserRepositoryProvider>
      </SessionRepositoryProvider>
    </ApiClientProvider>
  </StrictMode>,
);
