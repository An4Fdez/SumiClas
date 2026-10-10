import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { BrowserRouter } from 'react-router-dom';
import App from './App.tsx';
import { AnnouncementRepositoryProvider } from '@/context/AnnouncementRepositoryProvider';
import { ApiClientProvider } from '@/context/ApiClientProvider';
import { AuthenticationRepositoryProvider } from '@/context/AuthenticationRepositoryProvider';
import { CalendarEventRepositoryProvider } from '@/context/CalendarEventRepositoryProvider';
import { ClassExpenseRepositoryProvider } from '@/context/ClassExpenseRepositoryProvider';
import { HomeworkRepositoriesProvider } from '@/context/HomeworkRepositoriesProvider';
import { LocalityRepositoryProvider } from '@/context/LocalityRepositoryProvider';
import { LostItemRepositoryProvider } from '@/context/LostItemRepositoryProvider';
import { OnboardingRepositoriesProvider } from '@/context/OnboardingRepositoriesProvider';
import { ProvinceRepositoryProvider } from '@/context/ProvinceRepositoryProvider';
import { StudentRepositoryProvider } from '@/context/StudentRepositoryProvider';
import { UserRepositoryProvider } from '@/context/UserRepositoryProvider';
import './index.css';

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <ApiClientProvider>
      <AuthenticationRepositoryProvider>
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
                            <App />
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
      </AuthenticationRepositoryProvider>
    </ApiClientProvider>
  </StrictMode>
);
