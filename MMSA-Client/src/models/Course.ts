import { ProgrammingLanguage } from "./ProgrammingLanguage";

export type Course = {
  Id: number;
  Title: string;
  Slug: string;
  Description?: string | null;
  IsPublished: boolean;
  CanEdit: boolean;
  CanManage: boolean;
  SectionCount: number;
  Languages: ProgrammingLanguage[];
};

export type CourseTeacher = {
  UserId: string;
  Email: string;
  LastName: string | null;
  FirstName: string | null;
  MiddleName: string | null;
  IsOwner: boolean;
};

export type CourseEdit = {
  Title: string;
  Slug: string;
  Description?: string | null;
  IsPublished: boolean;
  LanguageCodes: string[];
};
