import type { ReactNode } from 'react';
import { Link } from 'react-router-dom';
import { LINK_CLASS } from '@/lib/legal';

export function LegalLink({ to, children }: { to: string; children: ReactNode }) {
  return (
    <Link to={to} className={LINK_CLASS}>
      {children}
    </Link>
  );
}
