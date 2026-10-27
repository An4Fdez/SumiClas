import { Hero } from '@/components/landing/Hero';
import { Problem } from '@/components/landing/Problem';
import { SummarySection } from '@/components/landing/SummarySection';
import { Features } from '@/components/landing/Features';
import { SocialProof } from '@/components/landing/SocialProof';
import { Faq } from '@/components/landing/Faq';

export default function Home() {
  return (
    <>
      <Hero />
      <Problem />
      <SummarySection />
      <Features />
      <SocialProof />
      <Faq />
    </>
  );
}
