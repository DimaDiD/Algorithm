import { ReactNode, useEffect, useMemo, useState } from "react";
import { Input, Skeleton } from "antd";
import { ArrowRightOutlined, PlusOutlined, SearchOutlined } from "@ant-design/icons";
import { Link, useLocation, useNavigate } from "react-router-dom";
import { useTable } from "../../Store";
import { useAuth } from "../../auth/AuthContext";
import { Course } from "../../models/Course";
import CourseService from "../../services/courseService";
import { coursePath, pluralize, pluralWord } from "../../course/courseUtils";
import AppHeader from "../../components/layout/AppHeader";
import AppFooter from "../../components/layout/AppFooter";
import LanguageSwitch from "../../components/course/LanguageSwitch";
import CourseEditorModal from "../../components/course/CourseEditorModal";
import CodeContent from "../../PageContentTypes/CodeContent";

const SAMPLE_CODE: Record<string, string> = {
  csharp: `int m = k;            // m починається як k
while (n % m == 0)
{
    Console.Write(k + " x ");
    m *= k;           // наступний степінь k
}`,
  cpp: `int m = k;            // m починається як k
while (n % m == 0)
{
    cout << k << " x ";
    m *= k;           // наступний степінь k
}`,
  pascal: `m := k;               { m починається як k }
while n mod m = 0 do
begin
  Write(k, ' x ');
  m := m * k;         { наступний степінь k }
end;`,
};

const STEPS = [
  { title: "Оберіть курс", text: "Кожен курс веде окремий викладач зі своєю програмою." },
  { title: "Читайте теми", text: "Умова, розбір ідеї та приклад коду обраною мовою." },
  { title: "Перевірте себе", text: "Короткі вправи з автоматичною перевіркою після теми." },
];

const sectionsLabel = (count: number) => pluralize(count, "розділ", "розділи", "розділів");

const CourseCard = ({ course }: { course: Course }) => (
  <Link to={coursePath(course.Slug)} className="card course-card">
    <div className="course-card__top">
      <span className="course-card__eyebrow">
        {course.SectionCount > 0 ? sectionsLabel(course.SectionCount) : "Поки без розділів"}
      </span>
      {!course.IsPublished && <span className="status-badge">Чернетка</span>}
    </div>
    <h3 className="course-card__title">{course.Title}</h3>
    {course.Description && <p className="course-card__description">{course.Description}</p>}
    <div className="course-card__footer">
      <div className="course-card__languages">
        {course.Languages.map((language) => (
          <span key={language.Code} className="language-chip">{language.Name}</span>
        ))}
      </div>
      <span className="course-card__cta" aria-hidden="true">
        <ArrowRightOutlined />
      </span>
    </div>
  </Link>
);

const normalize = (value: string) => value.trim().toLocaleLowerCase("uk");

const matches = (course: Course, query: string) =>
  [course.Title, course.Description ?? "", ...course.Languages.map((language) => language.Name)].some((text) =>
    normalize(text).includes(query)
  );

const useCourseSearch = (courses: Course[] | null) => {
  const [query, setQuery] = useState("");
  const normalized = normalize(query);
  const filtered = useMemo(
    () => (courses && normalized ? courses.filter((course) => matches(course, normalized)) : courses),
    [courses, normalized]
  );
  return { query, setQuery, filtered };
};

type SearchProps = { value: string; onChange: (value: string) => void; label: string };

const CourseSearch = ({ value, onChange, label }: SearchProps) => (
  <Input
    className="course-search"
    allowClear
    prefix={<SearchOutlined />}
    placeholder="Назва, опис або мова"
    aria-label={label}
    value={value}
    onChange={(event) => onChange(event.target.value)}
  />
);

type CourseListProps = { courses: Course[]; query: string; empty: ReactNode };

const CourseList = ({ courses, query, empty }: CourseListProps) => {
  if (courses.length > 0) {
    return (
      <div className="cards-grid">
        {courses.map((course) => (
          <CourseCard key={course.Id} course={course} />
        ))}
      </div>
    );
  }
  return (
    <div className="card empty-state">
      <p>{query.trim() ? `Нічого не знайдено за запитом «${query.trim()}».` : empty}</p>
    </div>
  );
};

export const CatalogPage = () => {
  const [state] = useTable();
  const { user, canEdit, isInitialized } = useAuth();
  const location = useLocation();
  const navigate = useNavigate();
  const [published, setPublished] = useState<Course[] | null>(null);
  const [mine, setMine] = useState<Course[] | null>(null);
  const [isCreateOpen, setIsCreateOpen] = useState(false);
  const [loadError, setLoadError] = useState(false);
  const mineSearch = useCourseSearch(mine);
  const catalogSearch = useCourseSearch(published);

  useEffect(() => {
    document.title = "Основи програмування";
  }, []);

  useEffect(() => {
    if (!isInitialized) return;
    let isActive = true;
    setLoadError(false);
    CourseService.getPublished()
      .then((courses) => isActive && setPublished(courses))
      .catch(() => isActive && setLoadError(true));
    return () => {
      isActive = false;
    };
  }, [isInitialized, user?.id]);

  useEffect(() => {
    if (!isInitialized || !canEdit) {
      setMine(null);
      return;
    }
    let isActive = true;
    CourseService.getMine()
      .then((courses) => isActive && setMine(courses))
      .catch(() => isActive && setMine([]));
    return () => {
      isActive = false;
    };
  }, [isInitialized, canEdit, user?.id]);

  useEffect(() => {
    if (location.hash && published) {
      document.getElementById(location.hash.slice(1))?.scrollIntoView({ behavior: "smooth" });
    }
  }, [location.hash, published]);

  const sampleLanguage = SAMPLE_CODE[state.codeOption] ? state.codeOption : "csharp";

  return (
    <div className="page">
      <AppHeader />

      <main>
        <section className="container hero">
          <div className="hero__text">
            <span className="eyebrow">Мультимовна навчальна платформа</span>
            <h1 className="hero__title">
              Основи програмування <span>кількома мовами</span>
            </h1>
            <p className="hero__lead">
              Курси від викладачів з поясненнями та прикладами коду мовами C#, C++, Pascal та іншими. Оберіть мову один раз —
              і всі приклади підлаштуються під неї.
            </p>
            <div className="hero__actions">
              <a href="#courses" className="btn btn--primary">
                Обрати курс <ArrowRightOutlined />
              </a>
            </div>
            <div className="hero__stats">
              <div className="stat">
                <span className="stat__value">{published ? published.length : "…"}</span>
                <span className="stat__label">курсів</span>
              </div>
              <div className="stat">
                <span className="stat__value">{state.languages.length}</span>
                <span className="stat__label">{pluralWord(state.languages.length, "мова", "мови", "мов")} програмування</span>
              </div>
              <div className="stat">
                <span className="stat__value">Відкрито</span>
                <span className="stat__label">читання без реєстрації</span>
              </div>
            </div>
          </div>

          <div className="hero-code">
            <div className="hero-code__top">
              <LanguageSwitch variant="dark" codes={Object.keys(SAMPLE_CODE)} />
              <span className="hero-code__caption">Приклад: розкладання на прості множники</span>
            </div>
            <CodeContent code={SAMPLE_CODE[sampleLanguage]} language={sampleLanguage} showHeader={false} />
          </div>
        </section>

        {canEdit && (
          <section className="container section">
            <div className="section__head">
              <div>
                <div className="section__title-row">
                  <h2 className="section__title">Мої курси</h2>
                  <button type="button" className="btn btn--primary btn--small" onClick={() => setIsCreateOpen(true)}>
                    <PlusOutlined /> Створити курс
                  </button>
                </div>
                <p className="section__lead">Курси, які ви ведете. Чернетки бачите лише ви.</p>
              </div>
              {mine && mine.length > 0 && (
                <div className="section__tools">
                  <CourseSearch value={mineSearch.query} onChange={mineSearch.setQuery} label="Пошук серед моїх курсів" />
                </div>
              )}
            </div>

            {!mine || !mineSearch.filtered ? (
              <Skeleton active paragraph={{ rows: 3 }} />
            ) : (
              <CourseList
                courses={mineSearch.filtered}
                query={mineSearch.query}
                empty="У вас ще немає курсів. Створіть перший — він з'явиться в каталозі після публікації."
              />
            )}
          </section>
        )}

        <section id="courses" className="container section">
          <div className="section__head">
            <div>
              <h2 className="section__title">Каталог курсів</h2>
              <p className="section__lead">Усі опубліковані курси. Читати можна без реєстрації.</p>
            </div>
            {published && published.length > 0 && (
              <div className="section__tools">
                <CourseSearch value={catalogSearch.query} onChange={catalogSearch.setQuery} label="Пошук у каталозі" />
              </div>
            )}
          </div>

          {loadError ? (
            <div className="card empty-state">
              <p>Не вдалося завантажити курси. Спробуйте оновити сторінку.</p>
            </div>
          ) : !published || !catalogSearch.filtered ? (
            <Skeleton active paragraph={{ rows: 4 }} />
          ) : (
            <CourseList courses={catalogSearch.filtered} query={catalogSearch.query} empty="Поки що немає опублікованих курсів." />
          )}
        </section>

        <section className="container section" style={{ paddingTop: 0 }}>
          <div className="card steps">
            <h2 className="section__title" style={{ fontSize: 30 }}>Як це працює</h2>
            <div className="steps__grid">
              {STEPS.map((step, index) => (
                <div key={step.title} className="step">
                  <span className="step__number">{String(index + 1).padStart(2, "0")}</span>
                  <h3 className="step__title">{step.title}</h3>
                  <p className="step__text">{step.text}</p>
                </div>
              ))}
            </div>
          </div>
        </section>
      </main>

      <AppFooter />

      {canEdit && (
        <CourseEditorModal
          open={isCreateOpen}
          onClose={() => setIsCreateOpen(false)}
          onSaved={(course) => {
            setIsCreateOpen(false);
            navigate(coursePath(course.Slug));
          }}
        />
      )}
    </div>
  );
};

export default CatalogPage;
