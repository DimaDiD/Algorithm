import { MoveDirection } from "../models/MoveDirection";
import { PageContent } from "../models/PageContent";
import Api from "./api-service";

const withQuery = (path: string, params: Record<string, string | number>) =>
  `${path}?${new URLSearchParams(Object.entries(params).map(([key, value]) => [key, String(value)])).toString()}`;

export default class MainService {
  getMenuItems = async (courseId: number) => {
    return (await Api.get(`Page/GetMenuItems`, { courseId })).data;
  };

  CreatePage = async (courseId: number, pageName: string) => {
    return await Api.post(withQuery("Page/CreatePage", { courseId, pageName }));
  };

  RenamePage = async (pageId: number, newName: string) => {
    return await Api.put(withQuery("Page/RenamePage", { pageId, newName }));
  };

  DeletePage = async (pageId: number) => {
    return await Api.remove("Page/DeletePage", { pageId });
  };

  CreateSubPage = async (pageId: number, subPageName: string) => {
    return await Api.post(withQuery("SubPage/CreateSubPage", { pageId, subPageName }));
  };

  RenameSubPage = async (subPageId: number, newName: string) => {
    return await Api.put(withQuery("SubPage/RenameSubPage", { subPageId, newName }));
  };

  DeleteSubPage = async (subPageId: number) => {
    return await Api.remove("SubPage/DeleteSubPage", { subPageId });
  };

  ReorderPages = async (courseId: number, pageIds: number[]) => {
    return await Api.put(withQuery("Page/ReorderPages", { courseId }), pageIds);
  };

  ReorderSubPages = async (pageId: number, subPageIds: number[]) => {
    return await Api.put(withQuery("SubPage/ReorderSubPages", { pageId }), subPageIds);
  };

  GetPageContentByMenuStatus = async (pageId: number, codeStatus: string, subPageId?: number) => {
    return (await Api.get(`PageContent/GetPageContentByMenuStatus`, { pageId, codeStatus, subPageId })).data;
  };

  CreatePageContent = async (pageContent: PageContent) => {
    return await Api.post(`PageContent/CreatePageContent`, pageContent);
  };

  DeletePageContent = async (pageContentId: number) => {
    return await Api.remove(`PageContent/DeletePageContent`, { pageContentId });
  };

  UpdatePageContent = async (pageContent: PageContent) => {
    return await Api.put(`PageContent/UpdatePageContent`, pageContent);
  };

  MovePageContent = async (pageContentId: number, direction: MoveDirection, codeStatus: string) => {
    return await Api.put(withQuery("PageContent/MovePageContent", { pageContentId, direction, codeStatus }));
  };

  GetProgrammingTopics = async (): Promise<string[]> => {
    return (await Api.get(`PageContent/GetProgrammingTopics`)).data;
  };
}
