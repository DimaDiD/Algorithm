import axios from "axios";
import { Action, createHook, createStore } from "react-sweet-state";
import { Page } from "./models/Page";
import { Course } from "./models/Course";
import MainService from "./services/mainService";
import LanguageService from "./services/languageService";
import CourseService from "./services/courseService";
import { DEFAULT_LANGUAGES, ProgrammingLanguage } from "./models/ProgrammingLanguage";

const CODE_OPTION_KEY = "mmsa.codeOption";
const DEFAULT_CODE_OPTION = "csharp";

const readCodeOption = (): string => {
  try {
    return localStorage.getItem(CODE_OPTION_KEY) ?? DEFAULT_CODE_OPTION;
  } catch {
    return DEFAULT_CODE_OPTION;
  }
};

const saveCodeOption = (value: string) => {
  try {
    localStorage.setItem(CODE_OPTION_KEY, value);
  } catch {
  }
};

export type CourseStatus = "idle" | "loading" | "ready" | "notFound" | "error";

type State = {
  codeOption: string;
  isEditMode: boolean;
  languages: ProgrammingLanguage[];
  course: Course | null;
  courseKey: string | null;
  courseStatus: CourseStatus;
  menu: Page[];
  isMenuLoaded: boolean;
};

const initialState: State = {
  codeOption: readCodeOption(),
  isEditMode: false,
  languages: DEFAULT_LANGUAGES,
  course: null,
  courseKey: null,
  courseStatus: "idle",
  menu: [],
  isMenuLoaded: false,
};

const api = new MainService();

const actions = {
  setCodeOption:
    (codeOption: string): Action<State> =>
    ({ setState }) => {
      saveCodeOption(codeOption);
      setState({ codeOption });
    },

  setEditMode:
    (isEditMode: boolean): Action<State> =>
    ({ setState }) => {
      setState({ isEditMode });
    },

  loadCourse:
    (slug: string, userKey: string): Action<State, void, Promise<void>> =>
    async ({ getState, setState }) => {
      const key = `${slug}|${userKey}`;
      const current = getState();
      if (current.courseKey === key && current.courseStatus !== "error") return;

      const isSameCourse = current.course?.Slug === slug;
      setState({
        courseKey: key,
        courseStatus: isSameCourse ? current.courseStatus : "loading",
        ...(isSameCourse ? {} : { course: null, menu: [], isMenuLoaded: false }),
      });

      try {
        const course = await CourseService.getBySlug(slug);
        if (getState().courseKey !== key) return;
        setState({ course, courseStatus: "ready" });

        const menu: Page[] = await api.getMenuItems(course.Id);
        if (getState().courseKey !== key) return;
        setState({ menu, isMenuLoaded: true });
      } catch (error) {
        if (getState().courseKey !== key) return;
        const isNotFound = axios.isAxiosError(error) && error.response?.status === 404;
        setState({ course: null, courseStatus: isNotFound ? "notFound" : "error", menu: [], isMenuLoaded: false });
      }
    },

  setCourse:
    (course: Course): Action<State> =>
    ({ setState }) => {
      setState({ course });
    },

  clearCourse:
    (): Action<State> =>
    ({ setState }) => {
      setState({ course: null, courseKey: null, courseStatus: "idle", menu: [], isMenuLoaded: false });
    },

  loadMenu:
    (): Action<State, void, Promise<void>> =>
    async ({ getState, setState }) => {
      const course = getState().course;
      if (!course) return;
      const menu: Page[] = await api.getMenuItems(course.Id);
      if (getState().course?.Id === course.Id) setState({ menu, isMenuLoaded: true });
    },

  loadLanguages:
    (): Action<State, void, Promise<void>> =>
    async ({ setState }) => {
      try {
        const languages = await LanguageService.getAll();
        if (languages.length > 0) setState({ languages });
      } catch {
      }
    },
};

const Store = createStore({ initialState, actions });
export const useTable = createHook(Store);
