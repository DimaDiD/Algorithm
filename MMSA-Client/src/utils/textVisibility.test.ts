import { ALL_LANGUAGES, isVisibleFor, parseVisibility, serializeVisibility } from "./textVisibility";

describe("textVisibility", () => {
  it("parses a stored list of languages", () => {
    expect(parseVisibility("cpp, csharp,,pascal")).toEqual(["cpp", "csharp", "pascal"]);
    expect(parseVisibility(undefined)).toEqual([]);
  });

  it("switches to all languages when All is picked last", () => {
    expect(serializeVisibility(["cpp", ALL_LANGUAGES])).toBe(ALL_LANGUAGES);
  });

  it("drops All when a specific language is picked after it", () => {
    expect(serializeVisibility([ALL_LANGUAGES, "cpp"])).toBe("cpp");
    expect(serializeVisibility(["cpp", "pascal"])).toBe("cpp,pascal");
  });

  it("returns an empty value when nothing is selected", () => {
    expect(serializeVisibility([])).toBe("");
  });

  it("checks visibility for a language", () => {
    expect(isVisibleFor("All", "pharo")).toBe(true);
    expect(isVisibleFor("cpp,csharp", "csharp")).toBe(true);
    expect(isVisibleFor("cpp,csharp", "pharo")).toBe(false);
  });
});
