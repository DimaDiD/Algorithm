import { ProgrammingLanguage } from "./ProgrammingLanguage";

export type Course = {
  Id: number;
  Title: string;
  Slug: string;
  Description?: string | null;
  IsPublished: boolean;
  CanEdit: boolean;
  SectionCount: number;
  Languages: ProgrammingLanguage[];
};

export type CourseEdit = {
  Title: string;
  Slug: string;
  Description?: string | null;
  IsPublished: boolean;
  LanguageCodes: string[];
};
