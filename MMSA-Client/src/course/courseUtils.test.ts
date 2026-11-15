import { pluralize, pluralWord, slugify, topicPath } from "./courseUtils";

describe("pluralWord", () => {
  it.each([
    [1, "тема"],
    [2, "теми"],
    [4, "теми"],
    [5, "тем"],
    [11, "тем"],
    [12, "тем"],
    [14, "тем"],
    [21, "тема"],
    [22, "теми"],
    [25, "тем"],
    [111, "тем"],
    [0, "тем"],
  ])("%i → %s", (count, expected) => {
    expect(pluralWord(count, "тема", "теми", "тем")).toBe(expected);
  });

  it("prefixes the count in pluralize", () => {
    expect(pluralize(3, "розділ", "розділи", "розділів")).toBe("3 розділи");
  });
});

describe("slugify", () => {
  it("transliterates Ukrainian text", () => {
    expect(slugify("Методи розробки алгоритмів")).toBe("metody-rozrobky-alhorytmiv");
  });

  it("drops apostrophes and collapses separators", () => {
    expect(slugify("  Об'єкти та  класи!  ")).toBe("obiekty-ta-klasy");
  });

  it("limits the length to 100 characters without a trailing dash", () => {
    const slug = slugify("а ".repeat(80));
    expect(slug.length).toBeLessThanOrEqual(100);
    expect(slug.endsWith("-")).toBe(false);
  });
});

describe("topicPath", () => {
  const page = { Id: 2, PageName: "1. Цикли", SubPages: [] };

  it("builds a section path", () => {
    expect(topicPath("algo", page)).toBe(`/courses/algo/${encodeURIComponent("1. Цикли")}`);
  });

  it("builds a topic path inside a section", () => {
    const subPage = { Id: 7, PageId: 2, Name: "1.1 While" };
    expect(topicPath("algo", page, subPage)).toBe(
      `/courses/algo/${encodeURIComponent("1. Цикли")}/${encodeURIComponent("1.1 While")}`
    );
  });
});
