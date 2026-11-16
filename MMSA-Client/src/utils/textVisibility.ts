export const ALL_LANGUAGES = "All";

export const parseVisibility = (value?: string | null): string[] =>
  (value ?? "")
    .split(",")
    .map((code) => code.trim())
    .filter((code) => code.length > 0);

export const serializeVisibility = (selected: string[]): string => {
  if (selected.length === 0) return "";
  if (selected[selected.length - 1] === ALL_LANGUAGES) return ALL_LANGUAGES;
  return selected.filter((code) => code !== ALL_LANGUAGES).join(",");
};

export const isVisibleFor = (value: string | null | undefined, language: string): boolean => {
  const codes = parseVisibility(value);
  return codes.includes(ALL_LANGUAGES) || codes.includes(language);
};
