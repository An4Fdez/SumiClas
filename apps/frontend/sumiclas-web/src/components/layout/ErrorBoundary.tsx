import { Component, type ErrorInfo, type ReactNode } from 'react';
import { ErrorFallback } from './ErrorFallback';

type Props = { children: ReactNode; resetKey?: string; fullScreen?: boolean };
type State = { failed: boolean };

/**
 * React only catches render errors in a class component. Errors in event handlers and in promises do not reach it: the
 * hooks already show those next to the action that failed. A change of `resetKey` clears the error, so navigating away
 * from a crashed page does not keep the fallback on screen. The fallback links with a plain anchor, not the router,
 * so it still works when the root boundary catches an error from the providers or the router itself.
 */
export class ErrorBoundary extends Component<Props, State> {
  state: State = { failed: false };

  static getDerivedStateFromError(): State {
    return { failed: true };
  }

  componentDidCatch(error: Error, info: ErrorInfo) {
    console.error('Render error caught by ErrorBoundary', error, info.componentStack);
  }

  componentDidUpdate(previous: Props) {
    if (this.state.failed && previous.resetKey !== this.props.resetKey) this.setState({ failed: false });
  }

  render() {
    if (this.state.failed) {
      return <ErrorFallback fullScreen={this.props.fullScreen} onRetry={() => this.setState({ failed: false })} />;
    }
    return this.props.children;
  }
}
