import Api from "./api-service";
import { CourseTeacher } from "../models/Course";

const basePath = (courseId: number) => `Courses/${courseId}/teachers`;

const CourseTeacherService = {
  getAll: async (courseId: number): Promise<CourseTeacher[]> => (await Api.get(basePath(courseId))).data,

  searchCandidates: async (courseId: number, search: string): Promise<CourseTeacher[]> =>
    (await Api.get(`${basePath(courseId)}/candidates`, { search })).data,

  add: async (courseId: number, email: string): Promise<CourseTeacher[]> =>
    (await Api.post(basePath(courseId), { Email: email })).data,

  remove: async (courseId: number, userId: string): Promise<void> => {
    await Api.remove(`${basePath(courseId)}/${encodeURIComponent(userId)}`);
  },
};

export default CourseTeacherService;
