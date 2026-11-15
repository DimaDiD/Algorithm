import { useEffect, useMemo, useState } from "react";
import { Dropdown, MenuProps, Modal } from "antd";
import { ArrowDownOutlined, ArrowUpOutlined, DeleteOutlined, EditOutlined, MoreOutlined, PlusOutlined, RightOutlined } from "@ant-design/icons";
import axios from "axios";
import { Link, useNavigate } from "react-router-dom";
import { Page } from "../../models/Page";
import { SubPage } from "../../models/SubPages";
import { coursePath, swapped, Topic, topicPath } from "../../course/courseUtils";
import { useCanEditCourse } from "../../course/useCourse";
import { useTable } from "../../Store";
import MainService from "../../services/mainService";
import notification from "../../notification/notificationMessage";
import NameModal from "./NameModal";

type Props = {
  courseId: number;
  courseSlug: string;
  menu: Page[];
  activeTopic?: Topic;
  onNavigate?: () => void;
};

type NameDialog =
  | { kind: "addSection" }
  | { kind: "addTopic"; page: Page }
  | { kind: "rename"; page: Page; subPage?: SubPage };

const HANDLED_STATUSES = [401, 403, 429];

const notifyError = (error: unknown) => {
  if (axios.isAxiosError(error) && HANDLED_STATUSES.includes(error.response?.status ?? 0)) return;
  notification("error", "Не вдалося зберегти зміни. Спробуйте ще раз.");
};

type MenuItems = NonNullable<MenuProps["items"]>;

const moveActions = (index: number, count: number): MenuItems => [
  { key: "up", label: "Перемістити вище", icon: <ArrowUpOutlined />, disabled: index === 0 },
  { key: "down", label: "Перемістити нижче", icon: <ArrowDownOutlined />, disabled: index === count - 1 },
];

const sectionActions = (index: number, count: number): MenuItems => [
  { key: "rename", label: "Перейменувати", icon: <EditOutlined /> },
  { key: "addTopic", label: "Додати тему", icon: <PlusOutlined /> },
  ...(count > 1 ? [{ type: "divider" as const }, ...moveActions(index, count)] : []),
  { type: "divider" },
  { key: "delete", label: "Видалити розділ", icon: <DeleteOutlined />, danger: true },
];

const topicActions = (index: number, count: number): MenuItems => [
  { key: "rename", label: "Перейменувати", icon: <EditOutlined /> },
  ...(count > 1 ? [{ type: "divider" as const }, ...moveActions(index, count)] : []),
  { type: "divider" },
  { key: "delete", label: "Видалити тему", icon: <DeleteOutlined />, danger: true },
];

const CourseNav = ({ courseId, courseSlug, menu, activeTopic, onNavigate }: Props) => {
  const [state, actions] = useTable();
  const canEdit = useCanEditCourse();
  const navigate = useNavigate();
  const api = useMemo(() => new MainService(), []);
  const [modal, contextHolder] = Modal.useModal();
  const [nameDialog, setNameDialog] = useState<NameDialog | null>(null);
  const [expanded, setExpanded] = useState<Set<number>>(
    () => new Set(activeTopic ? [activeTopic.page.Id] : [])
  );

  const isEditing = canEdit && state.isEditMode;
  const activePageId = activeTopic?.page.Id;

  useEffect(() => {
    if (activePageId === undefined) return;
    setExpanded((previous) => (previous.has(activePageId) ? previous : new Set(previous).add(activePageId)));
  }, [activePageId]);

  const toggle = (pageId: number) =>
    setExpanded((previous) => {
      const next = new Set(previous);
      if (next.has(pageId)) next.delete(pageId);
      else next.add(pageId);
      return next;
    });

  const isActive = (page: Page, subPage?: SubPage) =>
    activeTopic?.page.Id === page.Id && activeTopic?.subPage?.Id === subPage?.Id;

  const submitName = async (name: string) => {
    if (!nameDialog) return;
    try {
      let redirectTo: string | undefined;

      if (nameDialog.kind === "addSection") {
        await api.CreatePage(courseId, name);
      } else if (nameDialog.kind === "addTopic") {
        await api.CreateSubPage(nameDialog.page.Id, name);
        setExpanded((previous) => new Set(previous).add(nameDialog.page.Id));
      } else {
        const { page, subPage } = nameDialog;
        if (subPage) await api.RenameSubPage(subPage.Id, name);
        else await api.RenamePage(page.Id, name);

        if (activeTopic && activeTopic.page.Id === page.Id) {
          if (!subPage) {
            redirectTo = topicPath(courseSlug, { ...page, PageName: name }, activeTopic.subPage);
          } else if (activeTopic.subPage?.Id === subPage.Id) {
            redirectTo = topicPath(courseSlug, page, { ...subPage, Name: name });
          }
        }
      }

      await actions.loadMenu();
      if (redirectTo) navigate(redirectTo, { replace: true });
      setNameDialog(null);
      notification("success", "Зміни збережено.");
    } catch (error) {
      notifyError(error);
      throw error;
    }
  };

  const confirmDelete = (page: Page, subPage?: SubPage) => {
    const title = subPage?.Name ?? page.PageName;
    modal.confirm({
      title: `Видалити «${title}»?`,
      content: "Цю дію не можна скасувати.",
      okText: "Видалити",
      okButtonProps: { danger: true },
      cancelText: "Скасувати",
      onOk: async () => {
        try {
          if (subPage) await api.DeleteSubPage(subPage.Id);
          else await api.DeletePage(page.Id);
          const wasActive = activeTopic?.page.Id === page.Id && (!subPage || activeTopic?.subPage?.Id === subPage.Id);
          await actions.loadMenu();
          if (wasActive) navigate(coursePath(courseSlug));
          notification("success", "Видалено.");
        } catch (error) {
          notifyError(error);
        }
      },
    });
  };

  const move = async (save: () => Promise<unknown>) => {
    try {
      await save();
      await actions.loadMenu();
    } catch (error) {
      notifyError(error);
    }
  };

  const moveSection = (index: number, offset: number) =>
    move(() => api.ReorderPages(courseId, swapped(menu, index, offset).map((page) => page.Id)));

  const moveTopic = (page: Page, index: number, offset: number) =>
    move(() => api.ReorderSubPages(page.Id, swapped(page.SubPages, index, offset).map((subPage) => subPage.Id)));

  const handleSectionAction = (key: string, page: Page, index: number) => {
    if (key === "up") moveSection(index, -1);
    if (key === "down") moveSection(index, 1);
    if (key === "rename") setNameDialog({ kind: "rename", page });
    if (key === "addTopic") setNameDialog({ kind: "addTopic", page });
    if (key === "delete") confirmDelete(page);
  };

  const handleTopicAction = (key: string, page: Page, subPage: SubPage, index: number) => {
    if (key === "up") moveTopic(page, index, -1);
    if (key === "down") moveTopic(page, index, 1);
    if (key === "rename") setNameDialog({ kind: "rename", page, subPage });
    if (key === "delete") confirmDelete(page, subPage);
  };

  const dialogProps = (() => {
    if (!nameDialog) return { title: "", label: "", okText: "", initialValue: "" };
    if (nameDialog.kind === "addSection") {
      return { title: "Новий розділ", label: "Назва розділу", okText: "Додати", initialValue: "" };
    }
    if (nameDialog.kind === "addTopic") {
      return { title: `Нова тема в «${nameDialog.page.PageName}»`, label: "Назва теми", okText: "Додати", initialValue: "" };
    }
    return {
      title: "Перейменувати",
      label: "Нова назва",
      okText: "Зберегти",
      initialValue: nameDialog.subPage?.Name ?? nameDialog.page.PageName,
    };
  })();

  return (
    <nav className="course-nav" aria-label="Зміст курсу">
      <span className="course-nav__label">Зміст</span>

      {menu.map((page, pageIndex) => {
        const hasTopics = page.SubPages.length > 0;
        const isExpanded = expanded.has(page.Id);
        const isSectionActive = isActive(page);

        return (
          <div key={page.Id}>
            <div className="course-nav__row">
              {hasTopics ? (
                <button
                  type="button"
                  className="course-nav__section"
                  aria-expanded={isExpanded}
                  onClick={() => toggle(page.Id)}
                >
                  <span>{page.PageName}</span>
                  <RightOutlined className="course-nav__chevron" />
                </button>
              ) : (
                <Link
                  to={topicPath(courseSlug, page)}
                  className={`course-nav__section${isSectionActive ? " is-active" : ""}`}
                  aria-current={isSectionActive ? "page" : undefined}
                  onClick={onNavigate}
                >
                  {page.PageName}
                </Link>
              )}

              {isEditing && (
                <Dropdown
                  trigger={["click"]}
                  menu={{ items: sectionActions(pageIndex, menu.length), onClick: ({ key }) => handleSectionAction(key, page, pageIndex) }}
                >
                  <button type="button" className="course-nav__more" aria-label={`Дії з розділом «${page.PageName}»`}>
                    <MoreOutlined />
                  </button>
                </Dropdown>
              )}
            </div>

            {hasTopics && isExpanded && (
              <div className="course-nav__topics">
                {page.SubPages.map((subPage, topicIndex) => {
                  const isTopicActive = isActive(page, subPage);
                  return (
                    <div key={subPage.Id} className="course-nav__row">
                      <Link
                        to={topicPath(courseSlug, page, subPage)}
                        className={`course-nav__link${isTopicActive ? " is-active" : ""}`}
                        aria-current={isTopicActive ? "page" : undefined}
                        onClick={onNavigate}
                      >
                        {subPage.Name}
                      </Link>
                      {isEditing && (
                        <Dropdown
                          trigger={["click"]}
                          menu={{
                            items: topicActions(topicIndex, page.SubPages.length),
                            onClick: ({ key }) => handleTopicAction(key, page, subPage, topicIndex),
                          }}
                        >
                          <button type="button" className="course-nav__more" aria-label={`Дії з темою «${subPage.Name}»`}>
                            <MoreOutlined />
                          </button>
                        </Dropdown>
                      )}
                    </div>
                  );
                })}
                {isEditing && (
                  <button type="button" className="dashed-button" onClick={() => setNameDialog({ kind: "addTopic", page })}>
                    + Додати тему
                  </button>
                )}
              </div>
            )}
          </div>
        );
      })}

      {isEditing && (
        <button
          type="button"
          className="dashed-button course-nav__add-section"
          onClick={() => setNameDialog({ kind: "addSection" })}
        >
          + Додати розділ
        </button>
      )}

      <NameModal
        open={nameDialog !== null}
        title={dialogProps.title}
        label={dialogProps.label}
        okText={dialogProps.okText}
        initialValue={dialogProps.initialValue}
        onCancel={() => setNameDialog(null)}
        onSubmit={submitName}
      />
      {contextHolder}
    </nav>
  );
};

export default CourseNav;
