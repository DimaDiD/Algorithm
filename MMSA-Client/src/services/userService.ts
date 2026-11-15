import Api from "./api-service";

export type UserRole = "Admin" | "Teacher" | "Reader";

export type UserListItem = {
  Id: string;
  Email: string;
  Role: UserRole;
  CourseCount: number;
  IsCurrentUser: boolean;
};

export type UserPage = {
  Items: UserListItem[];
  Total: number;
  Page: number;
  PageSize: number;
};

const UserService = {
  search: async (search: string, page: number): Promise<UserPage> =>
    (await Api.get("Users", { search: search || undefined, page })).data,

  changeRole: async (email: string, role: UserRole): Promise<void> => {
    await Api.put("Users/role", { Email: email, Role: role });
  },
};

export default UserService;
