import { Routes, Route } from 'react-router-dom';
import { Layout } from '@/components/layout/Layout';
import { AppLayout } from '@/components/layout/AppLayout';
import { RequireSession } from '@/components/layout/RequireSession';
import Home from '@/pages/Home';
import Login from '@/pages/Login';
import Register from '@/pages/Register';
import Terms from '@/pages/Terms';
import Privacy from '@/pages/Privacy';
import Cookies from '@/pages/Cookies';
import Support from '@/pages/Support';
import Onboarding from '@/pages/Onboarding';
import Dashboard from '@/pages/Dashboard';
import Announcements from '@/pages/Announcements';
import Calendar from '@/pages/Calendar';
import Homework from '@/pages/Homework';
import Expenses from '@/pages/Expenses';
import LostAndFound from '@/pages/LostAndFound';
import More from '@/pages/More';
import Profile from '@/pages/Profile';
import MyClassRole from '@/pages/MyClassRole';
import Notifications from '@/pages/Notifications';
import Help from '@/pages/Help';

export default function App() {
  return (
    <Routes>
      <Route element={<Layout />}>
        <Route path="/" element={<Home />} />
        <Route path="/login" element={<Login />} />
        <Route path="/register" element={<Register />} />
        <Route path="/terms" element={<Terms />} />
        <Route path="/privacy" element={<Privacy />} />
        <Route path="/cookies" element={<Cookies />} />
        <Route path="/support" element={<Support />} />
        <Route element={<RequireSession />}>
          <Route path="/onboarding" element={<Onboarding />} />
        </Route>
      </Route>

      <Route element={<RequireSession />}>
        <Route element={<AppLayout />}>
          <Route path="/home" element={<Dashboard />} />
          <Route path="/announcements" element={<Announcements />} />
          <Route path="/calendar" element={<Calendar />} />
          <Route path="/homework" element={<Homework />} />
          <Route path="/expenses" element={<Expenses />} />
          <Route path="/lost-and-found" element={<LostAndFound />} />
          <Route path="/more" element={<More />} />
          <Route path="/more/profile" element={<Profile />} />
          <Route path="/more/class-role" element={<MyClassRole />} />
          <Route path="/more/notifications" element={<Notifications />} />
          <Route path="/more/help" element={<Help />} />
        </Route>
      </Route>
    </Routes>
  );
}
