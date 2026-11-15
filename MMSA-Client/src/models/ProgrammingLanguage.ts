export type ProgrammingLanguage = {
  Id: number;
  Code: string;
  Name: string;
  HighlightLanguage: string;
};

export const DEFAULT_LANGUAGES: ProgrammingLanguage[] = [
  { Id: 1, Code: "csharp", Name: "C#", HighlightLanguage: "csharp" },
  { Id: 2, Code: "cpp", Name: "C++", HighlightLanguage: "cpp" },
  { Id: 3, Code: "pascal", Name: "Pascal", HighlightLanguage: "delphi" },
];
