import { Button } from "antd";
import { LoginOutlined, LogoutOutlined } from "@ant-design/icons";
import { useState } from "react";
import { useAuth } from "../../auth/AuthContext";
import { useTable } from "../../Store";
import LoginModal from "./LoginModal";

const AuthPanel = () => {
  const { user, isInitialized, logout } = useAuth();
  const [, actions] = useTable();
  const [isLoginOpen, setIsLoginOpen] = useState(false);

  if (!isInitialized) return null;

  if (!user) {
    return (
      <>
        <Button size="large" icon={<LoginOutlined />} onClick={() => setIsLoginOpen(true)}>
          Увійти
        </Button>
        <LoginModal open={isLoginOpen} onClose={() => setIsLoginOpen(false)} />
      </>
    );
  }

  const handleLogout = async () => {
    actions.setEditMode(false);
    await logout();
  };

  return (
    <>
      <div className="user-chip">
        <span className="user-chip__avatar" aria-hidden="true">{user.email.charAt(0)}</span>
        <span className="user-chip__email">{user.email}</span>
        {user.roles.map((role) => (
          <span key={role} className="role-tag">{role}</span>
        ))}
      </div>
      <Button size="large" icon={<LogoutOutlined />} onClick={handleLogout} aria-label="Вийти">
        <span className="logout-label">Вийти</span>
      </Button>
    </>
  );
};

export default AuthPanel;
