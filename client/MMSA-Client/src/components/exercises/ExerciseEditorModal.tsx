import { useEffect, useState } from "react";
import { Alert, Button, Checkbox, Form, Input, InputNumber, Modal, Select } from "antd";
import { ArrowDownOutlined, ArrowUpOutlined, DeleteOutlined, PlusOutlined } from "@ant-design/icons";
import { AiStatus, ExerciseEdit, QuestionType } from "../../models/Exercise";
import ExerciseService from "../../services/exerciseService";
import { getErrorMessages } from "../../auth/authApi";
import { useCourseLanguages } from "../../course/useCourse";
import { useTable } from "../../Store";
import CodeEditor from "./LazyCodeEditor";

type OptionValues = {
  Id?: number;
  Text?: string;
  IsCorrect?: boolean;
};

type QuestionValues = {
  Id?: number;
  Type: QuestionType;
  Text?: string;
  Points?: number;
  ReferenceAnswer?: string;
  Rubric?: string;
  CodeLanguage?: string;
  StarterCode?: string;
  Options?: OptionValues[];
};

type FormValues = {
  Title?: string;
  Description?: string;
  Questions?: QuestionValues[];
};

type Props = {
  open: boolean;
  pageId: number;
  subPageId?: number;
  exercise?: ExerciseEdit;
  onCancel: () => void;
  onSaved: () => void;
};

const QUESTION_TYPES: { label: string; value: QuestionType }[] = [
  { label: "Один правильний варіант", value: "SingleChoice" },
  { label: "Кілька правильних варіантів", value: "MultipleChoice" },
  { label: "Коротка відповідь", value: "ShortAnswer" },
  { label: "Розгорнута відповідь (перевіряє AI)", value: "OpenAnswer" },
  { label: "Код (перевіряє AI)", value: "Code" },
];

const isAiType = (type?: QuestionType) => type === "OpenAnswer" || type === "Code";

const trimCode = (value?: string) => (value && value.trim() ? value.replace(/\s+$/, "") : null);

const newQuestion = (): QuestionValues => ({
  Type: "SingleChoice",
  Points: 1,
  Text: "",
  Options: [
    { Text: "", IsCorrect: true },
    { Text: "", IsCorrect: false },
  ],
});

const toFormValues = (exercise?: ExerciseEdit): FormValues =>
  exercise
    ? {
        Title: exercise.Title,
        Description: exercise.Description ?? "",
        Questions: exercise.Questions.map((question) => ({
          Id: question.Id,
          Type: question.Type,
          Text: question.Text,
          Points: question.Points,
          ReferenceAnswer: question.ReferenceAnswer ?? "",
          Rubric: question.Rubric ?? "",
          CodeLanguage: question.CodeLanguage ?? undefined,
          StarterCode: question.StarterCode ?? "",
          Options: question.Options.map((option) => ({ Id: option.Id, Text: option.Text, IsCorrect: option.IsCorrect })),
        })),
      }
    : { Title: "", Description: "", Questions: [newQuestion()] };

type QuestionFieldsProps = {
  name: number;
  index: number;
  total: number;
  onMove: (from: number, to: number) => void;
  onRemove: () => void;
};

const QuestionFields = ({ name, index, total, onMove, onRemove }: QuestionFieldsProps) => {
  const form = Form.useFormInstance<FormValues>();
  const type: QuestionType | undefined = Form.useWatch(["Questions", name, "Type"], form);
  const isShortAnswer = type === "ShortAnswer";
  const isOpenAnswer = type === "OpenAnswer";
  const isCode = type === "Code";
  const [state] = useTable();
  const courseLanguages = useCourseLanguages();
  const codeLanguage: string | undefined = Form.useWatch(["Questions", name, "CodeLanguage"], form);
  const highlightLanguage =
    state.languages.find((language) => language.Code === codeLanguage)?.HighlightLanguage ?? "plaintext";

  useEffect(() => {
    if (isCode && !codeLanguage && courseLanguages.length > 0) {
      form.setFieldValue(["Questions", name, "CodeLanguage"], courseLanguages[0].Code);
    }
  }, [isCode, codeLanguage, courseLanguages, form, name]);

  return (
    <div className="question-editor">
      <div className="question-editor__head">
        <span className="question-editor__number">Питання {index + 1}</span>
        <div className="question-editor__tools">
          <Button size="small" icon={<ArrowUpOutlined />} disabled={index === 0} onClick={() => onMove(index, index - 1)} aria-label="Перемістити вище" />
          <Button size="small" icon={<ArrowDownOutlined />} disabled={index === total - 1} onClick={() => onMove(index, index + 1)} aria-label="Перемістити нижче" />
          <Button size="small" danger icon={<DeleteOutlined />} disabled={total === 1} onClick={onRemove} aria-label="Видалити питання" />
        </div>
      </div>

      <Form.Item name={[name, "Id"]} hidden>
        <Input />
      </Form.Item>

      <div className="question-editor__row">
        <Form.Item name={[name, "Type"]} label="Тип" style={{ flex: "1 1 260px" }}>
          <Select options={QUESTION_TYPES} />
        </Form.Item>
        <Form.Item name={[name, "Points"]} label="Бали" style={{ flex: "0 0 120px" }} rules={[{ required: true, message: "Вкажіть бали" }]}>
          <InputNumber min={1} max={100} style={{ width: "100%" }} />
        </Form.Item>
      </div>

      <Form.Item
        name={[name, "Text"]}
        label="Текст питання"
        rules={[{ required: true, whitespace: true, message: "Введіть текст питання" }]}
      >
        <Input.TextArea autoSize={{ minRows: 2, maxRows: 8 }} maxLength={4000} />
      </Form.Item>

      {isOpenAnswer && (
        <div className="question-editor__ai">
          <Form.Item
            name={[name, "ReferenceAnswer"]}
            label="Еталонна відповідь"
            extra="Що має бути у правильній відповіді. Учні її не бачать."
          >
            <Input.TextArea autoSize={{ minRows: 2, maxRows: 8 }} maxLength={4000} />
          </Form.Item>
          <Form.Item
            name={[name, "Rubric"]}
            label="Критерії оцінювання"
            extra="Наприклад: «1 бал — названо умову виходу з циклу; 1 бал — пояснено, навіщо лічильник»."
          >
            <Input.TextArea autoSize={{ minRows: 2, maxRows: 8 }} maxLength={4000} />
          </Form.Item>
        </div>
      )}

      {isCode && (
        <div className="question-editor__ai">
          <Form.Item
            name={[name, "CodeLanguage"]}
            label="Мова програмування"
            extra="Лише мови цього курсу."
            rules={[{ required: true, message: "Оберіть мову" }]}
          >
            <Select
              options={courseLanguages.map((language) => ({ label: language.Name, value: language.Code }))}
              style={{ maxWidth: 260 }}
            />
          </Form.Item>
          <Form.Item
            name={[name, "StarterCode"]}
            label="Заготовка коду (необов'язково)"
            extra="З неї учень почне: наприклад, оголошення змінних або сигнатура методу."
          >
            <CodeEditor highlightLanguage={highlightLanguage} minLines={4} maxLength={4000} ariaLabel="Заготовка коду" />
          </Form.Item>
          <Form.Item
            name={[name, "ReferenceAnswer"]}
            label="Еталонне рішення"
            extra="Учні його не бачать. Інший правильний підхід AI теж зарахує."
          >
            <CodeEditor highlightLanguage={highlightLanguage} minLines={6} maxLength={4000} ariaLabel="Еталонне рішення" />
          </Form.Item>
          <Form.Item
            name={[name, "Rubric"]}
            label="Критерії оцінювання"
            extra="Наприклад: «2 бали — правильний алгоритм; 1 бал — обробка n = 1; 1 бал — зрозумілі імена змінних»."
          >
            <Input.TextArea autoSize={{ minRows: 2, maxRows: 8 }} maxLength={4000} />
          </Form.Item>
        </div>
      )}

      {!isAiType(type) && (
        <Form.List name={[name, "Options"]}>
          {(fields, { add, remove }) => (
            <div className="question-editor__options">
              <span className="question-editor__label">
                {isShortAnswer ? "Правильні відповіді (регістр і пробіли не враховуються)" : "Варіанти відповіді — позначте правильні"}
              </span>
              {fields.map((field) => (
                <div key={field.key} className="question-editor__option">
                  <Form.Item name={[field.name, "Id"]} hidden>
                    <Input />
                  </Form.Item>
                  {!isShortAnswer && (
                    <Form.Item name={[field.name, "IsCorrect"]} valuePropName="checked" noStyle>
                      <Checkbox aria-label="Правильний варіант" />
                    </Form.Item>
                  )}
                  <Form.Item
                    name={[field.name, "Text"]}
                    noStyle
                    rules={[{ required: true, whitespace: true, message: "Варіант не може бути порожнім" }]}
                  >
                    <Input placeholder={isShortAnswer ? "Правильна відповідь" : `Варіант ${field.name + 1}`} maxLength={1000} />
                  </Form.Item>
                  <Button
                    type="text"
                    icon={<DeleteOutlined />}
                    onClick={() => remove(field.name)}
                    disabled={fields.length <= 1}
                    aria-label="Видалити варіант"
                  />
                </div>
              ))}
              {fields.length < 10 && (
                <Button type="dashed" icon={<PlusOutlined />} onClick={() => add({ Text: "", IsCorrect: isShortAnswer })}>
                  {isShortAnswer ? "Ще одна правильна відповідь" : "Додати варіант"}
                </Button>
              )}
            </div>
          )}
        </Form.List>
      )}
    </div>
  );
};

const ExerciseEditorModal = ({ open, pageId, subPageId, exercise, onCancel, onSaved }: Props) => {
  const [form] = Form.useForm<FormValues>();
  const [errors, setErrors] = useState<string[]>([]);
  const [isSaving, setIsSaving] = useState(false);
  const [aiStatus, setAiStatus] = useState<AiStatus | null>(null);
  const questions: QuestionValues[] | undefined = Form.useWatch("Questions", form);
  const hasOpenAnswers = (questions ?? []).some((question) => isAiType(question?.Type));

  useEffect(() => {
    if (!open) return;
    ExerciseService.getAiStatus()
      .then(setAiStatus)
      .catch(() => setAiStatus(null));
  }, [open]);

  useEffect(() => {
    if (open) {
      form.resetFields();
      form.setFieldsValue(toFormValues(exercise));
      setErrors([]);
    }
  }, [open, exercise, form]);

  const handleFinish = async (values: FormValues) => {
    const payload: ExerciseEdit = {
      Id: exercise?.Id ?? 0,
      PageId: pageId,
      SubPageId: subPageId ?? null,
      Title: values.Title ?? "",
      Description: values.Description || null,
      Order: exercise?.Order ?? 0,
      Questions: (values.Questions ?? []).map((question) => ({
        Id: question.Id ?? 0,
        Type: question.Type,
        Text: question.Text ?? "",
        Points: question.Points ?? 1,
        CodeLanguage: question.Type === "Code" ? question.CodeLanguage ?? null : null,
        StarterCode: question.Type === "Code" ? trimCode(question.StarterCode) : null,
        ReferenceAnswer:
          question.Type === "Code"
            ? trimCode(question.ReferenceAnswer)
            : question.Type === "OpenAnswer"
              ? question.ReferenceAnswer?.trim() || null
              : null,
        Rubric: isAiType(question.Type) ? question.Rubric?.trim() || null : null,
        Options: (isAiType(question.Type) ? [] : question.Options ?? []).map((option) => ({
          Id: option.Id ?? 0,
          Text: option.Text ?? "",
          IsCorrect: question.Type === "ShortAnswer" || !!option.IsCorrect,
        })),
      })),
    };

    setIsSaving(true);
    setErrors([]);
    try {
      if (payload.Id === 0) await ExerciseService.create(payload);
      else await ExerciseService.update(payload);
      onSaved();
    } catch (error) {
      setErrors(getErrorMessages(error));
    } finally {
      setIsSaving(false);
    }
  };

  return (
    <Modal
      title={exercise ? "Редагування завдання" : "Нове завдання"}
      open={open}
      onCancel={onCancel}
      width={860}
      footer={null}
      forceRender
      destroyOnClose={false}
    >
      <Form form={form} layout="vertical" requiredMark={false} onFinish={handleFinish}>
        <Form.Item name="Title" label="Назва" rules={[{ required: true, whitespace: true, message: "Введіть назву" }]}>
          <Input size="large" placeholder="Наприклад: Перевір себе — цикли" maxLength={200} />
        </Form.Item>
        <Form.Item name="Description" label="Опис (необов'язково)">
          <Input.TextArea autoSize={{ minRows: 1, maxRows: 4 }} maxLength={2000} />
        </Form.Item>

        <Form.List name="Questions">
          {(fields, { add, remove, move }) => (
            <div className="question-list">
              {fields.map((field, index) => (
                <QuestionFields
                  key={field.key}
                  name={field.name}
                  index={index}
                  total={fields.length}
                  onMove={move}
                  onRemove={() => remove(field.name)}
                />
              ))}
              {fields.length < 50 && (
                <Button type="dashed" size="large" icon={<PlusOutlined />} onClick={() => add(newQuestion())} block>
                  Додати питання
                </Button>
              )}
            </div>
          )}
        </Form.List>

        {hasOpenAnswers && aiStatus && !aiStatus.Enabled && (
          <Alert
            type="warning"
            showIcon
            style={{ marginTop: 16 }}
            message="AI-перевірка вимкнена на сервері"
            description="Розгорнуті відповіді й код учнів не будуть оцінені, доки адміністратор не налаштує ключ Claude API."
          />
        )}

        {errors.length > 0 && (
          <Alert
            type="error"
            showIcon
            style={{ marginTop: 16 }}
            message={errors.map((error) => <div key={error}>{error}</div>)}
          />
        )}

        <div className="exercise-editor__footer">
          <Button onClick={onCancel}>Скасувати</Button>
          <Button type="primary" htmlType="submit" loading={isSaving}>
            {exercise ? "Зберегти зміни" : "Створити завдання"}
          </Button>
        </div>
      </Form>
    </Modal>
  );
};

export default ExerciseEditorModal;
