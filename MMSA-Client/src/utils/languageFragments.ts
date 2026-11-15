const LANGUAGE_LIST = String.raw`[A-Za-z0-9_+#-]+(?:\s*,\s*[A-Za-z0-9_+#-]+)*`;
const VARIANT = String.raw`\{\{\s*(?:${LANGUAGE_LIST})\s*:[\s\S]*?\}\}`;
const RUN_PATTERN = new RegExp(String.raw`${VARIANT}(?:\s*${VARIANT})*`, "g");
const VARIANT_SOURCE = String.raw`\{\{\s*(${LANGUAGE_LIST})\s*:([\s\S]*?)\}\}`;

type Variant = { codes: string[]; value: string };

const parseVariants = (run: string): Variant[] => {
  const pattern = new RegExp(VARIANT_SOURCE, "g");
  const variants: Variant[] = [];
  let match = pattern.exec(run);
  while (match) {
    variants.push({
      codes: match[1].split(",").map((code) => code.trim().toLowerCase()),
      value: match[2].trim(),
    });
    match = pattern.exec(run);
  }
  return variants;
};

export const resolveLanguageFragments = (html: string, language: string): string => {
  const code = language.toLowerCase();
  return html.replace(RUN_PATTERN, (run) => {
    const variants = parseVariants(run);
    const chosen = variants.find((variant) => variant.codes.includes(code)) ?? variants[0];
    return chosen ? `<code class="code-fragment">${chosen.value}</code>` : run;
  });
};

export const LANGUAGE_FRAGMENT_EXAMPLE = "{{csharp,cpp:a % b}}{{pascal:a mod b}}";
