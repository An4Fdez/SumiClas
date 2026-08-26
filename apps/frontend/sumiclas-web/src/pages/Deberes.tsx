import { BookOpen } from 'lucide-react';
import { PlaceholderSection } from '@/components/app/PlaceholderSection';

export default function Deberes() {
  return (
    <PlaceholderSection
      icon={BookOpen}
      color="mint"
      title="Deberes"
      description="Pizarra organizada por día y asignatura, con hilo corto de comentarios para dudas puntuales."
    />
  );
}
