import { fullName, initials } from "./personName";

describe("fullName", () => {
  it("joins last, first and middle names", () => {
    expect(fullName({ LastName: "Коваль", FirstName: "Олена", MiddleName: "Петрівна" })).toBe("Коваль Олена Петрівна");
  });

  it("skips a missing middle name", () => {
    expect(fullName({ LastName: "Коваль", FirstName: "Олена", MiddleName: null })).toBe("Коваль Олена");
  });

  it("returns null when no name is set", () => {
    expect(fullName({ LastName: null, FirstName: "  ", MiddleName: undefined })).toBeNull();
  });
});

describe("initials", () => {
  it("uses the name when present", () => {
    expect(initials("олена Коваль", "x@y.ua")).toBe("О");
  });

  it("falls back to the email", () => {
    expect(initials(null, "teacher@school.ua")).toBe("T");
  });
});
