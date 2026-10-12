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
import { refreshSession, setSessionLostHandler } from '@/modules/Shared/Infrastructure/authHttp';
import './index.css';

// Provisional until the AuthProvider takes over: the access token only lives in memory, so restore the session from
// the refresh cookie before the first render, and send the user to the login screen when a refresh is refused.
setSessionLostHandler(() => window.location.assign('/login'));

const root = createRoot(document.getElementById('root')!);

void refreshSession({ notifyOnLoss: false })
  .catch(() => undefined)
  .finally(() => root.render(
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
      </SessionRepositoryProvider>
    </ApiClientProvider>
  </StrictMode>
  ));
