import { Routes, Route } from 'react-router-dom';
import { Layout } from '@/components/layout/Layout';
import { AppLayout } from '@/components/layout/AppLayout';
import Home from '@/pages/Home';
import Login from '@/pages/Login';
import Register from '@/pages/Register';
import Terms from '@/pages/Terms';
import Privacy from '@/pages/Privacy';
import Onboarding from '@/pages/Onboarding';
import Dashboard from '@/pages/Dashboard';
import Announcements from '@/pages/Announcements';
import Calendar from '@/pages/Calendar';
import Homework from '@/pages/Homework';
import Expenses from '@/pages/Expenses';
import LostAndFound from '@/pages/LostAndFound';
import More from '@/pages/More';
import MyClassRole from '@/pages/MyClassRole';

export default function App() {
  return (
    <Routes>
      <Route element={<Layout />}>
        <Route path="/" element={<Home />} />
        <Route path="/login" element={<Login />} />
        <Route path="/register" element={<Register />} />
        <Route path="/terms" element={<Terms />} />
        <Route path="/privacy" element={<Privacy />} />
        <Route path="/onboarding" element={<Onboarding />} />
      </Route>

      <Route element={<AppLayout />}>
        <Route path="/home" element={<Dashboard />} />
        <Route path="/announcements" element={<Announcements />} />
        <Route path="/calendar" element={<Calendar />} />
        <Route path="/homework" element={<Homework />} />
        <Route path="/expenses" element={<Expenses />} />
        <Route path="/lost-and-found" element={<LostAndFound />} />
        <Route path="/more" element={<More />} />
        <Route path="/more/class-role" element={<MyClassRole />} />
      </Route>
    </Routes>
  );
}
