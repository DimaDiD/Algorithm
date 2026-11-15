import { Alert, Button, Form, Input, Modal, Segmented } from "antd";
import { useState } from "react";
import { useAuth } from "../../auth/AuthContext";
import { getErrorMessages } from "../../auth/authApi";
import notification from "../../notification/notificationMessage";

type Mode = "login" | "register";

type FormValues = {
  email: string;
  password: string;
};

type Props = {
  open: boolean;
  onClose: () => void;
};

const LoginModal = ({ open, onClose }: Props) => {
  const { login, register } = useAuth();
  const [form] = Form.useForm<FormValues>();
  const [mode, setMode] = useState<Mode>("login");
  const [errors, setErrors] = useState<string[]>([]);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const close = () => {
    form.resetFields();
    setErrors([]);
    setMode("login");
    onClose();
  };

  const handleSubmit = async (values: FormValues) => {
    setIsSubmitting(true);
    setErrors([]);
    try {
      if (mode === "login") {
        await login(values.email, values.password);
        notification("success", "Ви увійшли в систему.");
      } else {
        await register(values.email, values.password);
        notification("success", "Акаунт створено. Права на редагування надає адміністратор.");
      }
      close();
    } catch (error) {
      setErrors(getErrorMessages(error));
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <Modal title="Вхід для викладачів" open={open} onCancel={close} footer={null} width={460} destroyOnClose>
      <p className="modal-lead">Читати курс можна без входу. Акаунт потрібен, щоб редагувати матеріали.</p>

      <Segmented
        block
        size="large"
        value={mode}
        onChange={(value) => {
          setMode(value as Mode);
          setErrors([]);
        }}
        options={[
          { label: "Увійти", value: "login" },
          { label: "Реєстрація", value: "register" },
        ]}
        style={{ marginBottom: 20 }}
      />

      {errors.length > 0 && (
        <Alert
          type="error"
          showIcon
          style={{ marginBottom: 16 }}
          message={errors.map((error) => <div key={error}>{error}</div>)}
        />
      )}

      <Form form={form} layout="vertical" size="large" onFinish={handleSubmit} requiredMark={false}>
        <Form.Item
          name="email"
          label="Email"
          rules={[
            { required: true, message: "Введіть email" },
            { type: "email", message: "Некоректний email" },
          ]}
        >
          <Input autoComplete="email" />
        </Form.Item>

        <Form.Item
          name="password"
          label="Пароль"
          rules={[{ required: true, message: "Введіть пароль" }]}
          extra={mode === "register" ? "Щонайменше 8 символів, цифра, мала й велика літера." : undefined}
        >
          <Input.Password autoComplete={mode === "login" ? "current-password" : "new-password"} />
        </Form.Item>

        <Button type="primary" htmlType="submit" loading={isSubmitting} block style={{ height: 52, fontWeight: 800 }}>
          {mode === "login" ? "Увійти" : "Зареєструватися"}
        </Button>
      </Form>

      <p className="modal-note">Після реєстрації права на редагування надає адміністратор.</p>
    </Modal>
  );
};

export default LoginModal;
