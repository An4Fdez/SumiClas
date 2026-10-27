import { Navbar } from './Navbar';
import { Footer } from './Footer';
import { RoutedOutlet } from './RoutedOutlet';
import { ScrollOnNavigate } from './ScrollOnNavigate';
import { ScrollToTopButton } from './ScrollToTopButton';

export function Layout() {
  return (
    <div className="min-h-screen bg-white text-slate-900 antialiased dark:bg-slate-950 dark:text-slate-100">
      <ScrollOnNavigate />
      <Navbar />
      <main>
        <RoutedOutlet />
      </main>
      <Footer />
      <ScrollToTopButton />
    </div>
  );
}
