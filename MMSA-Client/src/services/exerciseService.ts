import Api from "./api-service";
import {
  AiStatus,
  Exercise,
  ExerciseCheckResult,
  ExerciseEdit,
  ExerciseProgress,
  ExerciseStats,
  QuestionAnswer,
} from "../models/Exercise";
import { MoveDirection } from "../models/MoveDirection";
import { getClientId } from "./clientId";

let aiStatusRequest: Promise<AiStatus> | null = null;

const ExerciseService = {
  getAiStatus: (): Promise<AiStatus> => {
    aiStatusRequest =
      aiStatusRequest ??
      Api.get("Exercises/ai-status")
        .then((response) => response.data as AiStatus)
        .catch((error) => {
          aiStatusRequest = null;
          throw error;
        });
    return aiStatusRequest;
  },

  getForTopic: async (pageId: number, subPageId?: number): Promise<Exercise[]> =>
    (await Api.get("Exercises", { pageId, subPageId })).data,

  getForEdit: async (id: number): Promise<ExerciseEdit> =>
    (await Api.get(`Exercises/${id}/edit`)).data,

  create: async (exercise: ExerciseEdit): Promise<number> =>
    (await Api.post("Exercises", exercise)).data.id,

  update: async (exercise: ExerciseEdit): Promise<void> => {
    await Api.put(`Exercises/${exercise.Id}`, exercise);
  },

  move: async (id: number, direction: MoveDirection): Promise<void> => {
    await Api.put(`Exercises/${id}/move?direction=${direction}`);
  },

  deleteMyData: async (): Promise<number> =>
    (await Api.remove("Exercises/my-data", { clientId: getClientId() })).data.deleted,

  remove: async (id: number): Promise<void> => {
    await Api.remove(`Exercises/${id}`);
  },

  check: async (id: number, answers: QuestionAnswer[]): Promise<ExerciseCheckResult> =>
    (await Api.post(`Exercises/${id}/check`, { ClientId: getClientId(), Answers: answers })).data,

  getProgress: async (pageId: number, subPageId?: number): Promise<ExerciseProgress[]> =>
    (await Api.get("Exercises/progress", { pageId, subPageId, clientId: getClientId() })).data,

  getStats: async (id: number): Promise<ExerciseStats> => (await Api.get(`Exercises/${id}/stats`)).data,
};

export default ExerciseService;
