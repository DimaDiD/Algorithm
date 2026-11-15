import { useEffect, useRef } from "react";
import { Compartment, EditorState, RangeSetBuilder } from "@codemirror/state";
import {
  Decoration,
  drawSelection,
  EditorView,
  highlightActiveLine,
  highlightActiveLineGutter,
  keymap,
  lineNumbers,
  placeholder as placeholderExtension,
} from "@codemirror/view";
import { defaultKeymap, history, historyKeymap, indentWithTab } from "@codemirror/commands";
import { bracketMatching, indentOnInput, indentUnit } from "@codemirror/language";
import { oneDark } from "@codemirror/theme-one-dark";
import { loadEditorLanguage } from "../../course/editorLanguages";

export type CodeEditorProps = {
  value?: string;
  onChange?: (value: string) => void;
  highlightLanguage?: string;
  readOnly?: boolean;
  markedLines?: number[];
  minLines?: number;
  maxLength?: number;
  placeholder?: string;
  ariaLabel?: string;
};

const markedLineDecoration = Decoration.line({ class: "cm-issue-line" });

const buildMarkedLines = (state: EditorState, lines: number[]) => {
  const builder = new RangeSetBuilder<Decoration>();
  Array.from(new Set(lines))
    .filter((line) => line >= 1 && line <= state.doc.lines)
    .sort((a, b) => a - b)
    .forEach((line) => {
      const { from } = state.doc.line(line);
      builder.add(from, from, markedLineDecoration);
    });
  return builder.finish();
};

const CodeEditor = ({
  value = "",
  onChange,
  highlightLanguage = "plaintext",
  readOnly = false,
  markedLines = [],
  minLines = 6,
  maxLength,
  placeholder,
  ariaLabel = "Редактор коду",
}: CodeEditorProps) => {
  const hostRef = useRef<HTMLDivElement>(null);
  const viewRef = useRef<EditorView | null>(null);
  const onChangeRef = useRef(onChange);
  const compartments = useRef({
    language: new Compartment(),
    readOnly: new Compartment(),
    marks: new Compartment(),
  });

  const initialConfig = useRef({ value, readOnly, minLines, maxLength, placeholder, ariaLabel });

  onChangeRef.current = onChange;

  useEffect(() => {
    if (!hostRef.current) return;
    const { language, readOnly: readOnlyCompartment, marks } = compartments.current;
    const config = initialConfig.current;

    const view = new EditorView({
      parent: hostRef.current,
      state: EditorState.create({
        doc: config.value,
        extensions: [
          lineNumbers(),
          highlightActiveLineGutter(),
          highlightActiveLine(),
          history(),
          drawSelection(),
          indentOnInput(),
          bracketMatching(),
          indentUnit.of("    "),
          EditorState.tabSize.of(4),
          keymap.of([...defaultKeymap, ...historyKeymap, indentWithTab]),
          oneDark,
          EditorView.contentAttributes.of({ "aria-label": config.ariaLabel }),
          config.placeholder ? placeholderExtension(config.placeholder) : [],
          config.maxLength
            ? EditorState.changeFilter.of((transaction) => transaction.newDoc.length <= (config.maxLength ?? Infinity))
            : [],
          EditorView.theme({ ".cm-content, .cm-gutter": { minHeight: `${config.minLines * 1.5}em` } }),
          EditorView.updateListener.of((update) => {
            if (update.docChanged) onChangeRef.current?.(update.state.doc.toString());
          }),
          language.of([]),
          readOnlyCompartment.of([EditorState.readOnly.of(config.readOnly), EditorView.editable.of(!config.readOnly)]),
          marks.of([]),
        ],
      }),
    });

    viewRef.current = view;
    return () => {
      view.destroy();
      viewRef.current = null;
    };
  }, []);

  useEffect(() => {
    const view = viewRef.current;
    if (view && value !== view.state.doc.toString()) {
      view.dispatch({ changes: { from: 0, to: view.state.doc.length, insert: value } });
    }
  }, [value]);

  useEffect(() => {
    let isActive = true;
    loadEditorLanguage(highlightLanguage).then((extension) => {
      if (isActive) viewRef.current?.dispatch({ effects: compartments.current.language.reconfigure(extension) });
    });
    return () => {
      isActive = false;
    };
  }, [highlightLanguage]);

  useEffect(() => {
    viewRef.current?.dispatch({
      effects: compartments.current.readOnly.reconfigure([
        EditorState.readOnly.of(readOnly),
        EditorView.editable.of(!readOnly),
      ]),
    });
  }, [readOnly]);

  const marksKey = markedLines.join(",");
  useEffect(() => {
    const view = viewRef.current;
    if (!view) return;
    const lines = marksKey ? marksKey.split(",").map(Number) : [];
    view.dispatch({
      effects: compartments.current.marks.reconfigure(
        lines.length === 0 ? [] : EditorView.decorations.of((view) => buildMarkedLines(view.state, lines))
      ),
    });
  }, [marksKey]);

  return <div ref={hostRef} className={`code-editor${readOnly ? " is-readonly" : ""}`} />;
};

export default CodeEditor;
