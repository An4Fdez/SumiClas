import { Wallet } from 'lucide-react';
import { PlaceholderSection } from '@/components/app/PlaceholderSection';

export default function Gastos() {
  return (
    <PlaceholderSection
      icon={Wallet}
      color="sun"
      title="Gastos de clase"
      description="Regalo colectivo, material… quién participa y quién ya ha pagado su parte, sin mover dinero."
    />
  );
}
