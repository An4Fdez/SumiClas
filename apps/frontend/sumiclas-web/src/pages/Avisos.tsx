import { Bell } from 'lucide-react';
import { PlaceholderSection } from '@/components/app/PlaceholderSection';

export default function Avisos() {
  return (
    <PlaceholderSection
      icon={Bell}
      color="rose"
      title="Avisos"
      description="Los avisos de la clase, ordenados por fecha y con lo prioritario destacado arriba."
    />
  );
}
