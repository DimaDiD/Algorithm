import { useEffect, useRef, useState } from "react";
import { Alert, AutoComplete, Button, Popconfirm, Skeleton } from "antd";
import { UserAddOutlined } from "@ant-design/icons";
import axios from "axios";
import { CourseTeacher } from "../../models/Course";
import CourseTeacherService from "../../services/courseTeacherService";
import { getErrorMessages } from "../../auth/authApi";
import notification from "../../notification/notificationMessage";
import { fullName } from "../../utils/personName";

type Props = {
  courseId: number;
  canManage: boolean;
};

const HANDLED_STATUSES = [401, 403, 429];
const MIN_SEARCH_LENGTH = 2;
const SEARCH_DELAY_MS = 300;
const EMAIL_PATTERN = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

const errorMessages = (error: unknown): string[] =>
  axios.isAxiosError(error) && HANDLED_STATUSES.includes(error.response?.status ?? 0) ? [] : getErrorMessages(error);

const Person = ({ person }: { person: CourseTeacher }) => {
  const name = fullName(person);
  return (
    <span className="course-teachers__person">
      <span className={`course-teachers__name${name ? "" : " course-teachers__name--empty"}`}>{name ?? "Ім'я не вказано"}</span>
      <span className="course-teachers__email">{person.Email}</span>
    </span>
  );
};

const CourseTeachersPanel = ({ courseId, canManage }: Props) => {
  const [teachers, setTeachers] = useState<CourseTeacher[] | null>(null);
  const [errors, setErrors] = useState<string[]>([]);
  const [search, setSearch] = useState("");
  const [candidates, setCandidates] = useState<CourseTeacher[]>([]);
  const [searchedTerm, setSearchedTerm] = useState("");
  const [isAdding, setIsAdding] = useState(false);
  const [removingId, setRemovingId] = useState<string | null>(null);
  const searchRequest = useRef(0);

  useEffect(() => {
    let isCancelled = false;
    CourseTeacherService.getAll(courseId)
      .then((items) => {
        if (!isCancelled) setTeachers(items);
      })
      .catch((error) => {
        if (isCancelled) return;
        setTeachers([]);
        setErrors(errorMessages(error));
      });
    return () => {
      isCancelled = true;
    };
  }, [courseId]);

  useEffect(() => {
    const term = search.trim();
    const request = ++searchRequest.current;
    if (!canManage || term.length < MIN_SEARCH_LENGTH) {
      setCandidates([]);
      setSearchedTerm("");
      return;
    }
    const timer = window.setTimeout(() => {
      CourseTeacherService.searchCandidates(courseId, term)
        .then((items) => {
          if (request !== searchRequest.current) return;
          setCandidates(items);
          setSearchedTerm(term);
        })
        .catch(() => {
          if (request === searchRequest.current) setCandidates([]);
        });
    }, SEARCH_DELAY_MS);
    return () => window.clearTimeout(timer);
  }, [courseId, canManage, search]);

  const add = async (email: string) => {
    const value = email.trim();
    if (!EMAIL_PATTERN.test(value)) {
      setErrors(["Введіть email викладача або оберіть його зі списку."]);
      return;
    }
    setIsAdding(true);
    setErrors([]);
    try {
      setTeachers(await CourseTeacherService.add(courseId, value));
      setSearch("");
      notification("success", "Викладача додано до курсу.");
    } catch (error) {
      setErrors(errorMessages(error));
    } finally {
      setIsAdding(false);
    }
  };

  const remove = async (teacher: CourseTeacher) => {
    setRemovingId(teacher.UserId);
    setErrors([]);
    try {
      await CourseTeacherService.remove(courseId, teacher.UserId);
      setTeachers((previous) => previous?.filter((item) => item.UserId !== teacher.UserId) ?? null);
      notification("success", "Викладача прибрано з курсу.");
    } catch (error) {
      setErrors(errorMessages(error));
    } finally {
      setRemovingId(null);
    }
  };

  const coTeacherCount = teachers?.filter((teacher) => !teacher.IsOwner).length ?? 0;
  const isSearchFinished = searchedTerm !== "" && searchedTerm === search.trim();

  return (
    <section className="course-teachers" aria-labelledby="course-teachers-title">
      <h3 id="course-teachers-title" className="course-teachers__title">Викладачі курсу</h3>
      <p className="course-teachers__lead">
        {canManage
          ? "Додані викладачі можуть редагувати матеріали та вправи. Видаляти курс і змінювати цей список можуть лише автор і адміністратор."
          : "Ці викладачі можуть редагувати курс. Змінювати список можуть лише автор і адміністратор."}
      </p>

      {errors.length > 0 && (
        <Alert
          type="error"
          showIcon
          className="course-teachers__alert"
          message={errors.length === 1 ? errors[0] : <ul className="form-errors">{errors.map((e) => <li key={e}>{e}</li>)}</ul>}
        />
      )}

      {teachers === null ? (
        <Skeleton active title={false} paragraph={{ rows: 2 }} />
      ) : (
        <ul className="course-teachers__list">
          {teachers.map((teacher) => (
            <li key={teacher.UserId} className="course-teachers__item">
              <Person person={teacher} />
              {teacher.IsOwner ? (
                <span className="course-teachers__badge">Автор</span>
              ) : (
                canManage && (
                  <Popconfirm
                    title="Прибрати викладача з курсу?"
                    description="Він більше не зможе редагувати цей курс."
                    okText="Прибрати"
                    okButtonProps={{ danger: true }}
                    cancelText="Скасувати"
                    onConfirm={() => remove(teacher)}
                  >
                    <Button type="text" danger size="small" loading={removingId === teacher.UserId}>
                      Прибрати
                    </Button>
                  </Popconfirm>
                )
              )}
            </li>
          ))}
          {coTeacherCount === 0 && <li className="course-teachers__empty">Співвикладачів ще немає.</li>}
        </ul>
      )}

      {canManage && (
        <>
          <div className="course-teachers__add">
            <AutoComplete
              className="course-teachers__search"
              value={search}
              options={candidates.map((candidate) => ({ value: candidate.Email, label: <Person person={candidate} /> }))}
              onChange={setSearch}
              onSelect={(value: string) => add(value)}
              notFoundContent={isSearchFinished ? "Нікого не знайдено" : undefined}
              placeholder="Email, прізвище або ім'я викладача"
              disabled={isAdding}
              aria-label="Пошук викладача"
            />
            <Button type="primary" icon={<UserAddOutlined />} loading={isAdding} onClick={() => add(search)}>
              Додати
            </Button>
          </div>
          <p className="course-teachers__hint">
            Знайти можна лише зареєстрованих користувачів з роллю «Викладач». Роль призначає адміністратор на сторінці «Користувачі».
          </p>
        </>
      )}
    </section>
  );
};

export default CourseTeachersPanel;
