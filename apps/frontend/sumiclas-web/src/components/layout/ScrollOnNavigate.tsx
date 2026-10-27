import { useEffect } from 'react';
import { useLocation, useNavigationType } from 'react-router-dom';

export function ScrollOnNavigate() {
  const { hash, key } = useLocation();
  const navigationType = useNavigationType();

  useEffect(() => {
    const reduceMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;

    if (!hash) {
      // Back and forward keep the position the browser restores.
      if (navigationType !== 'POP') window.scrollTo({ top: 0, behavior: 'instant' });
      return;
    }

    const target = document.getElementById(decodeURIComponent(hash.slice(1)));
    if (!target) return;
    const navbarHeight = document.querySelector('header')?.getBoundingClientRect().height ?? 0;
    window.scrollTo({
      top: target.getBoundingClientRect().top + window.scrollY - navbarHeight,
      behavior: reduceMotion ? 'instant' : 'smooth',
    });
  }, [hash, key, navigationType]);

  return null;
}
