import { CONTACT_EMAIL, LINK_CLASS } from '@/lib/legal';

export function ContactLink() {
  return (
    <a href={`mailto:${CONTACT_EMAIL}`} className={LINK_CLASS}>
      {CONTACT_EMAIL}
    </a>
  );
}
