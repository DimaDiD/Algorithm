import { useEffect, useRef, useState } from "react";
import { Alert, Button, Form, Input, Modal, Switch } from "antd";
import { DeleteOutlined } from "@ant-design/icons";
import { Course, CourseEdit } from "../../models/Course";
import CourseService from "../../services/courseService";
import { getErrorMessages } from "../../auth/authApi";
import { slugify } from "../../course/courseUtils";
import notification from "../../notification/notificationMessage";
import { useTable } from "../../Store";
import CourseLanguagePicker from "./CourseLanguagePicker";

type Props = {
  open: boolean;
  course?: Course | null;
  onClose: () => void;
  onSaved: (course: Course) => void;
  onDeleted?: () => void;
};

const SLUG_PATTERN = /^[a-z0-9]+(-[a-z0-9]+)*$/;

const CourseEditorModal = ({ open, course, onClose, onSaved, onDeleted }: Props) => {
  const [state] = useTable();
  const [form] = Form.useForm<CourseEdit>();
  const [modal, contextHolder] = Modal.useModal();
  const [isSaving, setIsSaving] = useState(false);
  const [errors, setErrors] = useState<string[]>([]);
  const isSlugTouched = useRef(false);
  const defaultCodeRef = useRef<string | undefined>();
  defaultCodeRef.current = state.languages.some((language) => language.Code === state.codeOption)
    ? state.codeOption
    : state.languages[0]?.Code;
  const isEdit = !!course;

  useEffect(() => {
    if (!open) return;
    setErrors([]);
    isSlugTouched.current = false;
    const defaultCode = defaultCodeRef.current;
    form.setFieldsValue(
      course
        ? {
            Title: course.Title,
            Slug: course.Slug,
            Description: course.Description ?? "",
            IsPublished: course.IsPublished,
            LanguageCodes: course.Languages.map((language) => language.Code),
          }
        : { Title: "", Slug: "", Description: "", IsPublished: false, LanguageCodes: defaultCode ? [defaultCode] : [] }
    );
  }, [open, course, form]);

  const handleValuesChange = (changed: Partial<CourseEdit>) => {
    if ("Slug" in changed) isSlugTouched.current = true;
    if ("Title" in changed && !isEdit && !isSlugTouched.current) {
      form.setFieldsValue({ Slug: slugify(changed.Title ?? "") });
    }
  };

  const handleSave = async () => {
    const values = await form.validateFields();
    const payload: CourseEdit = {
      Title: values.Title.trim(),
      Slug: values.Slug.trim(),
      Description: values.Description?.trim() || null,
      IsPublished: values.IsPublished,
      LanguageCodes: values.LanguageCodes,
    };

    setIsSaving(true);
    setErrors([]);
    try {
      if (course) {
        await CourseService.update(course.Id, payload);
        const languages = payload.LanguageCodes.map((code) => state.languages.find((language) => language.Code === code)).filter(
          (language): language is NonNullable<typeof language> => !!language
        );
        onSaved({ ...course, ...payload, Languages: languages });
      } else {
        onSaved(await CourseService.create(payload));
      }
      notification("success", isEdit ? "Курс збережено." : "Курс створено.");
    } catch (error) {
      setErrors(getErrorMessages(error));
    } finally {
      setIsSaving(false);
    }
  };

  const confirmDelete = () => {
    if (!course) return;
    modal.confirm({
      title: `Видалити курс «${course.Title}»?`,
      content: "Разом з курсом буде видалено всі розділи, теми, матеріали та вправи. Цю дію не можна скасувати.",
      okText: "Видалити",
      okButtonProps: { danger: true },
      cancelText: "Скасувати",
      onOk: async () => {
        try {
          await CourseService.remove(course.Id);
          notification("success", "Курс видалено.");
          onDeleted?.();
        } catch (error) {
          setErrors(getErrorMessages(error));
        }
      },
    });
  };

  return (
    <Modal
      title={isEdit ? "Налаштування курсу" : "Новий курс"}
      open={open}
      onCancel={onClose}
      width={640}
      destroyOnClose
      footer={
        <div className="course-editor__footer">
          {isEdit && onDeleted ? (
            <Button danger icon={<DeleteOutlined />} onClick={confirmDelete}>
              Видалити курс
            </Button>
          ) : (
            <span />
          )}
          <div className="course-editor__footer-actions">
            <Button onClick={onClose}>Скасувати</Button>
            <Button type="primary" loading={isSaving} onClick={handleSave}>
              {isEdit ? "Зберегти" : "Створити"}
            </Button>
          </div>
        </div>
      }
    >
      <Form form={form} layout="vertical" size="large" requiredMark={false} onValuesChange={handleValuesChange}>
        {errors.length > 0 && (
          <Alert
            type="error"
            showIcon
            style={{ marginBottom: 16 }}
            message={errors.length === 1 ? errors[0] : <ul className="form-errors">{errors.map((e) => <li key={e}>{e}</li>)}</ul>}
          />
        )}

        <Form.Item
          name="Title"
          label="Назва курсу"
          rules={[
            { required: true, whitespace: true, message: "Введіть назву" },
            { max: 200, message: "Не довше 200 символів" },
          ]}
        >
          <Input autoFocus placeholder="Наприклад, Основи програмування на C#" />
        </Form.Item>

        <Form.Item
          name="Slug"
          label="Адреса курсу"
          extra={isEdit ? "Адресу не можна змінити після створення." : "Латинські літери, цифри та дефіси. Буде частиною посилання."}
          rules={[
            { required: true, message: "Введіть адресу" },
            { min: 3, max: 100, message: "Від 3 до 100 символів" },
            { pattern: SLUG_PATTERN, message: "Лише a–z, 0–9 та одинарні дефіси" },
          ]}
        >
          <Input addonBefore="/courses/" disabled={isEdit} />
        </Form.Item>

        <Form.Item name="Description" label="Опис" rules={[{ max: 2000, message: "Не довше 2000 символів" }]}>
          <Input.TextArea autoSize={{ minRows: 3, maxRows: 8 }} placeholder="Кому підійде курс і чого він навчить" />
        </Form.Item>

        <Form.Item
          name="LanguageCodes"
          label="Мови програмування"
          extra="Ці мови з'являться в перемикачі курсу. Перша — мова за замовчуванням."
          rules={[{ type: "array", min: 1, message: "Оберіть хоча б одну мову" }]}
        >
          <CourseLanguagePicker />
        </Form.Item>

        <Form.Item
          name="IsPublished"
          label="Опубліковано"
          valuePropName="checked"
          extra="Неопублікований курс бачите лише ви та адміністратор."
        >
          <Switch />
        </Form.Item>
      </Form>
      {contextHolder}
    </Modal>
  );
};

export default CourseEditorModal;
