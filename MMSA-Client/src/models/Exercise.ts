export type QuestionType = "SingleChoice" | "MultipleChoice" | "ShortAnswer" | "OpenAnswer" | "Code";

export type QuestionOption = {
  Id: number;
  Text: string;
};

export type Question = {
  Id: number;
  Type: QuestionType;
  Text: string;
  Points: number;
  Order: number;
  CodeLanguage?: string | null;
  StarterCode?: string | null;
  Options: QuestionOption[];
};

export type Exercise = {
  Id: number;
  Title: string;
  Description?: string | null;
  Order: number;
  Questions: Question[];
};

export type QuestionOptionEdit = {
  Id: number;
  Text: string;
  IsCorrect: boolean;
};

export type QuestionEdit = {
  Id: number;
  Type: QuestionType;
  Text: string;
  Points: number;
  CodeLanguage?: string | null;
  ReferenceAnswer?: string | null;
  Rubric?: string | null;
  StarterCode?: string | null;
  Options: QuestionOptionEdit[];
};

export type ExerciseEdit = {
  Id: number;
  PageId: number;
  SubPageId?: number | null;
  Title: string;
  Description?: string | null;
  Order: number;
  Questions: QuestionEdit[];
};

export type QuestionAnswer = {
  QuestionId: number;
  SelectedOptionIds: number[];
  Text?: string;
};

export type QuestionResultStatus = "Correct" | "PartiallyCorrect" | "Incorrect" | "NotChecked";

export type QuestionResult = {
  QuestionId: number;
  Status: QuestionResultStatus;
  EarnedPoints: number;
  MaxPoints: number;
  CorrectOptionIds: number[];
  AcceptedAnswers: string[];
  Feedback?: string | null;
  IsAiReviewed: boolean;
  Issues?: CodeIssue[];
};

export type CodeIssue = {
  Line: number;
  Message: string;
};

export type AiStatus = {
  Enabled: boolean;
  MaxAnswerLength: number;
  MaxCodeLength: number;
};

export type ExerciseCheckResult = {
  Score: number;
  MaxScore: number;
  Questions: QuestionResult[];
};

export type ExerciseProgress = {
  ExerciseId: number;
  Attempts: number;
  BestScore: number;
  LastScore: number;
  MaxScore: number;
  LastSubmittedAtUtc: string;
  TeacherReviews?: TeacherReview[];
};

export type TeacherReview = {
  QuestionId: number;
  Points: number;
  MaxPoints: number;
  IsOverridden: boolean;
  Comment?: string | null;
  ReviewedAtUtc: string;
};

export type OptionStats = {
  OptionId: number;
  Text: string;
  IsCorrect: boolean;
  Picks: number;
};

export type WrongAnswerStats = {
  Text: string;
  Count: number;
};

export type QuestionStats = {
  QuestionId: number;
  Type: QuestionType;
  Text: string;
  Answered: number;
  Correct: number;
  Partial: number;
  CorrectPercent: number | null;
  AveragePoints: number | null;
  MaxPoints: number;
  Options: OptionStats[];
  WrongAnswers: WrongAnswerStats[];
};

export type ExerciseStats = {
  ExerciseId: number;
  Title: string;
  Attempts: number;
  Students: number;
  AveragePercent: number | null;
  FirstAttemptAveragePercent: number | null;
  BestAveragePercent: number | null;
  LastSubmittedAtUtc: string | null;
  Questions: QuestionStats[];
};
