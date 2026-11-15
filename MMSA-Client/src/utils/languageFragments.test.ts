import { resolveLanguageFragments } from "./languageFragments";

const sample = "<p>Остача: {{csharp,cpp:n % 10}}{{pascal:n mod 10}}.</p>";

describe("resolveLanguageFragments", () => {
  it("shows the variant for the selected language", () => {
    expect(resolveLanguageFragments(sample, "pascal")).toBe('<p>Остача: <code class="code-fragment">n mod 10</code>.</p>');
    expect(resolveLanguageFragments(sample, "cpp")).toBe('<p>Остача: <code class="code-fragment">n % 10</code>.</p>');
  });

  it("falls back to the first variant when the language is missing", () => {
    expect(resolveLanguageFragments(sample, "python")).toBe('<p>Остача: <code class="code-fragment">n % 10</code>.</p>');
  });

  it("treats variants separated by spaces as one fragment", () => {
    expect(resolveLanguageFragments("{{cpp:a &amp;&amp; b}} {{pascal:a and b}}", "PASCAL")).toBe(
      '<code class="code-fragment">a and b</code>'
    );
  });

  it("resolves several fragments independently", () => {
    const html = "{{cpp:x++}}{{pascal:Inc(x)}} і {{cpp:x--}}{{pascal:Dec(x)}}";
    expect(resolveLanguageFragments(html, "pascal")).toBe(
      '<code class="code-fragment">Inc(x)</code> і <code class="code-fragment">Dec(x)</code>'
    );
  });

  it("leaves ordinary braces untouched", () => {
    const html = "<p>Множина {{1, 2}} і {{ not a fragment }}</p>";
    expect(resolveLanguageFragments(html, "cpp")).toBe(html);
  });
});
