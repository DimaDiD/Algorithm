import { useEffect, useMemo, useState } from "react";
import { Drawer, Result, Skeleton } from "antd";
import { Link, useParams } from "react-router-dom";
import { coursePath, findTopic, flattenTopics } from "../../course/courseUtils";
import { useCanEditCourse, useLoadCourse } from "../../course/useCourse";
import AppHeader from "../../components/layout/AppHeader";
import AppFooter from "../../components/layout/AppFooter";
import CourseNav from "../../components/course/CourseNav";
import LanguageSwitch from "../../components/course/LanguageSwitch";
import TopicContent from "../../components/course/TopicContent";
import Pager from "../../components/course/Pager";
import ExerciseSection from "../../components/exercises/ExerciseSection";

const SITE_TITLE = "Основи програмування";

export const TopicPage = () => {
  const { courseSlug = "", page: pageName, subPage: subPageName } = useParams();
  const state = useLoadCourse(courseSlug);
  const canEditCourse = useCanEditCourse();
  const [isNavOpen, setIsNavOpen] = useState(false);

  const course = state.course?.Slug === courseSlug ? state.course : null;
  const isLoaded = !!course && state.isMenuLoaded;
  const topics = useMemo(() => flattenTopics(courseSlug, state.menu), [courseSlug, state.menu]);
  const topic = useMemo(
    () => findTopic(courseSlug, state.menu, pageName, subPageName),
    [courseSlug, state.menu, pageName, subPageName]
  );
  const index = topic ? topics.findIndex((item) => item.path === topic.path) : -1;

  useEffect(() => {
    window.scrollTo({ top: 0 });
    setIsNavOpen(false);
  }, [pageName, subPageName]);

  useEffect(() => {
    const suffix = course ? course.Title : SITE_TITLE;
    document.title = topic ? `${topic.title} — ${suffix}` : suffix;
  }, [topic, course]);

  const renderNav = () =>
    course && (
      <CourseNav
        courseId={course.Id}
        courseSlug={course.Slug}
        menu={state.menu}
        activeTopic={topic}
        onNavigate={() => setIsNavOpen(false)}
      />
    );

  if (state.courseStatus === "notFound" || state.courseStatus === "error") {
    return (
      <div className="page">
        <AppHeader />
        <main className="container section">
          <div className="card topic-card">
            <Result
              status={state.courseStatus === "notFound" ? "404" : "error"}
              title={state.courseStatus === "notFound" ? "Курс не знайдено" : "Не вдалося завантажити курс"}
              subTitle={
                state.courseStatus === "notFound"
                  ? "Можливо, його ще не опубліковано або видалено."
                  : "Перевірте з'єднання і спробуйте оновити сторінку."
              }
              extra={<Link to="/" className="btn btn--primary">До каталогу курсів</Link>}
            />
          </div>
        </main>
        <AppFooter />
      </div>
    );
  }

  return (
    <div className="page">
      <AppHeader onOpenNavigation={() => setIsNavOpen(true)} courseSlug={courseSlug} showEditToggle={canEditCourse} />

      <div className="container topic-layout">
        <aside className="course-sidebar card">
          {isLoaded ? renderNav() : <div style={{ padding: 16 }}><Skeleton active /></div>}
        </aside>

        <main className="topic-main">
          {!isLoaded ? (
            <div className="card topic-card">
              <Skeleton active paragraph={{ rows: 8 }} />
            </div>
          ) : !topic ? (
            <div className="card topic-card">
              <Result
                status="404"
                title="Тему не знайдено"
                subTitle="Можливо, її перейменували або видалили."
                extra={
                  <Link to={coursePath(courseSlug)} className="btn btn--primary">
                    До змісту курсу
                  </Link>
                }
              />
            </div>
          ) : (
            <>
              <article className="card topic-card">
                <div className="topic-card__top">
                  <nav className="breadcrumbs" aria-label="Навігаційний ланцюжок">
                    <Link to="/">Курси</Link>
                    <span aria-hidden="true">/</span>
                    <Link to={coursePath(courseSlug)}>{course?.Title}</Link>
                    <span aria-hidden="true">/</span>
                    {topic.subPage ? (
                      <>
                        <span>{topic.page.PageName}</span>
                        <span aria-hidden="true">/</span>
                        <span className="breadcrumbs__current" aria-current="page">{topic.subPage.Name}</span>
                      </>
                    ) : (
                      <span className="breadcrumbs__current" aria-current="page">{topic.page.PageName}</span>
                    )}
                  </nav>
                  <LanguageSwitch />
                </div>

                <TopicContent key={topic.path} pageId={topic.page.Id} subPageId={topic.subPage?.Id} />
              </article>

              <ExerciseSection key={`exercises-${topic.path}`} pageId={topic.page.Id} subPageId={topic.subPage?.Id} />

              <Pager previous={index > 0 ? topics[index - 1] : undefined} next={topics[index + 1]} />
            </>
          )}
        </main>
      </div>

      <Drawer
        className="course-nav-drawer"
        title="Зміст курсу"
        placement="left"
        width={320}
        open={isNavOpen}
        onClose={() => setIsNavOpen(false)}
      >
        {isLoaded && renderNav()}
      </Drawer>

      <AppFooter />
    </div>
  );
};

export default TopicPage;
