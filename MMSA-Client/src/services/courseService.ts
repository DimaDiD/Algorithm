import Api from "./api-service";
import { Course, CourseEdit } from "../models/Course";

const CourseService = {
  getPublished: async (): Promise<Course[]> => (await Api.get("Courses")).data,

  getMine: async (): Promise<Course[]> => (await Api.get("Courses/mine")).data,

  getBySlug: async (slug: string): Promise<Course> =>
    (await Api.get(`Courses/by-slug/${encodeURIComponent(slug)}`)).data,

  create: async (course: CourseEdit): Promise<Course> => (await Api.post("Courses", course)).data,

  update: async (id: number, course: CourseEdit): Promise<void> => {
    await Api.put(`Courses/${id}`, course);
  },

  remove: async (id: number): Promise<void> => {
    await Api.remove(`Courses/${id}`);
  },
};

export default CourseService;
