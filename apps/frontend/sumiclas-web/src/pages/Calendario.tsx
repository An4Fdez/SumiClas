import { Calendar } from 'lucide-react';
import { PlaceholderSection } from '@/components/app/PlaceholderSection';

export default function Calendario() {
  return (
    <PlaceholderSection
      icon={Calendar}
      color="violet"
      title="Calendario"
      description="Excursiones, reuniones y actividades de la clase, todo en un mismo sitio."
    />
  );
}
