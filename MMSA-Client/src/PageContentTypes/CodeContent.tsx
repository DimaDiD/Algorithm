import { useEffect, useMemo, useRef, useState } from "react";
import { Popover } from "antd";
import { CheckOutlined, CopyOutlined, InfoCircleOutlined } from "@ant-design/icons";
import "highlight.js/styles/atom-one-dark.css";
import { useTable } from "../Store";
import { ensureHighlightLanguage, highlightCode, isHighlightReady } from "../course/highlight";

type Props = {
  code: string;
  language: string;
  showHeader?: boolean;
  topics?: string[];
};

const CodeContent = ({ code, language, showHeader = true, topics = [] }: Props) => {
  const [isCopied, setIsCopied] = useState(false);
  const resetTimer = useRef<number>();

  const [state] = useTable();
  const languageInfo = state.languages.find((item) => item.Code === language);
  const highlightName = languageInfo?.HighlightLanguage ?? language;
  const [isReady, setIsReady] = useState(() => isHighlightReady(highlightName));

  useEffect(() => {
    if (isHighlightReady(highlightName)) {
      setIsReady(true);
      return;
    }
    let isCancelled = false;
    setIsReady(false);
    ensureHighlightLanguage(highlightName).then(() => {
      if (!isCancelled) setIsReady(true);
    });
    return () => {
      isCancelled = true;
    };
  }, [highlightName]);

  const highlighted = useMemo(
    () => highlightCode(code, highlightName),
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [code, highlightName, isReady]
  );

  useEffect(() => () => window.clearTimeout(resetTimer.current), []);

  const copy = async () => {
    try {
      await navigator.clipboard.writeText(code);
      setIsCopied(true);
      window.clearTimeout(resetTimer.current);
      resetTimer.current = window.setTimeout(() => setIsCopied(false), 1500);
    } catch {
      setIsCopied(false);
    }
  };

  return (
    <figure className="code-block">
      {showHeader && (
        <figcaption className="code-block__bar">
          <span>{languageInfo?.Name ?? language}</span>
          <div className="code-block__actions">
            {topics.length > 0 && (
              <Popover
                trigger={["hover", "click"]}
                placement="bottomRight"
                title="Щоб зрозуміти цей код, варто знати:"
                content={
                  <ul className="topics-popover">
                    {topics.map((topic) => (
                      <li key={topic}>{topic}</li>
                    ))}
                  </ul>
                }
              >
                <button type="button" className="code-block__copy" aria-label="Які теми потрібно знати для цього коду">
                  <InfoCircleOutlined />
                  <span className="code-block__label">Теми · {topics.length}</span>
                </button>
              </Popover>
            )}
            <button type="button" className="code-block__copy" onClick={copy} aria-live="polite">
              {isCopied ? <CheckOutlined /> : <CopyOutlined />}
              <span className="code-block__label">{isCopied ? "Скопійовано" : "Копіювати"}</span>
            </button>
          </div>
        </figcaption>
      )}
      <pre>
        <code className={`hljs language-${language}`} dangerouslySetInnerHTML={{ __html: highlighted }} />
      </pre>
    </figure>
  );
};

export default CodeContent;
