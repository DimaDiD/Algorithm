import { useMemo } from "react";
import { sanitizeHtml } from "../utils/sanitizeHtml";

const TextContent = ({ text }: { text: string }) => {
  const safeHtml = useMemo(() => sanitizeHtml(text), [text]);
  return <div className="prose" dangerouslySetInnerHTML={{ __html: safeHtml }} />;
};

export default TextContent;
