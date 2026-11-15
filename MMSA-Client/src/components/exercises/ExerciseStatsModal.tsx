import { useEffect, useState } from "react";
import { Alert, Empty, Modal, Skeleton } from "antd";
import { CheckCircleFilled } from "@ant-design/icons";
import { Exercise, ExerciseStats, QuestionStats } from "../../models/Exercise";
import ExerciseService from "../../services/exerciseService";
import { pluralWord } from "../../course/courseUtils";

type Props = {
  exercise: Pick<Exercise, "Id" | "Title"> | null;
  onClose: () => void;
};

const formatPercent = (value: number | null) => (value === null ? "—" : `${Math.round(value)}%`);

const formatDate = (value: string | null) =>
  value ? new Date(value).toLocaleString("uk-UA", { day: "numeric", month: "long", hour: "2-digit", minute: "2-digit" }) : "—";

const StatTile = ({ value, label, hint }: { value: string; label: string; hint?: string }) => (
  <div className="stats-tile" title={hint}>
    <span className="stats-tile__value">{value}</span>
    <span className="stats-tile__label">{label}</span>
  </div>
);

const MeterRow = ({ label, count, total, isCorrect }: { label: string; count: number; total: number; isCorrect?: boolean }) => {
  const share = total === 0 ? 0 : count / total;
  return (
    <li className="stats-meter" title={`${label}: ${count} з ${total} (${Math.round(share * 100)}%)`}>
      <div className="stats-meter__text">
        <span className="stats-meter__label">
          {isCorrect && <CheckCircleFilled className="stats-meter__correct" aria-hidden="true" />}
          {label}
          {isCorrect && <span className="stats-meter__tag">правильна</span>}
        </span>
        <span className="stats-meter__value">
          {count} · {Math.round(share * 100)}%
        </span>
      </div>
      <div className="stats-meter__track" aria-hidden="true">
        <div className="stats-meter__fill" style={{ width: `${share * 100}%` }} />
      </div>
    </li>
  );
};

const QuestionBlock = ({ question, index }: { question: QuestionStats; index: number }) => (
  <li className="stats-question">
    <div className="stats-question__head">
      <span className="exercise-question__number">{index + 1}</span>
      <p className="stats-question__text">{question.Text}</p>
      <span className="stats-question__rate">
        {question.Answered === 0 ? "немає відповідей" : `${formatPercent(question.CorrectPercent)} правильно`}
      </span>
    </div>

    {question.Answered > 0 && question.Options.length > 0 && (
      <ul className="stats-meters" aria-label="Скільки разів обирали кожен варіант">
        {question.Options.map((option) => (
          <MeterRow
            key={option.OptionId}
            label={option.Text}
            count={option.Picks}
            total={question.Answered}
            isCorrect={option.IsCorrect}
          />
        ))}
      </ul>
    )}

    {(question.Type === "OpenAnswer" || question.Type === "Code") && question.Answered > 0 && (
      <p className="stats-question__summary">
        Повністю: {question.Correct} · частково: {question.Partial} · середній бал AI: {question.AveragePoints ?? 0} з {question.MaxPoints}
      </p>
    )}

    {question.Type === "ShortAnswer" && question.Answered > 0 && (
      question.WrongAnswers.length === 0 ? (
        <p className="exercise-hint">Неправильних відповідей немає.</p>
      ) : (
        <div className="stats-wrong">
          <span className="stats-wrong__title">Найчастіші помилки</span>
          <ul className="stats-wrong__list">
            {question.WrongAnswers.map((answer) => (
              <li key={answer.Text}>
                <code>{answer.Text}</code>
                <span>× {answer.Count}</span>
              </li>
            ))}
          </ul>
        </div>
      )
    )}
  </li>
);

const ExerciseStatsModal = ({ exercise, onClose }: Props) => {
  const [stats, setStats] = useState<ExerciseStats | null>(null);
  const [hasError, setHasError] = useState(false);

  useEffect(() => {
    if (!exercise) return;
    let isActive = true;
    setStats(null);
    setHasError(false);
    ExerciseService.getStats(exercise.Id)
      .then((data) => isActive && setStats(data))
      .catch(() => isActive && setHasError(true));
    return () => {
      isActive = false;
    };
  }, [exercise]);

  return (
    <Modal
      title={exercise ? `Статистика: ${exercise.Title}` : "Статистика"}
      open={exercise !== null}
      onCancel={onClose}
      footer={null}
      width={760}
      destroyOnClose
    >
      {hasError ? (
        <Alert type="error" showIcon message="Не вдалося завантажити статистику." />
      ) : !stats ? (
        <Skeleton active paragraph={{ rows: 6 }} />
      ) : stats.Attempts === 0 ? (
        <Empty description="Ще ніхто не виконував це завдання." />
      ) : (
        <div className="stats">
          <div className="stats-tiles">
            <StatTile value={String(stats.Students)} label={pluralWord(stats.Students, "учень", "учні", "учнів")} />
            <StatTile value={String(stats.Attempts)} label={pluralWord(stats.Attempts, "спроба", "спроби", "спроб")} />
            <StatTile
              value={formatPercent(stats.FirstAttemptAveragePercent)}
              label="з першої спроби"
              hint="Середній результат першої спроби кожного учня"
            />
            <StatTile
              value={formatPercent(stats.BestAveragePercent)}
              label="найкращий результат"
              hint="Середнє з найкращих спроб кожного учня"
            />
          </div>
          <p className="exercise-hint">
            Учні анонімні: один браузер — один учень. Останнє виконання: {formatDate(stats.LastSubmittedAtUtc)}.
          </p>

          <ol className="stats-questions">
            {stats.Questions.map((question, index) => (
              <QuestionBlock key={question.QuestionId} question={question} index={index} />
            ))}
          </ol>
        </div>
      )}
    </Modal>
  );
};

export default ExerciseStatsModal;
