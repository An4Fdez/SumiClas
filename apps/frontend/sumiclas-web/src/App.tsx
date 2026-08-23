import { Navbar } from '@/components/Navbar';
import { Hero } from '@/components/Hero';
import { Problem } from '@/components/Problem';
import { SummarySection } from '@/components/SummarySection';
import { Features } from '@/components/Features';
import { SocialProof } from '@/components/SocialProof';
import { FinalCTA } from '@/components/FinalCTA';
import { Footer } from '@/components/Footer';

export default function App() {
  return (
    <div className="min-h-screen bg-white text-slate-900 antialiased dark:bg-slate-950 dark:text-slate-100">
      <Navbar />
      <main>
        <Hero />
        <Problem />
        <SummarySection />
        <Features />
        <SocialProof />
        <FinalCTA />
      </main>
      <Footer />
    </div>
  );
}
