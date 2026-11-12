import { useState } from "react";
import { Button, Input, InputNumber, Tag } from "antd";
import { CheckOutlined, EditOutlined, RobotOutlined, UserOutlined } from "@ant-design/icons";
import { ReviewItem, ReviewUpdate } from "../../models/Review";
import { useTable } from "../../Store";
import CodeEditor from "../exercises/LazyCodeEditor";

type Props = {
  item: ReviewItem;
  onReview: (update: ReviewUpdate) => Promise<void>;
};

const STATUS_TAGS = {
  Pending: { color: "gold", label: "Чекає перегляду" },
  Confirmed: { color: "green", label: "Погоджено з AI" },
  Overridden: { color: "purple", label: "Оцінку змінено" },
} as const;

const ISSUE_LINE = /^Рядок (\d+):/;

const formatDate = (value?: string | null) =>
  value
    ? new Date(value).toLocaleString("uk-UA", { day: "numeric", month: "short", hour: "2-digit", minute: "2-digit" })
    : "";

const ReviewCard = ({ item, onReview }: Props) => {
  const [state] = useTable();
  const [isEditing, setIsEditing] = useState(false);
  const [points, setPoints] = useState<number>(item.EarnedPoints);
  const [comment, setComment] = useState(item.TeacherComment ?? "");
  const [isSaving, setIsSaving] = useState(false);

  const isCode = item.QuestionType === "Code";
  const language = state.languages.find((entry) => entry.Code === item.CodeLanguage);
  const feedbackLines = (item.AiFeedback ?? "").split("\n").filter((line) => line.trim());
  const issueLines = feedbackLines
    .map((line) => ISSUE_LINE.exec(line))
    .filter((match): match is RegExpExecArray => match !== null)
    .map((match) => Number(match[1]));
  const status = STATUS_TAGS[item.ReviewStatus];

  const submit = async (update: ReviewUpdate) => {
    setIsSaving(true);
    try {
      await onReview(update);
      setIsEditing(false);
    } finally {
      setIsSaving(false);
    }
  };

  return (
    <article className="card review-card">
      <header className="review-card__head">
        <div className="review-card__meta">
          <span className="review-card__student">
            <UserOutlined /> {item.StudentLabel}
          </span>
          <span>{formatDate(item.SubmittedAtUtc)}</span>
          <span className="review-card__topic">
            {item.ExerciseTitle} · {item.TopicTitle}
          </span>
        </div>
        <Tag color={status.color}>{status.label}</Tag>
      </header>

      <p className="review-card__question">{item.QuestionText ?? "Питання видалено з завдання."}</p>

      {isCode ? (
        <div className="exercise-open">
          {language && <span className="exercise-code__language review-card__language">{language.Name}</span>}
          <CodeEditor
            value={item.AnswerText ?? ""}
            readOnly
            highlightLanguage={language?.HighlightLanguage ?? "plaintext"}
            markedLines={issueLines}
            minLines={3}
            ariaLabel={`Код учня ${item.StudentLabel}`}
          />
        </div>
      ) : (
        <blockquote className="review-card__answer">{item.AnswerText}</blockquote>
      )}

      {feedbackLines.length > 0 ? (
        <div className="exercise-feedback">
          <span className="exercise-feedback__title">
            <RobotOutlined /> Відгук AI · {item.AiPoints ?? "—"} з {item.MaxPoints}
          </span>
          {feedbackLines.map((line, index) => (
            <p key={index} className="exercise-feedback__text">
              {line}
            </p>
          ))}
        </div>
      ) : (
        <div className="exercise-feedback exercise-feedback--muted">
          <p className="exercise-feedback__text">AI не оцінив цю відповідь — поставте бали самостійно.</p>
        </div>
      )}

      {item.ReviewStatus !== "Pending" && !isEditing && (
        <div className="review-card__verdict">
          <span>
            Оцінка викладача: <strong>{item.EarnedPoints} з {item.MaxPoints}</strong>
          </span>
          {item.TeacherComment && <p className="review-card__comment">«{item.TeacherComment}»</p>}
        </div>
      )}

      {isEditing ? (
        <div className="review-card__form">
          <label className="field">
            <span className="field__label">Бали (0–{item.MaxPoints})</span>
            <InputNumber min={0} max={item.MaxPoints} value={points} onChange={(value) => setPoints(value ?? 0)} />
          </label>
          <label className="field" style={{ flex: "1 1 320px" }}>
            <span className="field__label">Коментар для учня (необов'язково)</span>
            <Input.TextArea
              value={comment}
              maxLength={1000}
              autoSize={{ minRows: 1, maxRows: 5 }}
              onChange={(event) => setComment(event.target.value)}
            />
          </label>
          <div className="review-card__actions">
            <Button onClick={() => setIsEditing(false)}>Скасувати</Button>
            <Button
              type="primary"
              loading={isSaving}
              onClick={() => submit({ Action: "override", Points: points, Comment: comment })}
            >
              Зберегти оцінку
            </Button>
          </div>
        </div>
      ) : (
        <div className="review-card__actions">
          {item.ReviewStatus === "Pending" && item.AiPoints != null && (
            <Button
              type="primary"
              icon={<CheckOutlined />}
              loading={isSaving}
              onClick={() => submit({ Action: "confirm", Comment: null })}
            >
              Погоджуюсь з AI ({item.AiPoints} б.)
            </Button>
          )}
          <Button icon={<EditOutlined />} onClick={() => setIsEditing(true)}>
            {item.ReviewStatus === "Pending" ? "Змінити оцінку" : "Переглянути ще раз"}
          </Button>
        </div>
      )}
    </article>
  );
};

export default ReviewCard;
