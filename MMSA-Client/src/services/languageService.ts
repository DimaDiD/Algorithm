import Api from "./api-service";
import { ProgrammingLanguage } from "../models/ProgrammingLanguage";

export type NewProgrammingLanguage = Omit<ProgrammingLanguage, "Id">;

const LanguageService = {
  getAll: async (): Promise<ProgrammingLanguage[]> => (await Api.get("ProgrammingLanguages")).data,

  create: async (language: NewProgrammingLanguage): Promise<ProgrammingLanguage> =>
    (await Api.post("ProgrammingLanguages", language)).data,
};

export default LanguageService;
