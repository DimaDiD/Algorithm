import hljs from "highlight.js/lib/core";
import type { LanguageFn } from "highlight.js";
import csharp from "highlight.js/lib/languages/csharp";
import cpp from "highlight.js/lib/languages/cpp";
import delphi from "highlight.js/lib/languages/delphi";

hljs.registerLanguage("csharp", csharp);
hljs.registerLanguage("cpp", cpp);
hljs.registerLanguage("delphi", delphi);

type Loader = () => Promise<{ default: LanguageFn }>;

const LOADERS: Record<string, Loader> = {
  c: () => import("highlight.js/lib/languages/c"),
  python: () => import("highlight.js/lib/languages/python"),
  java: () => import("highlight.js/lib/languages/java"),
  javascript: () => import("highlight.js/lib/languages/javascript"),
  typescript: () => import("highlight.js/lib/languages/typescript"),
  go: () => import("highlight.js/lib/languages/go"),
  kotlin: () => import("highlight.js/lib/languages/kotlin"),
  rust: () => import("highlight.js/lib/languages/rust"),
  php: () => import("highlight.js/lib/languages/php"),
  ruby: () => import("highlight.js/lib/languages/ruby"),
  swift: () => import("highlight.js/lib/languages/swift"),
  sql: () => import("highlight.js/lib/languages/sql"),
  basic: () => import("highlight.js/lib/languages/basic"),
  haskell: () => import("highlight.js/lib/languages/haskell"),
  scala: () => import("highlight.js/lib/languages/scala"),
  dart: () => import("highlight.js/lib/languages/dart"),
};

export const HIGHLIGHT_OPTIONS = [
  { value: "plaintext", label: "Без підсвітки" },
  { value: "csharp", label: "C#" },
  { value: "cpp", label: "C++" },
  { value: "c", label: "C" },
  { value: "delphi", label: "Pascal / Delphi" },
  { value: "python", label: "Python" },
  { value: "java", label: "Java" },
  { value: "javascript", label: "JavaScript" },
  { value: "typescript", label: "TypeScript" },
  { value: "go", label: "Go" },
  { value: "kotlin", label: "Kotlin" },
  { value: "rust", label: "Rust" },
  { value: "php", label: "PHP" },
  { value: "ruby", label: "Ruby" },
  { value: "swift", label: "Swift" },
  { value: "sql", label: "SQL" },
  { value: "basic", label: "Basic" },
  { value: "haskell", label: "Haskell" },
  { value: "scala", label: "Scala" },
  { value: "dart", label: "Dart" },
];

const pending = new Map<string, Promise<void>>();

export const isHighlightReady = (name: string) => name === "plaintext" || !!hljs.getLanguage(name) || !LOADERS[name];

export const ensureHighlightLanguage = (name: string): Promise<void> => {
  if (isHighlightReady(name)) return Promise.resolve();

  let promise = pending.get(name);
  if (!promise) {
    promise = LOADERS[name]()
      .then((module) => {
        hljs.registerLanguage(name, module.default);
      })
      .catch(() => undefined);
    pending.set(name, promise);
  }
  return promise;
};

const escapeHtml = (value: string) => value.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");

export const highlightCode = (code: string, name: string): string => {
  if (name === "plaintext" || !hljs.getLanguage(name)) return escapeHtml(code);
  try {
    return hljs.highlight(code, { language: name, ignoreIllegals: true }).value;
  } catch {
    return escapeHtml(code);
  }
};
