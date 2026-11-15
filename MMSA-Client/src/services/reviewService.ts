import Api from "./api-service";
import { CourseDashboard, ReviewFilter, ReviewPage, ReviewUpdate } from "../models/Review";

const ReviewService = {
  getDashboard: async (courseId: number): Promise<CourseDashboard> =>
    (await Api.get(`Courses/${courseId}/dashboard`)).data,

  getReviews: async (
    courseId: number,
    filter: { exerciseId?: number; status: ReviewFilter; page: number }
  ): Promise<ReviewPage> =>
    (await Api.get(`Courses/${courseId}/reviews`, filter)).data,

  review: async (answerId: number, update: ReviewUpdate): Promise<void> => {
    await Api.put(`Reviews/${answerId}`, update);
  },
};

export default ReviewService;
