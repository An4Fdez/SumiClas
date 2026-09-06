import { Routes, Route } from 'react-router-dom';
import { Layout } from '@/components/layout/Layout';
import { AppLayout } from '@/components/layout/AppLayout';
import Home from '@/pages/Home';
import Login from '@/pages/Login';
import Onboarding from '@/pages/Onboarding';
import Dashboard from '@/pages/Dashboard';
import Avisos from '@/pages/Avisos';
import Calendario from '@/pages/Calendario';
import Deberes from '@/pages/Deberes';
import Gastos from '@/pages/Gastos';
import ObjetosPerdidos from '@/pages/ObjetosPerdidos';
import Mas from '@/pages/Mas';
import RolClase from '@/pages/RolClase';

export default function App() {
  return (
    <Routes>
      <Route element={<Layout />}>
        <Route path="/" element={<Home />} />
        <Route path="/login" element={<Login />} />
        <Route path="/onboarding" element={<Onboarding />} />
      </Route>

      <Route element={<AppLayout />}>
        <Route path="/inicio" element={<Dashboard />} />
        <Route path="/avisos" element={<Avisos />} />
        <Route path="/calendario" element={<Calendario />} />
        <Route path="/deberes" element={<Deberes />} />
        <Route path="/gastos" element={<Gastos />} />
        <Route path="/objetos-perdidos" element={<ObjetosPerdidos />} />
        <Route path="/mas" element={<Mas />} />
        <Route path="/mas/rol" element={<RolClase />} />
      </Route>
    </Routes>
  );
}
