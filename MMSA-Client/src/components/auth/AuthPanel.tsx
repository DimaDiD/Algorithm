import { Button } from "antd";
import { LoginOutlined, LogoutOutlined } from "@ant-design/icons";
import { useState } from "react";
import { useAuth } from "../../auth/AuthContext";
import { useTable } from "../../Store";
import LoginModal from "./LoginModal";
import ProfileModal from "./ProfileModal";
import { initials } from "../../utils/personName";

const AuthPanel = () => {
  const { user, isInitialized, logout } = useAuth();
  const [, actions] = useTable();
  const [isLoginOpen, setIsLoginOpen] = useState(false);
  const [isProfileOpen, setIsProfileOpen] = useState(false);

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
      <button
        type="button"
        className="user-chip user-chip--button"
        onClick={() => setIsProfileOpen(true)}
        title="Мій профіль"
        aria-label={`Мій профіль: ${user.name || user.email}`}
      >
        <span className="user-chip__avatar" aria-hidden="true">{initials(user.name, user.email)}</span>
        <span className="user-chip__email">{user.name || user.email}</span>
        {user.roles.map((role) => (
          <span key={role} className="role-tag">{role}</span>
        ))}
      </button>
      <ProfileModal open={isProfileOpen} onClose={() => setIsProfileOpen(false)} />
      <Button size="large" icon={<LogoutOutlined />} onClick={handleLogout} aria-label="Вийти">
        <span className="logout-label">Вийти</span>
      </Button>
    </>
  );
};

export default AuthPanel;
