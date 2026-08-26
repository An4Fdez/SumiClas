import { PackageSearch } from 'lucide-react';
import { PlaceholderSection } from '@/components/app/PlaceholderSection';

export default function ObjetosPerdidos() {
  return (
    <PlaceholderSection
      icon={PackageSearch}
      color="orange"
      title="Objetos perdidos"
      description="Tablón de buscado / encontrado, visible para la clase o para todo el colegio."
    />
  );
}
