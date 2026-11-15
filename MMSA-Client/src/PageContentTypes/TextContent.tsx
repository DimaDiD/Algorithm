import { useMemo } from "react";
import { sanitizeHtml } from "../utils/sanitizeHtml";
import { resolveLanguageFragments } from "../utils/languageFragments";

type Props = {
  text: string;
  language?: string;
};

const TextContent = ({ text, language }: Props) => {
  const safeHtml = useMemo(
    () => sanitizeHtml(language ? resolveLanguageFragments(text, language) : text),
    [text, language]
  );
  return <div className="prose" dangerouslySetInnerHTML={{ __html: safeHtml }} />;
};

export default TextContent;
