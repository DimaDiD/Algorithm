export type DashboardExercise = {
  ExerciseId: number;
  Title: string;
  PageId: number;
  PageName: string;
  SubPageId?: number | null;
  SubPageName?: string | null;
  Attempts: number;
  Students: number;
  MaxScore: number | null;
  FirstAttemptAverageScore: number | null;
  FirstAttemptAveragePercent: number | null;
  BestAverageScore: number | null;
  BestAveragePercent: number | null;
  PendingReviews: number;
};

export type CourseDashboard = {
  CourseId: number;
  Students: number;
  Attempts: number;
  FirstAttemptAveragePercent: number | null;
  BestAveragePercent: number | null;
  PendingReviews: number;
  LastSubmittedAtUtc: string | null;
  Exercises: DashboardExercise[];
};

export type ReviewStatus = "Pending" | "Confirmed" | "Overridden";

export type ReviewFilter = "Pending" | "Reviewed" | "all";

export type ReviewItem = {
  AnswerId: number;
  ExerciseId: number;
  ExerciseTitle: string;
  TopicTitle: string;
  StudentLabel: string;
  SubmittedAtUtc: string;
  QuestionId: number;
  QuestionText?: string | null;
  QuestionType?: string | null;
  CodeLanguage?: string | null;
  AnswerText?: string | null;
  AiFeedback?: string | null;
  AiPoints?: number | null;
  EarnedPoints: number;
  MaxPoints: number;
  Status: string;
  ReviewStatus: ReviewStatus;
  TeacherComment?: string | null;
  ReviewedAtUtc?: string | null;
};

export type ReviewPage = {
  Items: ReviewItem[];
  Total: number;
  Page: number;
  PageSize: number;
};

export type ReviewUpdate = {
  Action: "confirm" | "override";
  Points?: number;
  Comment?: string | null;
};
