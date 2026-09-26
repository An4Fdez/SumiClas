import { ShieldCheck } from 'lucide-react';
import { PlaceholderSection } from '@/components/app/PlaceholderSection';

export default function Privacy() {
  return (
    <section className="mx-auto max-w-3xl px-4 pb-16 pt-28 sm:px-6 lg:px-8">
      <PlaceholderSection
        icon={ShieldCheck}
        color="mint"
        title="Política de privacidad"
        description="Aquí irá la política de privacidad de SumiClas, incluyendo el tratamiento de datos de menores."
      />
    </section>
  );
}
