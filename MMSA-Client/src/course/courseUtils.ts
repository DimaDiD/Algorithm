import { Page } from "../models/Page";
import { SubPage } from "../models/SubPages";

export type Topic = {
  page: Page;
  subPage?: SubPage;
  title: string;
  path: string;
};

export const coursePath = (courseSlug: string): string => `/courses/${encodeURIComponent(courseSlug)}`;

export const topicPath = (courseSlug: string, page: Page, subPage?: SubPage): string =>
  subPage
    ? `${coursePath(courseSlug)}/${encodeURIComponent(page.PageName)}/${encodeURIComponent(subPage.Name)}`
    : `${coursePath(courseSlug)}/${encodeURIComponent(page.PageName)}`;

const toTopic = (courseSlug: string, page: Page, subPage?: SubPage): Topic => ({
  page,
  subPage,
  title: subPage?.Name ?? page.PageName,
  path: topicPath(courseSlug, page, subPage),
});

export const sectionTopics = (courseSlug: string, page: Page): Topic[] =>
  page.SubPages.length === 0
    ? [toTopic(courseSlug, page)]
    : page.SubPages.map((subPage) => toTopic(courseSlug, page, subPage));

export const flattenTopics = (courseSlug: string, menu: Page[]): Topic[] =>
  menu.flatMap((page) => sectionTopics(courseSlug, page));

export const findTopic = (
  courseSlug: string,
  menu: Page[],
  pageName?: string,
  subPageName?: string
): Topic | undefined => {
  const page = menu.find((item) => item.PageName === pageName);
  if (!page) return undefined;

  if (!subPageName) return toTopic(courseSlug, page);

  const subPage = page.SubPages.find((item) => item.Name === subPageName);
  return subPage ? toTopic(courseSlug, page, subPage) : undefined;
};

export const swapped = <T,>(items: T[], index: number, offset: number): T[] => {
  const next = [...items];
  [next[index], next[index + offset]] = [next[index + offset], next[index]];
  return next;
};

export const pluralWord = (count: number, one: string, few: string, many: string): string => {
  const lastTwo = count % 100;
  const last = count % 10;
  if (lastTwo >= 11 && lastTwo <= 14) return many;
  if (last === 1) return one;
  if (last >= 2 && last <= 4) return few;
  return many;
};

export const pluralize = (count: number, one: string, few: string, many: string): string =>
  `${count} ${pluralWord(count, one, few, many)}`;

export const pluralizeTopics = (count: number): string => pluralize(count, "тема", "теми", "тем");

const TRANSLIT: Record<string, string> = {
  а: "a", б: "b", в: "v", г: "h", ґ: "g", д: "d", е: "e", є: "ie", ж: "zh", з: "z", и: "y", і: "i", ї: "i", й: "i",
  к: "k", л: "l", м: "m", н: "n", о: "o", п: "p", р: "r", с: "s", т: "t", у: "u", ф: "f", х: "kh", ц: "ts", ч: "ch",
  ш: "sh", щ: "shch", ь: "", ю: "iu", я: "ia", ы: "y", э: "e", ё: "io", ъ: "",
};

export const slugify = (value: string): string =>
  value
    .toLowerCase()
    .split("")
    .map((char) => TRANSLIT[char] ?? char)
    .join("")
    .replace(/['’ʼ]/g, "")
    .replace(/[^a-z0-9]+/g, "-")
    .replace(/^-+|-+$/g, "")
    .slice(0, 100)
    .replace(/-+$/g, "");
