import { FileText } from 'lucide-react';
import { PlaceholderSection } from '@/components/app/PlaceholderSection';

export default function Terms() {
  return (
    <section className="mx-auto max-w-3xl px-4 pb-16 pt-28 sm:px-6 lg:px-8">
      <PlaceholderSection
        icon={FileText}
        color="brand"
        title="Términos de uso"
        description="Aquí irán las condiciones de uso de SumiClas."
      />
    </section>
  );
}
