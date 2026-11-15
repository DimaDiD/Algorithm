import { useEffect, useMemo, useState } from "react";
import { Button, Checkbox, Input, Radio } from "antd";
import { Link } from "react-router-dom";
import {
  ArrowDownOutlined,
  ArrowUpOutlined,
  BarChartOutlined,
  CheckCircleFilled,
  CloseCircleFilled,
  DeleteOutlined,
  EditOutlined,
  MinusCircleFilled,
  ReloadOutlined,
  RobotOutlined,
  UserOutlined,
} from "@ant-design/icons";
import {
  Exercise,
  ExerciseCheckResult,
  ExerciseProgress,
  Question,
  QuestionAnswer,
  QuestionResult,
} from "../../models/Exercise";
import { pluralize } from "../../course/courseUtils";
import ExerciseService from "../../services/exerciseService";
import { useTable } from "../../Store";
import CodeEditor from "./LazyCodeEditor";
import notification from "../../notification/notificationMessage";
import { getErrorMessages } from "../../auth/authApi";

type AnswerState = {
  selected: number[];
  text: string;
};

type Props = {
  exercise: Exercise;
  isEditing: boolean;
  isCourseEditor: boolean;
  progress?: ExerciseProgress;
  onEdit: () => void;
  onDelete: () => void;
  onMoveUp?: () => void;
  onMoveDown?: () => void;
  onStats: () => void;
  onChecked: () => void;
};

const EMPTY_ANSWER: AnswerState = { selected: [], text: "" };

const DEFAULT_OPEN_ANSWER_LENGTH = 2000;
const DEFAULT_CODE_LENGTH = 4000;

const isTextQuestion = (question: Question) =>
  question.Type === "ShortAnswer" || question.Type === "OpenAnswer" || question.Type === "Code";

const isAiQuestion = (question: Question) => question.Type === "OpenAnswer" || question.Type === "Code";

const initialAnswer = (question: Question): AnswerState =>
  question.Type === "Code" ? { selected: [], text: question.StarterCode ?? "" } : EMPTY_ANSWER;

const isAnswered = (question: Question, answer?: AnswerState) => {
  if (!answer) return false;
  if (!isTextQuestion(question)) return answer.selected.length > 0;
  const text = answer.text.trim();
  return text.length > 0 && (question.Type !== "Code" || text !== (question.StarterCode ?? "").trim());
};

const STATUS_ICONS = {
  Correct: <CheckCircleFilled />,
  PartiallyCorrect: <MinusCircleFilled />,
  Incorrect: <CloseCircleFilled />,
};

const optionClass = (optionId: number, answer: AnswerState, result?: QuestionResult) => {
  if (!result) return "exercise-option";
  if (result.CorrectOptionIds.includes(optionId)) return "exercise-option is-correct";
  if (answer.selected.includes(optionId)) return "exercise-option is-wrong";
  return "exercise-option";
};

const ExerciseCard = ({
  exercise,
  isEditing,
  isCourseEditor,
  progress,
  onEdit,
  onDelete,
  onMoveUp,
  onMoveDown,
  onStats,
  onChecked,
}: Props) => {
  const [answers, setAnswers] = useState<Record<number, AnswerState>>({});
  const [result, setResult] = useState<ExerciseCheckResult | null>(null);
  const [isChecking, setIsChecking] = useState(false);
  const [state] = useTable();
  const [limits, setLimits] = useState({ answer: DEFAULT_OPEN_ANSWER_LENGTH, code: DEFAULT_CODE_LENGTH });
  const [isAiEnabled, setIsAiEnabled] = useState(false);
  const hasAiQuestions = exercise.Questions.some(isAiQuestion);

  useEffect(() => {
    if (!hasAiQuestions) return;
    ExerciseService.getAiStatus()
      .then((status) => {
        setIsAiEnabled(status.Enabled);
        setLimits({ answer: status.MaxAnswerLength, code: status.MaxCodeLength || DEFAULT_CODE_LENGTH });
      })
      .catch(() => undefined);
  }, [hasAiQuestions]);

  const getAnswer = (question: Question) => answers[question.Id] ?? initialAnswer(question);

  const totalPoints = useMemo(() => exercise.Questions.reduce((sum, q) => sum + q.Points, 0), [exercise]);
  const answeredCount = exercise.Questions.filter((q) => isAnswered(q, getAnswer(q))).length;
  const resultsById = useMemo(
    () => new Map((result?.Questions ?? []).map((item) => [item.QuestionId, item])),
    [result]
  );

  const setAnswer = (question: Question, patch: Partial<AnswerState>) =>
    setAnswers((previous) => ({
      ...previous,
      [question.Id]: { ...(previous[question.Id] ?? initialAnswer(question)), ...patch },
    }));

  const check = async () => {
    const payload: QuestionAnswer[] = exercise.Questions.map((question) => {
      const answer = getAnswer(question);
      return { QuestionId: question.Id, SelectedOptionIds: answer.selected, Text: answer.text };
    });

    setIsChecking(true);
    try {
      setResult(await ExerciseService.check(exercise.Id, payload));
      onChecked();
    } catch (error) {
      const messages = getErrorMessages(error);
      notification("error", messages[0] ?? "Не вдалося перевірити відповіді. Спробуйте ще раз.");
    } finally {
      setIsChecking(false);
    }
  };

  const retry = () => {
    setResult(null);
    setAnswers((previous) =>
      Object.fromEntries(
        exercise.Questions.filter((question) => question.Type === "Code" && previous[question.Id]).map((question) => [
          question.Id,
          previous[question.Id],
        ])
      )
    );
  };

  const renderFeedback = (questionResult: QuestionResult | undefined, note: string) =>
    questionResult?.Feedback ? (
      <div className={`exercise-feedback${questionResult.IsAiReviewed ? "" : " exercise-feedback--muted"}`} role="status">
        {questionResult.IsAiReviewed && (
          <span className="exercise-feedback__title">
            <RobotOutlined /> Відгук AI
          </span>
        )}
        <p className="exercise-feedback__text">{questionResult.Feedback}</p>
        {(questionResult.Issues ?? []).length > 0 && (
          <ul className="exercise-issues">
            {(questionResult.Issues ?? []).map((issue) => (
              <li key={`${issue.Line}-${issue.Message}`}>
                <span className="exercise-issues__line">Рядок {issue.Line}</span>
                <span>{issue.Message}</span>
              </li>
            ))}
          </ul>
        )}
        {questionResult.IsAiReviewed && <span className="exercise-feedback__note">{note}</span>}
      </div>
    ) : null;

  const aiNotice = isAiEnabled ? (
    <p className="exercise-ai-notice">
      <RobotOutlined /> Відповідь перевірить ШІ (Claude). Не пишіть у ній ім'я, контакти чи інші особисті дані.{" "}
      <Link to="/about#ai">Докладніше</Link>
    </p>
  ) : null;

  const renderQuestionBody = (question: Question) => {
    const answer = getAnswer(question);
    const questionResult = resultsById.get(question.Id);
    const isLocked = result !== null;

    if (question.Type === "SingleChoice") {
      return (
        <Radio.Group
          className="exercise-options"
          value={answer.selected[0]}
          disabled={isLocked}
          onChange={(event) => setAnswer(question, { selected: [event.target.value] })}
        >
          {question.Options.map((option) => (
            <Radio key={option.Id} value={option.Id} className={optionClass(option.Id, answer, questionResult)}>
              {option.Text}
            </Radio>
          ))}
        </Radio.Group>
      );
    }

    if (question.Type === "MultipleChoice") {
      return (
        <Checkbox.Group
          className="exercise-options"
          value={answer.selected}
          disabled={isLocked}
          onChange={(values) => setAnswer(question, { selected: values as number[] })}
        >
          {question.Options.map((option) => (
            <Checkbox key={option.Id} value={option.Id} className={optionClass(option.Id, answer, questionResult)}>
              {option.Text}
            </Checkbox>
          ))}
        </Checkbox.Group>
      );
    }

    if (question.Type === "ShortAnswer") {
      return (
        <div className="exercise-short">
          <Input
            size="large"
            placeholder="Ваша відповідь"
            value={answer.text}
            disabled={isLocked}
            maxLength={500}
            status={questionResult?.Status === "Incorrect" ? "error" : undefined}
            onChange={(event) => setAnswer(question, { text: event.target.value })}
            onPressEnter={() => !isLocked && check()}
          />
          {questionResult?.Status === "Incorrect" && questionResult.AcceptedAnswers.length > 0 && (
            <span className="exercise-hint">Правильна відповідь: {questionResult.AcceptedAnswers.join(" або ")}</span>
          )}
        </div>
      );
    }

    if (question.Type === "OpenAnswer") {
      return (
        <div className="exercise-open">
          {!questionResult && aiNotice}
          <Input.TextArea
            placeholder="Напишіть відповідь своїми словами"
            value={answer.text}
            disabled={isLocked}
            maxLength={limits.answer}
            showCount
            autoSize={{ minRows: 3, maxRows: 12 }}
            onChange={(event) => setAnswer(question, { text: event.target.value })}
          />
          {renderFeedback(questionResult, "Оцінка AI рекомендаційна — орієнтуйтеся на пояснення, а не лише на бали.")}
        </div>
      );
    }

    if (question.Type === "Code") {
      const language = state.languages.find((item) => item.Code === question.CodeLanguage);
      return (
        <div className="exercise-open">
          {!questionResult && aiNotice}
          <div className="exercise-code__meta">
            <span className="exercise-code__language">{language?.Name ?? question.CodeLanguage}</span>
            <span className="exercise-hint">
              {answer.text.length} / {limits.code} · Tab — відступ, Esc потім Tab — вийти з редактора
            </span>
          </div>
          <CodeEditor
            value={answer.text}
            onChange={(text) => setAnswer(question, { text })}
            highlightLanguage={language?.HighlightLanguage ?? "plaintext"}
            readOnly={isLocked}
            markedLines={(questionResult?.Issues ?? []).map((issue) => issue.Line)}
            maxLength={limits.code}
            minLines={8}
            placeholder="Напишіть рішення тут"
            ariaLabel={`Код відповіді, мова ${language?.Name ?? question.CodeLanguage ?? ""}`}
          />
          {renderFeedback(
            questionResult,
            "Код не запускався: AI проаналізував його логіку. Оцінка рекомендаційна."
          )}
        </div>
      );
    }

    return <span className="exercise-hint">Цей тип завдання поки не підтримується.</span>;
  };

  return (
    <section className="card exercise-card" aria-labelledby={`exercise-${exercise.Id}`}>
      <header className="exercise-card__header">
        <div className="exercise-card__heading">
          <h3 id={`exercise-${exercise.Id}`} className="exercise-card__title">{exercise.Title}</h3>
          <span className="exercise-card__meta">
            {exercise.Questions.length} пит. · {totalPoints} б.
          </span>
          {progress && (
            <span className="exercise-progress">
              Ваш найкращий результат: <strong>{progress.BestScore} з {progress.MaxScore}</strong> ·{" "}
              {pluralize(progress.Attempts, "спроба", "спроби", "спроб")}
            </span>
          )}
        </div>
        {isEditing && (
          <div className="exercise-card__tools">
            <Button icon={<ArrowUpOutlined />} disabled={!onMoveUp} onClick={onMoveUp} aria-label="Перемістити завдання вище" title="Вище" />
            <Button
              icon={<ArrowDownOutlined />}
              disabled={!onMoveDown}
              onClick={onMoveDown}
              aria-label="Перемістити завдання нижче"
              title="Нижче"
            />
            <Button icon={<BarChartOutlined />} onClick={onStats}>
              Статистика
            </Button>
            <Button icon={<EditOutlined />} onClick={onEdit}>
              Редагувати
            </Button>
            <Button danger icon={<DeleteOutlined />} onClick={onDelete} aria-label="Видалити завдання" />
          </div>
        )}
      </header>

      {exercise.Description && <p className="exercise-card__description">{exercise.Description}</p>}

      {progress?.TeacherReviews && progress.TeacherReviews.length > 0 && (
        <div className="exercise-teacher" role="note">
          <span className="exercise-teacher__title">
            <UserOutlined /> Викладач переглянув ваші відповіді
          </span>
          <ul className="exercise-teacher__list">
            {progress.TeacherReviews.map((review) => {
              const index = exercise.Questions.findIndex((question) => question.Id === review.QuestionId);
              return (
                <li key={review.QuestionId}>
                  <span>
                    {index >= 0 ? `Питання ${index + 1}` : "Питання"}: <strong>{review.Points} з {review.MaxPoints}</strong>
                    {review.IsOverridden ? " (оцінку змінено)" : " (оцінку AI підтверджено)"}
                  </span>
                  {review.Comment && <span className="exercise-teacher__comment">«{review.Comment}»</span>}
                </li>
              );
            })}
          </ul>
        </div>
      )}

      <ol className="exercise-questions">
        {exercise.Questions.map((question, index) => {
          const questionResult = resultsById.get(question.Id);
          return (
            <li key={question.Id} className="exercise-question">
              <div className="exercise-question__head">
                <span className="exercise-question__number">{index + 1}</span>
                <p className="exercise-question__text">{question.Text}</p>
                {questionResult && questionResult.Status !== "NotChecked" ? (
                  <span className={`exercise-status exercise-status--${questionResult.Status.toLowerCase()}`}>
                    {STATUS_ICONS[questionResult.Status]}
                    {questionResult.EarnedPoints}/{questionResult.MaxPoints}
                  </span>
                ) : (
                  <span className="exercise-question__points">{question.Points} б.</span>
                )}
              </div>
              {question.Type === "MultipleChoice" && !result && (
                <span className="exercise-hint">Можна обрати кілька варіантів</span>
              )}
              {renderQuestionBody(question)}
            </li>
          );
        })}
      </ol>

      <footer className="exercise-card__footer">
        {result ? (
          <>
            <div className="exercise-score" role="status">
              <span className="exercise-score__value">
                {result.Score} з {result.MaxScore}
              </span>
              <span className="exercise-score__label">
                {result.MaxScore === 0
                  ? "Цього разу відповіді не вдалося оцінити автоматично."
                  : result.Score === result.MaxScore
                    ? "Чудово, усе правильно!"
                    : "Перегляньте помилки й спробуйте ще раз."}
              </span>
              {isCourseEditor && (
                <span className="exercise-hint">Ви автор курсу — ця спроба не потрапила до статистики.</span>
              )}
            </div>
            <Button size="large" icon={<ReloadOutlined />} onClick={retry}>
              Спробувати ще раз
            </Button>
          </>
        ) : (
          <>
            <span className="exercise-card__progress">
              {hasAiQuestions && isChecking
                ? "Відповіді перевіряє AI — це може тривати до півхвилини."
                : `Відповідей: ${answeredCount} з ${exercise.Questions.length}`}
            </span>
            <Button type="primary" size="large" loading={isChecking} disabled={answeredCount === 0} onClick={check}>
              {isChecking && hasAiQuestions ? "AI перевіряє…" : "Перевірити"}
            </Button>
          </>
        )}
      </footer>
    </section>
  );
};

export default ExerciseCard;
