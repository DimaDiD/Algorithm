export const Roles = {
  Admin: "Admin",
  Teacher: "Teacher",
  Reader: "Reader",
} as const;

export const EDITOR_ROLES: string[] = [Roles.Admin, Roles.Teacher];
