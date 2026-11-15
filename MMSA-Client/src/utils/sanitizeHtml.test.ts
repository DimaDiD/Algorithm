import { sanitizeHtml } from "./sanitizeHtml";

describe("sanitizeHtml", () => {
  it.each([
    ["<script>alert(1)</script><p>ok</p>", "<p>ok</p>"],
    ['<img src="x" onerror="alert(1)">', '<img src="x">'],
    ['<a href="javascript:alert(1)">link</a>', "<a>link</a>"],
    ["<iframe src=\"https://example.com\"></iframe>", ""],
    ['<svg onload="alert(1)"></svg>', ""],
  ])("neutralises %s", (input, expected) => {
    expect(sanitizeHtml(input)).toBe(expected);
  });

  it("keeps editor formatting", () => {
    const html = '<h2>Тема</h2><p><strong>жирний</strong> <em>курсив</em></p><ol><li>крок</li></ol>';
    expect(sanitizeHtml(html)).toBe(html);
  });

  it("keeps target=_blank on links and adds rel", () => {
    expect(sanitizeHtml('<a href="https://example.com" target="_blank">x</a>')).toBe(
      '<a href="https://example.com" target="_blank" rel="noopener noreferrer">x</a>'
    );
  });

  it("drops other target values", () => {
    expect(sanitizeHtml('<a href="https://example.com" target="evil">x</a>')).toBe('<a href="https://example.com">x</a>');
    expect(sanitizeHtml('<p target="_blank">x</p>')).toBe("<p>x</p>");
  });
});
