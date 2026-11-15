import { Fragment, useCallback, useEffect, useMemo, useRef, useState } from "react";
import { Modal, Skeleton } from "antd";
import { ArrowDownOutlined, ArrowUpOutlined, EditOutlined } from "@ant-design/icons";
import axios from "axios";
import { MoveDirection } from "../../models/MoveDirection";
import { PageContent } from "../../models/PageContent";
import { useCanEditCourse, useCodeOption, useCourseLanguages } from "../../course/useCourse";
import { useTable } from "../../Store";
import MainService from "../../services/mainService";
import notification from "../../notification/notificationMessage";
import CodeContent from "../../PageContentTypes/CodeContent";
import TextContent from "../../PageContentTypes/TextContent";
import ContentEditorModal, { ContentFormValues } from "./ContentEditorModal";
import { BlockErrorBoundary } from "../common/ErrorFallbacks";

type Props = {
  pageId: number;
  subPageId?: number;
};

type EditorState =
  | { mode: "add"; position: number; initialValues: Partial<ContentFormValues> }
  | { mode: "edit"; block: PageContent; initialValues: Partial<ContentFormValues> };

const HANDLED_STATUSES = [401, 403, 429];

const notifyError = (error: unknown, message: string) => {
  if (axios.isAxiosError(error) && HANDLED_STATUSES.includes(error.response?.status ?? 0)) return;
  notification("error", message);
};

const InsertBar = ({ onInsert, position }: { onInsert: (type: ContentFormValues["Type"]) => void; position: string }) => (
  <div className="insert-bar">
    <span className="insert-bar__line" />
    <button type="button" className="insert-bar__button" onClick={() => onInsert("Text")} aria-label={`Вставити текст ${position}`}>
      + Текст
    </button>
    <button
      type="button"
      className="insert-bar__button insert-bar__button--code"
      onClick={() => onInsert("Code")}
      aria-label={`Вставити код ${position}`}
    >
      + Код
    </button>
    <span className="insert-bar__line" />
  </div>
);

const TopicContent = ({ pageId, subPageId }: Props) => {
  const api = useMemo(() => new MainService(), []);
  const [state] = useTable();
  const canEdit = useCanEditCourse();
  const [modal, contextHolder] = Modal.useModal();
  const [blocks, setBlocks] = useState<PageContent[]>([]);
  const [hasLoaded, setHasLoaded] = useState(false);
  const [isRefreshing, setIsRefreshing] = useState(false);
  const cache = useRef(new Map<string, PageContent[]>());
  const [editor, setEditor] = useState<EditorState | null>(null);

  const isEditing = canEdit && state.isEditMode;
  const codeOption = useCodeOption();
  const courseLanguages = useCourseLanguages();

  const fetchContent = useCallback(
    async (language: string): Promise<PageContent[]> => {
      const content: PageContent[] = await api.GetPageContentByMenuStatus(pageId, language, subPageId);
      cache.current.set(language, content);
      return content;
    },
    [api, pageId, subPageId]
  );

  const load = useCallback(async () => {
    cache.current.clear();
    setBlocks(await fetchContent(codeOption));
  }, [fetchContent, codeOption]);

  useEffect(() => {
    const cached = cache.current.get(codeOption);
    if (cached) {
      setBlocks(cached);
      return;
    }

    let isCancelled = false;
    setIsRefreshing(true);
    fetchContent(codeOption)
      .then((content) => {
        if (!isCancelled) setBlocks(content);
      })
      .catch(() => {
        if (!isCancelled) setBlocks([]);
      })
      .finally(() => {
        if (!isCancelled) {
          setIsRefreshing(false);
          setHasLoaded(true);
        }
      });
    return () => {
      isCancelled = true;
    };
  }, [fetchContent, codeOption]);

  const languageCodes = useMemo(() => courseLanguages.map((language) => language.Code).join("|"), [courseLanguages]);

  useEffect(() => {
    if (!hasLoaded) return;
    languageCodes
      .split("|")
      .filter((language) => language && !cache.current.has(language))
      .forEach((language) => fetchContent(language).catch(() => undefined));
  }, [hasLoaded, fetchContent, languageCodes]);

  const openAdd = (position: number, type: ContentFormValues["Type"]) =>
    setEditor({
      mode: "add",
      position,
      initialValues: { Type: type, CodeType: type === "Code" ? codeOption : "All" },
    });

  const openEdit = (block: PageContent) =>
    setEditor({
      mode: "edit",
      block,
      initialValues: {
        Type: block.TextType === "Code" ? "Code" : "Text",
        CodeType: block.CodeType,
        Topics: block.Topics ?? [],
        Text: block.Text,
      },
    });

  const submit = async (values: ContentFormValues) => {
    if (!editor) return;
    try {
      if (editor.mode === "add") {
        const previous = blocks[editor.position - 1];
        await api.CreatePageContent({
          Id: 0,
          Text: values.Text,
          SubPageId: subPageId,
          TextType: values.Type,
          CodeType: values.CodeType,
          Topics: values.Type === "Code" ? values.Topics ?? [] : [],
          ContentLocation: previous ? previous.ContentLocation + 1 : 0,
          PageId: pageId,
        });
        notification("success", "Блок додано.");
      } else {
        await api.UpdatePageContent({
          ...editor.block,
          Text: values.Text,
          TextType: values.Type,
          CodeType: values.CodeType,
          Topics: values.Type === "Code" ? values.Topics ?? [] : [],
        });
        notification("success", "Зміни збережено.");
      }
      setEditor(null);
      await load();
    } catch (error) {
      notifyError(error, "Не вдалося зберегти блок. Спробуйте ще раз.");
      throw error;
    }
  };

  const move = async (block: PageContent, direction: MoveDirection) => {
    try {
      await api.MovePageContent(block.Id, direction, codeOption);
      await load();
    } catch (error) {
      notifyError(error, "Не вдалося перемістити блок.");
    }
  };

  const confirmDelete = (block: PageContent) =>
    modal.confirm({
      title: "Видалити блок?",
      content: "Цю дію не можна скасувати.",
      okText: "Видалити",
      okButtonProps: { danger: true },
      cancelText: "Скасувати",
      onOk: async () => {
        try {
          await api.DeletePageContent(block.Id);
          notification("success", "Блок видалено.");
          await load();
        } catch (error) {
          notifyError(error, "Не вдалося видалити блок.");
        }
      },
    });

  if (!hasLoaded) {
    return <Skeleton active paragraph={{ rows: 8 }} />;
  }

  return (
    <div className={`topic-content${isRefreshing ? " is-refreshing" : ""}`} aria-busy={isRefreshing}>
      {isEditing && (
        <div className="edit-banner" role="status">
          <EditOutlined />
          <span>Ви редагуєте тему. Наведіть на блок, щоб змінити його, або вставте новий кнопками «+ Текст» і «+ Код».</span>
        </div>
      )}

      {blocks.length === 0 && (
        <div className="empty-state">
          <span>У цій темі ще немає матеріалів для обраної мови.</span>
        </div>
      )}

      {blocks.map((block, index) => (
        <Fragment key={block.Id}>
          {isEditing && <InsertBar position={`перед блоком ${index + 1}`} onInsert={(type) => openAdd(index, type)} />}
          <div className={`content-block${isEditing ? " is-editing" : ""}`}>
            {isEditing && (
              <div className="block-toolbar">
                <button
                  type="button"
                  className="block-toolbar__button block-toolbar__button--icon"
                  disabled={index === 0}
                  onClick={() => move(block, "up")}
                  aria-label="Перемістити блок вище"
                  title="Вище"
                >
                  <ArrowUpOutlined />
                </button>
                <button
                  type="button"
                  className="block-toolbar__button block-toolbar__button--icon"
                  disabled={index === blocks.length - 1}
                  onClick={() => move(block, "down")}
                  aria-label="Перемістити блок нижче"
                  title="Нижче"
                >
                  <ArrowDownOutlined />
                </button>
                <button type="button" className="block-toolbar__button" onClick={() => openEdit(block)}>
                  Редагувати
                </button>
                <button
                  type="button"
                  className="block-toolbar__button block-toolbar__button--danger"
                  onClick={() => confirmDelete(block)}
                >
                  Видалити
                </button>
              </div>
            )}
            <BlockErrorBoundary resetKey={block}>
              {block.TextType === "Code" ? (
                <CodeContent code={block.Text} language={block.CodeType} topics={block.Topics} />
              ) : (
                <TextContent text={block.Text} />
              )}
            </BlockErrorBoundary>
          </div>
        </Fragment>
      ))}

      {isEditing && <InsertBar position="в кінець теми" onInsert={(type) => openAdd(blocks.length, type)} />}

      <ContentEditorModal
        open={editor !== null}
        mode={editor?.mode ?? "add"}
        initialValues={editor?.initialValues}
        onCancel={() => setEditor(null)}
        onSubmit={submit}
      />
      {contextHolder}
    </div>
  );
};

export default TopicContent;
