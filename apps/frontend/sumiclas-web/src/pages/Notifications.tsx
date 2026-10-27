import { Smartphone } from 'lucide-react';
import { PlaceholderSection } from '@/components/app/PlaceholderSection';

export default function Notifications() {
  return (
    <PlaceholderSection
      icon={Smartphone}
      color="brand"
      title="Notificaciones"
      description="Aquí podrás elegir qué notificaciones quieres recibir."
    />
  );
}
