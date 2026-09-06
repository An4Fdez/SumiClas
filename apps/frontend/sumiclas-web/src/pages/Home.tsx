import { Hero } from '@/components/landing/Hero';
import { Problem } from '@/components/landing/Problem';
import { SummarySection } from '@/components/landing/SummarySection';
import { Features } from '@/components/landing/Features';
import { SocialProof } from '@/components/landing/SocialProof';
import { FinalCTA } from '@/components/landing/FinalCTA';

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
