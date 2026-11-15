import { Switch } from "antd";
import { CodeOutlined, DashboardOutlined, MenuOutlined, TeamOutlined } from "@ant-design/icons";
import { Link, useLocation } from "react-router-dom";
import { useTable } from "../../Store";
import { coursePath } from "../../course/courseUtils";
import { useCanEditCourse } from "../../course/useCourse";
import { useAuth } from "../../auth/AuthContext";
import { Roles } from "../../auth/roles";
import AuthPanel from "../auth/AuthPanel";

type Props = {
  onOpenNavigation?: () => void;
  courseSlug?: string;
  showEditToggle?: boolean;
};

const AppHeader = ({ onOpenNavigation, courseSlug, showEditToggle = false }: Props) => {
  const [state, actions] = useTable();
  const canEditCourse = useCanEditCourse();
  const location = useLocation();
  const dashboardPath = courseSlug ? `${coursePath(courseSlug)}/dashboard` : null;
  const showDashboard = !!dashboardPath && canEditCourse && state.course?.Slug === courseSlug;
  const isDashboardActive = !!dashboardPath && decodeURI(location.pathname) === decodeURI(dashboardPath);
  const { user } = useAuth();
  const isAdmin = !!user?.roles.includes(Roles.Admin);
  const isUsersActive = location.pathname === "/admin/users";

  return (
    <header className="app-header">
      <div className="container app-header__inner">
        {onOpenNavigation && (
          <button type="button" className="icon-button only-mobile" aria-label="Відкрити зміст" onClick={onOpenNavigation}>
            <MenuOutlined />
          </button>
        )}

        <Link to="/" className="app-logo">
          <span className="app-logo__mark" aria-hidden="true">
            <CodeOutlined />
          </span>
          <span className="app-logo__text">Основи програмування</span>
        </Link>

        <nav className="app-header__nav" aria-label="Головна навігація">
          <Link to="/#courses">Курси</Link>
          {courseSlug && <Link to={`${coursePath(courseSlug)}#contents`}>Зміст</Link>}
        </nav>

        <div className="app-header__actions">
          {showEditToggle && (
            <label className="edit-toggle">
              <span className="edit-toggle__label">Режим редагування</span>
              <Switch
                checked={state.isEditMode}
                onChange={(checked) => actions.setEditMode(checked)}
                aria-label="Режим редагування"
              />
            </label>
          )}
          {isAdmin && (
            <Link
              to="/admin/users"
              className={`header-link-button${isUsersActive ? " is-active" : ""}`}
              aria-current={isUsersActive ? "page" : undefined}
              aria-label="Користувачі"
            >
              <TeamOutlined />
              <span className="header-button__label">Користувачі</span>
            </Link>
          )}
          {showDashboard && dashboardPath && (
            <Link
              to={dashboardPath}
              className={`header-link-button${isDashboardActive ? " is-active" : ""}`}
              aria-current={isDashboardActive ? "page" : undefined}
              aria-label="Кабінет викладача"
            >
              <DashboardOutlined />
              <span className="header-button__label">Кабінет викладача</span>
            </Link>
          )}          
          <AuthPanel />
        </div>
      </div>
    </header>
  );
};

export default AppHeader;
