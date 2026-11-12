import { useEffect, useMemo } from "react";
import { useAuth } from "../auth/AuthContext";
import { useTable } from "../Store";
import { ProgrammingLanguage } from "../models/ProgrammingLanguage";

export const useCanEditCourse = (): boolean => {
  const [state] = useTable();
  const { user } = useAuth();
  return !!user && !!state.course?.CanEdit;
};

export const useLoadCourse = (slug: string | undefined) => {
  const [state, actions] = useTable();
  const { user, isInitialized } = useAuth();
  const userKey = user?.id ?? "anonymous";

  useEffect(() => {
    if (slug && isInitialized) actions.loadCourse(slug, userKey);
  }, [slug, userKey, isInitialized, actions]);

  return state;
};

export const useCourseLanguages = (): ProgrammingLanguage[] => {
  const [state] = useTable();
  const courseLanguages = state.course?.Languages;
  return courseLanguages && courseLanguages.length > 0 ? courseLanguages : state.languages;
};

export const pickCodeOption = (codeOption: string, codes: string[]): string =>
  codes.includes(codeOption) || codes.length === 0 ? codeOption : codes[0];

export const useCodeOption = (): string => {
  const [state] = useTable();
  const languages = useCourseLanguages();
  return useMemo(
    () => pickCodeOption(state.codeOption, languages.map((language) => language.Code)),
    [state.codeOption, languages]
  );
};
