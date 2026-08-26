import { Hero } from '@/components/Hero';
import { Problem } from '@/components/Problem';
import { SummarySection } from '@/components/SummarySection';
import { Features } from '@/components/Features';
import { SocialProof } from '@/components/SocialProof';
import { FinalCTA } from '@/components/FinalCTA';

export default function Home() {
  return (
    <>
      <Hero />
      <Problem />
      <SummarySection />
      <Features />
      <SocialProof />
      <FinalCTA />
    </>
  );
}
