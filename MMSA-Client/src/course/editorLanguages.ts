import type { Extension } from "@codemirror/state";
import { StreamLanguage, StreamParser } from "@codemirror/language";

type Loader = () => Promise<Extension>;

const legacy = (parser: StreamParser<unknown>): Extension => StreamLanguage.define(parser);

const LOADERS: Record<string, Loader> = {
  csharp: async () => legacy((await import("@codemirror/legacy-modes/mode/clike")).csharp),
  kotlin: async () => legacy((await import("@codemirror/legacy-modes/mode/clike")).kotlin),
  scala: async () => legacy((await import("@codemirror/legacy-modes/mode/clike")).scala),
  dart: async () => legacy((await import("@codemirror/legacy-modes/mode/clike")).dart),
  delphi: async () => legacy((await import("@codemirror/legacy-modes/mode/pascal")).pascal),
  ruby: async () => legacy((await import("@codemirror/legacy-modes/mode/ruby")).ruby),
  swift: async () => legacy((await import("@codemirror/legacy-modes/mode/swift")).swift),
  haskell: async () => legacy((await import("@codemirror/legacy-modes/mode/haskell")).haskell),
  basic: async () => legacy((await import("@codemirror/legacy-modes/mode/vb")).vb),
  cpp: async () => (await import("@codemirror/lang-cpp")).cpp(),
  c: async () => (await import("@codemirror/lang-cpp")).cpp(),
  java: async () => (await import("@codemirror/lang-java")).java(),
  python: async () => (await import("@codemirror/lang-python")).python(),
  javascript: async () => (await import("@codemirror/lang-javascript")).javascript(),
  typescript: async () => (await import("@codemirror/lang-javascript")).javascript({ typescript: true }),
  go: async () => (await import("@codemirror/lang-go")).go(),
  rust: async () => (await import("@codemirror/lang-rust")).rust(),
  php: async () => (await import("@codemirror/lang-php")).php(),
  sql: async () => (await import("@codemirror/lang-sql")).sql(),
};

const cache = new Map<string, Promise<Extension>>();

export const loadEditorLanguage = (highlightLanguage: string): Promise<Extension> => {
  const loader = LOADERS[highlightLanguage];
  if (!loader) return Promise.resolve([]);

  const cached = cache.get(highlightLanguage);
  if (cached) return cached;

  const pending = loader().catch(() => {
    cache.delete(highlightLanguage);
    return [] as Extension;
  });
  cache.set(highlightLanguage, pending);
  return pending;
};
