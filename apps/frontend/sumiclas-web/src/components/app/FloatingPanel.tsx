import { useEffect, useLayoutEffect, useRef } from 'react';
import type { ReactNode, RefObject } from 'react';
import { createPortal } from 'react-dom';

const PANEL_GAP = 8;

type FloatingPanelProps = {
  triggerRef: RefObject<HTMLElement>;
  onClose: () => void;
  minWidth?: number;
  className?: string;
  children: ReactNode;
};

export function FloatingPanel({ triggerRef, onClose, minWidth = 0, className = '', children }: FloatingPanelProps) {
  const panelRef = useRef<HTMLDivElement>(null);
  const onCloseRef = useRef(onClose);
  onCloseRef.current = onClose;

  // Tailwind cannot generate classes for coordinates only known at runtime, so they travel as CSS variables.
  useLayoutEffect(() => {
    const trigger = triggerRef.current;
    const panel = panelRef.current;
    if (!trigger || !panel) return;
    const rect = trigger.getBoundingClientRect();
    panel.style.setProperty('--panel-top', `${rect.bottom + PANEL_GAP}px`);
    panel.style.setProperty('--panel-left', `${rect.left}px`);
    panel.style.setProperty('--panel-width', `${Math.max(rect.width, minWidth)}px`);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  useEffect(() => {
    const handlePointerDown = (e: MouseEvent) => {
      const target = e.target as Node;
      if (triggerRef.current?.contains(target) || panelRef.current?.contains(target)) return;
      onCloseRef.current();
    };
    const handleScroll = (e: Event) => {
      if (panelRef.current?.contains(e.target as Node)) return;
      onCloseRef.current();
    };
    const handleResize = () => onCloseRef.current();
    document.addEventListener('mousedown', handlePointerDown);
    window.addEventListener('scroll', handleScroll, true);
    window.addEventListener('resize', handleResize);
    return () => {
      document.removeEventListener('mousedown', handlePointerDown);
      window.removeEventListener('scroll', handleScroll, true);
      window.removeEventListener('resize', handleResize);
    };
  }, [triggerRef]);

  return createPortal(
    <div
      ref={panelRef}
      className={`fixed left-[var(--panel-left)] top-[var(--panel-top)] w-[var(--panel-width)] ${className}`}
    >
      {children}
    </div>,
    document.body
  );
}
