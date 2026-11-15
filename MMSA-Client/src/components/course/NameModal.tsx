import { Form, Input, Modal } from "antd";
import { useEffect, useState } from "react";

type Props = {
  open: boolean;
  title: string;
  label: string;
  okText: string;
  initialValue?: string;
  onCancel: () => void;
  onSubmit: (name: string) => Promise<void>;
};

type FormValues = {
  name: string;
};

const NameModal = ({ open, title, label, okText, initialValue, onCancel, onSubmit }: Props) => {
  const [form] = Form.useForm<FormValues>();
  const [isSaving, setIsSaving] = useState(false);

  useEffect(() => {
    if (open) {
      form.setFieldsValue({ name: initialValue ?? "" });
    }
  }, [open, initialValue, form]);

  const handleOk = async () => {
    const { name } = await form.validateFields();
    setIsSaving(true);
    try {
      await onSubmit(name.trim());
    } catch {
    } finally {
      setIsSaving(false);
    }
  };

  return (
    <Modal
      title={title}
      open={open}
      okText={okText}
      cancelText="Скасувати"
      confirmLoading={isSaving}
      onOk={handleOk}
      onCancel={onCancel}
      destroyOnClose
    >
      <Form form={form} layout="vertical" size="large" requiredMark={false} onFinish={handleOk}>
        <Form.Item
          name="name"
          label={label}
          rules={[
            { required: true, whitespace: true, message: "Введіть назву" },
            { max: 100, message: "Не довше 100 символів" },
          ]}
        >
          <Input autoFocus />
        </Form.Item>
      </Form>
    </Modal>
  );
};

export default NameModal;
