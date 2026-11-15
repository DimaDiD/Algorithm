import { CSSProperties, KeyboardEvent, useEffect, useRef, useState } from "react";
import { useTable } from "../../Store";
import { useCodeOption, useCourseLanguages } from "../../course/useCourse";

const EDGE_OFFSET_PX = 24;

const KEY_STEPS: Record<string, number> = { ArrowUp: -1, ArrowLeft: -1, ArrowDown: 1, ArrowRight: 1 };

const useFooterLift = (element: HTMLElement | null) => {
  useEffect(() => {
    const footer = document.querySelector<HTMLElement>(".app-footer");
    if (!element || !footer) return;

    let frame = 0;
    const update = () => {
      frame = 0;
      const overlap = Math.max(0, window.innerHeight - footer.getBoundingClientRect().top);
      element.style.setProperty("--lang-dock-bottom", `${EDGE_OFFSET_PX + overlap}px`);
    };
    const schedule = () => {
      if (!frame) frame = window.requestAnimationFrame(update);
    };

    update();
    const observer = new ResizeObserver(schedule);
    observer.observe(document.body);
    window.addEventListener("scroll", schedule, { passive: true });
    window.addEventListener("resize", schedule);
    return () => {
      observer.disconnect();
      window.removeEventListener("scroll", schedule);
      window.removeEventListener("resize", schedule);
      if (frame) window.cancelAnimationFrame(frame);
    };
  }, [element]);
};

const LanguageDock = () => {
  const [, actions] = useTable();
  const languages = useCourseLanguages();
  const value = useCodeOption();
  const [dock, setDock] = useState<HTMLDivElement | null>(null);
  const buttons = useRef<(HTMLButtonElement | null)[]>([]);
  const isVisible = languages.length > 1;

  useFooterLift(dock);

  if (!isVisible) return null;

  const selectedIndex = Math.max(0, languages.findIndex((language) => language.Code === value));

  const select = (index: number, shouldFocus = false) => {
    const nextIndex = (index + languages.length) % languages.length;
    actions.setCodeOption(languages[nextIndex].Code);
    if (shouldFocus) buttons.current[nextIndex]?.focus();
  };

  const handleKeyDown = (event: KeyboardEvent<HTMLDivElement>) => {
    let target: number | undefined;
    if (event.key in KEY_STEPS) target = selectedIndex + KEY_STEPS[event.key];
    if (event.key === "Home") target = 0;
    if (event.key === "End") target = languages.length - 1;
    if (target === undefined) return;
    event.preventDefault();
    select(target, true);
  };

  return (
    <div
      ref={setDock}
      className="lang-dock"
      style={{ "--lang-dock-index": selectedIndex } as CSSProperties}
    >
      <span className="lang-dock__label" id="lang-dock-label">Мова коду</span>
      <div className="lang-dock__options" role="radiogroup" aria-labelledby="lang-dock-label" onKeyDown={handleKeyDown}>
        <span className="lang-dock__thumb" aria-hidden="true" />
        {languages.map((language, index) => {
          const isActive = index === selectedIndex;
          return (
            <button
              key={language.Code}
              ref={(element) => {
                buttons.current[index] = element;
              }}
              type="button"
              role="radio"
              aria-checked={isActive}
              tabIndex={isActive ? 0 : -1}
              className={`lang-dock__option${isActive ? " is-active" : ""}`}
              onClick={() => select(index)}
            >
              {language.Name}
            </button>
          );
        })}
      </div>
    </div>
  );
};

export default LanguageDock;
