import { Component, ErrorInfo, ReactNode } from "react";

export type ErrorFallbackProps = {
  error: Error;
  reset: () => void;
};

type Props = {
  children: ReactNode;
  fallback: (props: ErrorFallbackProps) => ReactNode;
  resetKey?: unknown;
};

type State = {
  error: Error | null;
};

export const isChunkLoadError = (error: Error) =>
  error.name === "ChunkLoadError" || /loading (css )?chunk|dynamically imported module/i.test(error.message);

class ErrorBoundary extends Component<Props, State> {
  state: State = { error: null };

  static getDerivedStateFromError(error: Error): State {
    return { error };
  }

  componentDidCatch(error: Error, info: ErrorInfo) {
    console.error(error, info.componentStack);
  }

  componentDidUpdate(previous: Props) {
    if (this.state.error && previous.resetKey !== this.props.resetKey) this.reset();
  }

  reset = () => this.setState({ error: null });

  render() {
    const { error } = this.state;
    return error ? this.props.fallback({ error, reset: this.reset }) : this.props.children;
  }
}

export default ErrorBoundary;
