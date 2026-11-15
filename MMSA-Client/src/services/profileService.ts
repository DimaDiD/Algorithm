import Api from "./api-service";

export type Profile = {
  Email: string;
  LastName: string | null;
  FirstName: string | null;
  MiddleName: string | null;
};

export type ProfileUpdate = Omit<Profile, "Email">;

const ProfileService = {
  get: async (): Promise<Profile> => (await Api.get("Profile")).data,

  update: async (profile: ProfileUpdate): Promise<void> => {
    await Api.put("Profile", profile);
  },
};

export default ProfileService;
