import { SubPage } from "./SubPages";

export type Page = {
    Id: number;
    PageName: string;
    SubPages: Array<SubPage>;
  };