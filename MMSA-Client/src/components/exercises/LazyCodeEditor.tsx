import { lazy, Suspense } from "react";
import type { CodeEditorProps } from "./CodeEditor";

const CodeEditor = lazy(() => import("./CodeEditor"));

const LazyCodeEditor = (props: CodeEditorProps) => (
  <Suspense fallback={<div className="code-editor code-editor--loading" aria-busy="true" />}>
    <CodeEditor {...props} />
  </Suspense>
);

export default LazyCodeEditor;
