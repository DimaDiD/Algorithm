import { useEffect, useState } from "react";
import { Alert, Button, Form, Input, Modal, Skeleton } from "antd";
import { refreshAccessToken, getErrorMessages } from "../../auth/authApi";
import ProfileService, { Profile } from "../../services/profileService";
import notification from "../../notification/notificationMessage";
import { PERSON_NAME_MAX_LENGTH } from "../../utils/personName";

type FormValues = {
  lastName: string;
  firstName: string;
  middleName?: string;
};

type Props = {
  open: boolean;
  onClose: () => void;
};

const nameRules = (required: boolean, message: string) => [
  ...(required ? [{ required: true, whitespace: true, message }] : []),
  { max: PERSON_NAME_MAX_LENGTH, message: `Не довше ${PERSON_NAME_MAX_LENGTH} символів` },
];

const ProfileModal = ({ open, onClose }: Props) => {
  const [form] = Form.useForm<FormValues>();
  const [profile, setProfile] = useState<Profile | null>(null);
  const [errors, setErrors] = useState<string[]>([]);
  const [isSaving, setIsSaving] = useState(false);

  useEffect(() => {
    if (!open) return;
    let isActive = true;
    setProfile(null);
    setErrors([]);
    ProfileService.get()
      .then((loaded) => {
        if (!isActive) return;
        setProfile(loaded);
        form.setFieldsValue({
          lastName: loaded.LastName ?? "",
          firstName: loaded.FirstName ?? "",
          middleName: loaded.MiddleName ?? "",
        });
      })
      .catch((error) => isActive && setErrors(getErrorMessages(error)));
    return () => {
      isActive = false;
    };
  }, [open, form]);

  const save = async (values: FormValues) => {
    setIsSaving(true);
    setErrors([]);
    try {
      await ProfileService.update({
        LastName: values.lastName,
        FirstName: values.firstName,
        MiddleName: values.middleName || null,
      });
      await refreshAccessToken();
      notification("success", "Профіль збережено.");
      onClose();
    } catch (error) {
      setErrors(getErrorMessages(error));
    } finally {
      setIsSaving(false);
    }
  };

  return (
    <Modal title="Мій профіль" open={open} onCancel={onClose} footer={null} width={460} destroyOnClose>
      {errors.length > 0 && (
        <Alert
          type="error"
          showIcon
          style={{ marginBottom: 16 }}
          message={errors.map((error) => <div key={error}>{error}</div>)}
        />
      )}

      {!profile && errors.length === 0 ? (
        <Skeleton active paragraph={{ rows: 4 }} />
      ) : (
        <Form form={form} layout="vertical" size="large" onFinish={save} requiredMark={false}>
          <Form.Item label="Email" extra="Email змінити не можна: за ним ви входите на сайт.">
            <Input value={profile?.Email} disabled />
          </Form.Item>
          <Form.Item name="lastName" label="Прізвище" rules={nameRules(true, "Введіть прізвище")}>
            <Input autoComplete="family-name" />
          </Form.Item>
          <Form.Item name="firstName" label="Ім'я" rules={nameRules(true, "Введіть ім'я")}>
            <Input autoComplete="given-name" />
          </Form.Item>
          <Form.Item name="middleName" label="По батькові" extra="Необов'язково" rules={nameRules(false, "")}>
            <Input autoComplete="additional-name" />
          </Form.Item>
          <Button type="primary" htmlType="submit" loading={isSaving} block style={{ height: 52, fontWeight: 800 }}>
            Зберегти
          </Button>
        </Form>
      )}
    </Modal>
  );
};

export default ProfileModal;
