import { useMemo, useState } from "react";
import { Alert, Button, Input, Select, Tag } from "antd";
import { ArrowDownOutlined, ArrowUpOutlined, CloseOutlined, PlusOutlined } from "@ant-design/icons";
import { useTable } from "../../Store";
import LanguageService from "../../services/languageService";
import { HIGHLIGHT_OPTIONS } from "../../course/highlight";
import { getErrorMessages } from "../../auth/authApi";

type Props = {
  value?: string[];
  onChange?: (value: string[]) => void;
};

const CODE_PATTERN = /^[a-z][a-z0-9-]{1,29}$/;

const highlightLabel = (value: string) => HIGHLIGHT_OPTIONS.find((option) => option.value === value)?.label ?? value;

const CourseLanguagePicker = ({ value = [], onChange }: Props) => {
  const [state, actions] = useTable();
  const [isAdding, setIsAdding] = useState(false);
  const [draft, setDraft] = useState({ Name: "", Code: "", HighlightLanguage: "plaintext" });
  const [errors, setErrors] = useState<string[]>([]);
  const [isSaving, setIsSaving] = useState(false);

  const byCode = useMemo(() => new Map(state.languages.map((language) => [language.Code, language])), [state.languages]);
  const available = state.languages.filter((language) => !value.includes(language.Code));

  const update = (next: string[]) => onChange?.(next);

  const move = (index: number, offset: number) => {
    const next = [...value];
    [next[index], next[index + offset]] = [next[index + offset], next[index]];
    update(next);
  };

  const resetDraft = () => {
    setDraft({ Name: "", Code: "", HighlightLanguage: "plaintext" });
    setErrors([]);
    setIsAdding(false);
  };

  const createLanguage = async () => {
    const name = draft.Name.trim();
    const code = draft.Code.trim().toLowerCase();
    const problems = [
      ...(name ? [] : ["Введіть назву мови."]),
      ...(CODE_PATTERN.test(code) ? [] : ["Код: латиниця, цифри, дефіс, починається з літери (наприклад, python)."]),
    ];
    if (problems.length > 0) {
      setErrors(problems);
      return;
    }

    setIsSaving(true);
    setErrors([]);
    try {
      const created = await LanguageService.create({ Name: name, Code: code, HighlightLanguage: draft.HighlightLanguage });
      await actions.loadLanguages();
      update([...value, created.Code]);
      resetDraft();
    } catch (error) {
      setErrors(getErrorMessages(error));
    } finally {
      setIsSaving(false);
    }
  };

  return (
    <div className="course-languages">
      {value.length === 0 ? (
        <p className="course-languages__empty">Оберіть хоча б одну мову.</p>
      ) : (
        <ul className="language-list">
          {value.map((code, index) => {
            const language = byCode.get(code);
            return (
              <li key={code} className="language-row">
                <div className="language-row__info">
                  <span className="language-row__name">
                    {language?.Name ?? code}
                    {index === 0 && <span className="language-row__default">за замовчуванням</span>}
                  </span>
                  <span className="language-row__meta">
                    <Tag>{code}</Tag>
                    {language && `підсвітка: ${highlightLabel(language.HighlightLanguage)}`}
                  </span>
                </div>
                <div className="language-row__tools">
                  <Button size="small" icon={<ArrowUpOutlined />} disabled={index === 0} onClick={() => move(index, -1)} aria-label="Вище" />
                  <Button
                    size="small"
                    icon={<ArrowDownOutlined />}
                    disabled={index === value.length - 1}
                    onClick={() => move(index, 1)}
                    aria-label="Нижче"
                  />
                  <Button
                    size="small"
                    icon={<CloseOutlined />}
                    disabled={value.length === 1}
                    onClick={() => update(value.filter((item) => item !== code))}
                    aria-label={`Прибрати ${language?.Name ?? code}`}
                  />
                </div>
              </li>
            );
          })}
        </ul>
      )}

      <div className="course-languages__add">
        <Select
          value={null}
          placeholder="Додати мову з каталогу"
          options={available.map((language) => ({ label: language.Name, value: language.Code }))}
          onChange={(code: string) => update([...value, code])}
          disabled={available.length === 0}
          showSearch
          optionFilterProp="label"
          style={{ flex: "1 1 220px" }}
        />
        {!isAdding && (
          <Button icon={<PlusOutlined />} onClick={() => setIsAdding(true)}>
            Нова мова
          </Button>
        )}
      </div>

      {isAdding && (
        <div className="language-form">
          <h4 className="language-form__title">Нова мова в каталозі</h4>
          {errors.length > 0 && (
            <Alert type="error" showIcon style={{ marginBottom: 12 }} message={errors.map((error) => <div key={error}>{error}</div>)} />
          )}
          <div className="question-editor__row">
            <label className="field" style={{ flex: "1 1 160px" }}>
              <span className="field__label">Назва</span>
              <Input value={draft.Name} placeholder="Python" maxLength={50} onChange={(e) => setDraft({ ...draft, Name: e.target.value })} />
            </label>
            <label className="field" style={{ flex: "1 1 140px" }}>
              <span className="field__label">Код</span>
              <Input value={draft.Code} placeholder="python" maxLength={30} onChange={(e) => setDraft({ ...draft, Code: e.target.value })} />
            </label>
            <label className="field" style={{ flex: "1 1 180px" }}>
              <span className="field__label">Підсвітка синтаксису</span>
              <Select
                value={draft.HighlightLanguage}
                options={HIGHLIGHT_OPTIONS}
                showSearch
                optionFilterProp="label"
                onChange={(highlight: string) => setDraft({ ...draft, HighlightLanguage: highlight })}
              />
            </label>
          </div>
          <p className="language-form__hint">Мова з'явиться в каталозі для всіх викладачів. Код змінити буде неможливо.</p>
          <div className="exercise-editor__footer" style={{ marginTop: 0 }}>
            <Button onClick={resetDraft}>Скасувати</Button>
            <Button type="primary" loading={isSaving} onClick={createLanguage}>
              Додати до курсу
            </Button>
          </div>
        </div>
      )}
    </div>
  );
};

export default CourseLanguagePicker;
