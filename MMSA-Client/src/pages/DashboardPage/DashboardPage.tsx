import { useCallback, useEffect, useMemo, useState } from "react";
import { Badge, Button, Empty, Pagination, Result, Segmented, Select, Skeleton, Table, Tooltip } from "antd";
import type { ColumnsType } from "antd/es/table";
import { BarChartOutlined, QuestionCircleOutlined } from "@ant-design/icons";
import { Link, useParams } from "react-router-dom";
import { coursePath, pluralWord } from "../../course/courseUtils";
import { useCanEditCourse, useLoadCourse } from "../../course/useCourse";
import { CourseDashboard, DashboardExercise, ReviewFilter, ReviewPage, ReviewUpdate } from "../../models/Review";
import ReviewService from "../../services/reviewService";
import { getErrorMessages } from "../../auth/authApi";
import notification from "../../notification/notificationMessage";
import AppHeader from "../../components/layout/AppHeader";
import AppFooter from "../../components/layout/AppFooter";
import ExerciseStatsModal from "../../components/exercises/ExerciseStatsModal";
import ReviewCard from "../../components/dashboard/ReviewCard";

const formatPercent = (value: number | null) => (value === null ? "—" : `${Math.round(value)}%`);

const formatScore = (value: number) => value.toLocaleString("uk-UA", { maximumFractionDigits: 1 });

const comparePercent = (a: number | null, b: number | null) => (a ?? Infinity) - (b ?? Infinity);

const ColumnTitle = ({ title, hint }: { title: string; hint: string }) => (
  <Tooltip title={hint}>
    <span className="column-title">
      {title} <QuestionCircleOutlined className="column-title__icon" aria-label={hint} />
    </span>
  </Tooltip>
);

const ScoreCell = ({ score, percent, max }: { score: number | null; percent: number | null; max: number | null }) =>
  score === null || max === null ? (
    <>—</>
  ) : (
    <div className="score-cell">
      <span className="score-cell__value">
        {formatScore(score)} <span className="score-cell__max">з {max}</span>
      </span>
      <span className="score-cell__percent">{formatPercent(percent)}</span>
    </div>
  );

const FILTERS: { label: string; value: ReviewFilter }[] = [
  { label: "Чекають перегляду", value: "Pending" },
  { label: "Переглянуті", value: "Reviewed" },
  { label: "Усі", value: "all" },
];

export const DashboardPage = () => {
  const { courseSlug = "" } = useParams();
  const state = useLoadCourse(courseSlug);
  const canEdit = useCanEditCourse();
  const course = state.course?.Slug === courseSlug ? state.course : null;

  const [dashboard, setDashboard] = useState<CourseDashboard | null>(null);
  const [reviews, setReviews] = useState<ReviewPage | null>(null);
  const [filter, setFilter] = useState<ReviewFilter>("Pending");
  const [exerciseId, setExerciseId] = useState<number | undefined>();
  const [page, setPage] = useState(1);
  const [statsFor, setStatsFor] = useState<{ Id: number; Title: string } | null>(null);

  const courseId = course && canEdit ? course.Id : null;

  const loadDashboard = useCallback(async () => {
    if (courseId === null) return;
    try {
      setDashboard(await ReviewService.getDashboard(courseId));
    } catch (error) {
      notification("error", getErrorMessages(error)[0]);
    }
  }, [courseId]);

  const loadReviews = useCallback(async () => {
    if (courseId === null) return;
    try {
      setReviews(await ReviewService.getReviews(courseId, { exerciseId, status: filter, page }));
    } catch (error) {
      notification("error", getErrorMessages(error)[0]);
    }
  }, [courseId, exerciseId, filter, page]);

  useEffect(() => {
    loadDashboard();
  }, [loadDashboard]);

  useEffect(() => {
    loadReviews();
  }, [loadReviews]);

  useEffect(() => {
    document.title = course ? `Кабінет — ${course.Title}` : "Кабінет викладача";
  }, [course]);

  const handleReview = async (answerId: number, update: ReviewUpdate) => {
    try {
      await ReviewService.review(answerId, update);
      notification("success", update.Action === "confirm" ? "Оцінку AI підтверджено." : "Оцінку збережено.");
      await Promise.all([loadReviews(), loadDashboard()]);
    } catch (error) {
      notification("error", getErrorMessages(error)[0]);
      throw error;
    }
  };

  const columns: ColumnsType<DashboardExercise> = useMemo(
    () => [
      {
        title: "Завдання",
        key: "title",
        render: (_, exercise) => (
          <div className="dashboard-exercise">
            <span className="dashboard-exercise__title">{exercise.Title}</span>
            <span className="dashboard-exercise__topic">
              {exercise.SubPageName ? `${exercise.PageName} / ${exercise.SubPageName}` : exercise.PageName}
            </span>
          </div>
        ),
      },
      {
        title: <ColumnTitle title="Учні" hint="Скільки різних учнів (браузерів) хоча б раз виконали завдання" />,
        dataIndex: "Students",
        align: "right",
        sorter: (a, b) => a.Students - b.Students,
      },
      {
        title: <ColumnTitle title="Спроби" hint="Скільки разів усього натиснули «Перевірити», разом із повторними спробами" />,
        dataIndex: "Attempts",
        align: "right",
        sorter: (a, b) => a.Attempts - b.Attempts,
      },
      {
        title: (
          <ColumnTitle
            title="Перша спроба"
            hint="Середній бал за першу спробу кожного учня — наскільки тему зрозуміли одразу після читання"
          />
        ),
        key: "first",
        align: "right",
        defaultSortOrder: "ascend",
        sorter: (a, b) => comparePercent(a.FirstAttemptAveragePercent, b.FirstAttemptAveragePercent),
        render: (_, exercise) => (
          <ScoreCell
            score={exercise.FirstAttemptAverageScore}
            percent={exercise.FirstAttemptAveragePercent}
            max={exercise.MaxScore}
          />
        ),
      },
      {
        title: (
          <ColumnTitle
            title="Найкраща спроба"
            hint="Середній бал за найкращу спробу кожного учня — чого учні досягли після повторних спроб"
          />
        ),
        key: "best",
        align: "right",
        sorter: (a, b) => comparePercent(a.BestAveragePercent, b.BestAveragePercent),
        render: (_, exercise) => (
          <ScoreCell score={exercise.BestAverageScore} percent={exercise.BestAveragePercent} max={exercise.MaxScore} />
        ),
      },
      {
        title: <ColumnTitle title="На перегляді" hint="Відкриті відповіді, оцінку яких ШІ ще чекає вашого підтвердження" />,
        dataIndex: "PendingReviews",
        align: "right",
        sorter: (a, b) => a.PendingReviews - b.PendingReviews,
        render: (value: number, exercise) =>
          value > 0 ? (
            <button
              type="button"
              className="link-button"
              onClick={() => {
                setExerciseId(exercise.ExerciseId);
                setFilter("Pending");
                setPage(1);
                document.getElementById("reviews")?.scrollIntoView({ behavior: "smooth" });
              }}
            >
              <Badge count={value} color="#B07A00" />
            </button>
          ) : (
            "—"
          ),
      },
      {
        title: "",
        key: "stats",
        align: "right",
        render: (_, exercise) => (
          <Button
            size="small"
            icon={<BarChartOutlined />}
            onClick={() => setStatsFor({ Id: exercise.ExerciseId, Title: exercise.Title })}
            aria-label={`Статистика: ${exercise.Title}`}
          />
        ),
      },
    ],
    []
  );

  if (state.courseStatus === "notFound" || (course && !canEdit)) {
    return (
      <div className="page">
        <AppHeader courseSlug={courseSlug} />
        <main className="container section">
          <div className="card topic-card">
            <Result
              status="403"
              title="Кабінет доступний лише автору курсу"
              extra={<Link to={coursePath(courseSlug)} className="btn btn--primary">До курсу</Link>}
            />
          </div>
        </main>
        <AppFooter />
      </div>
    );
  }

  return (
    <div className="page">
      <AppHeader courseSlug={courseSlug} />

      <main className="container section dashboard">
        <div>
          <nav className="breadcrumbs" aria-label="Навігаційний ланцюжок">
            <Link to="/">Курси</Link>
            <span aria-hidden="true">/</span>
            <Link to={coursePath(courseSlug)}>{course?.Title ?? "…"}</Link>
            <span aria-hidden="true">/</span>
            <span className="breadcrumbs__current" aria-current="page">Кабінет</span>
          </nav>
          <h1 className="section__title dashboard__title">Кабінет викладача</h1>
        </div>

        {!dashboard ? (
          <Skeleton active paragraph={{ rows: 6 }} />
        ) : (
          <>
            <div className="stats-tiles">
              <div className="stats-tile">
                <span className="stats-tile__value">{dashboard.Students}</span>
                <span className="stats-tile__label">{pluralWord(dashboard.Students, "учень", "учні", "учнів")}</span>
              </div>
              <div className="stats-tile">
                <span className="stats-tile__value">{dashboard.Attempts}</span>
                <span className="stats-tile__label">{pluralWord(dashboard.Attempts, "спроба", "спроби", "спроб")}</span>
              </div>
              <div className="stats-tile" title="Середній результат першої спроби кожного учня по всіх завданнях курсу">
                <span className="stats-tile__value">{formatPercent(dashboard.FirstAttemptAveragePercent)}</span>
                <span className="stats-tile__label">перша спроба</span>
              </div>
              <div className="stats-tile" title="Середній результат найкращої спроби кожного учня по всіх завданнях курсу">
                <span className="stats-tile__value">{formatPercent(dashboard.BestAveragePercent)}</span>
                <span className="stats-tile__label">найкраща спроба</span>
              </div>
              <div className={`stats-tile${dashboard.PendingReviews > 0 ? " stats-tile--attention" : ""}`}>
                <span className="stats-tile__value">{dashboard.PendingReviews}</span>
                <span className="stats-tile__label">чекають перегляду</span>
              </div>
            </div>

            <section className="card dashboard__panel">
              <div className="dashboard__panel-head">
                <h2 className="dashboard__subtitle">Завдання курсу</h2>
                <span className="exercise-hint">Відсортовано від найслабшого результату першої спроби. Наведіть на «?», щоб побачити пояснення</span>
              </div>
              <Table
                rowKey="ExerciseId"
                columns={columns}
                dataSource={dashboard.Exercises}
                pagination={false}
                size="middle"
                scroll={{ x: 720 }}
                locale={{ emptyText: <Empty description="У курсі ще немає завдань." /> }}
              />
            </section>
          </>
        )}

        <section id="reviews" className="dashboard__reviews">
          <div className="dashboard__panel-head">
            <h2 className="dashboard__subtitle">Відповіді, перевірені AI</h2>
            <div className="dashboard__filters">
              <Select
                allowClear
                placeholder="Усі завдання"
                value={exerciseId}
                style={{ minWidth: 240 }}
                options={(dashboard?.Exercises ?? []).map((exercise) => ({
                  label: exercise.Title,
                  value: exercise.ExerciseId,
                }))}
                onChange={(value?: number) => {
                  setExerciseId(value);
                  setPage(1);
                }}
              />
              <Segmented
                options={FILTERS}
                value={filter}
                onChange={(value) => {
                  setFilter(value as ReviewFilter);
                  setPage(1);
                }}
              />
            </div>
          </div>

          {!reviews ? (
            <Skeleton active paragraph={{ rows: 4 }} />
          ) : reviews.Items.length === 0 ? (
            <div className="card empty-state">
              <p>{filter === "Pending" ? "Усі відповіді переглянуто." : "Відповідей поки немає."}</p>
            </div>
          ) : (
            <>
              {reviews.Items.map((item) => (
                <ReviewCard
                  key={`${item.AnswerId}-${item.ReviewStatus}-${item.EarnedPoints}`}
                  item={item}
                  onReview={(update) => handleReview(item.AnswerId, update)}
                />
              ))}
              {reviews.Total > reviews.PageSize && (
                <Pagination
                  current={reviews.Page}
                  pageSize={reviews.PageSize}
                  total={reviews.Total}
                  showSizeChanger={false}
                  onChange={setPage}
                />
              )}
            </>
          )}
        </section>
      </main>

      <AppFooter />
      <ExerciseStatsModal exercise={statsFor} onClose={() => setStatsFor(null)} />
    </div>
  );
};

export default DashboardPage;
