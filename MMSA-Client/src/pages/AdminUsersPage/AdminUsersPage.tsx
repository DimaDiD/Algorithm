import { useCallback, useEffect, useState } from "react";
import { Input, Modal, Result, Select, Table, Tag } from "antd";
import type { ColumnsType } from "antd/es/table";
import { Link } from "react-router-dom";
import { useAuth } from "../../auth/AuthContext";
import { Roles } from "../../auth/roles";
import { getErrorMessages } from "../../auth/authApi";
import UserService, { UserListItem, UserPage, UserRole } from "../../services/userService";
import notification from "../../notification/notificationMessage";
import AppHeader from "../../components/layout/AppHeader";
import AppFooter from "../../components/layout/AppFooter";

const ROLE_OPTIONS: { label: string; value: UserRole; hint: string }[] = [
  { label: "Читач", value: "Reader", hint: "читає курси й виконує завдання" },
  { label: "Викладач", value: "Teacher", hint: "створює та веде власні курси" },
  { label: "Адміністратор", value: "Admin", hint: "керує всіма курсами й користувачами" },
];

const roleLabel = (role: UserRole) => ROLE_OPTIONS.find((option) => option.value === role)?.label ?? role;

export const AdminUsersPage = () => {
  const { user, isInitialized } = useAuth();
  const [modal, contextHolder] = Modal.useModal();
  const [search, setSearch] = useState("");
  const [query, setQuery] = useState("");
  const [page, setPage] = useState(1);
  const [data, setData] = useState<UserPage | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [savingId, setSavingId] = useState<string | null>(null);

  const isAdmin = !!user?.roles.includes(Roles.Admin);

  useEffect(() => {
    document.title = "Користувачі — Основи програмування";
  }, []);

  useEffect(() => {
    const timer = window.setTimeout(() => {
      setQuery(search.trim());
      setPage(1);
    }, 300);
    return () => window.clearTimeout(timer);
  }, [search]);

  const load = useCallback(async () => {
    if (!isAdmin) return;
    setIsLoading(true);
    try {
      setData(await UserService.search(query, page));
    } catch (error) {
      notification("error", getErrorMessages(error)[0]);
    } finally {
      setIsLoading(false);
    }
  }, [isAdmin, query, page]);

  useEffect(() => {
    load();
  }, [load]);

  const applyRole = async (target: UserListItem, role: UserRole) => {
    setSavingId(target.Id);
    try {
      await UserService.changeRole(target.Email, role);
      notification("success", `${target.Email}: роль змінено на «${roleLabel(role)}».`);
      await load();
    } catch (error) {
      notification("error", getErrorMessages(error)[0]);
    } finally {
      setSavingId(null);
    }
  };

  const requestRoleChange = (target: UserListItem, role: UserRole) => {
    if (role === target.Role) return;
    const isSensitive = role === "Admin" || target.Role === "Admin";
    const losesCourses = target.Role !== "Reader" && role === "Reader" && target.CourseCount > 0;

    if (!isSensitive && !losesCourses) {
      applyRole(target, role);
      return;
    }

    modal.confirm({
      title: `Змінити роль ${target.Email} на «${roleLabel(role)}»?`,
      content: [
        role === "Admin" ? "Адміністратор отримає доступ до всіх курсів і керування користувачами." : null,
        losesCourses
          ? `Користувач веде ${target.CourseCount} курс(и) — як читач він більше не зможе їх редагувати. Самі курси залишаться.`
          : null,
        "Користувачеві потрібно буде увійти знову.",
      ]
        .filter(Boolean)
        .join(" "),
      okText: "Змінити",
      cancelText: "Скасувати",
      onOk: () => applyRole(target, role),
    });
  };

  const columns: ColumnsType<UserListItem> = [
    {
      title: "Email",
      dataIndex: "Email",
      render: (email: string, item) => (
        <span className="users-table__email">
          {email}
          {item.IsCurrentUser && <Tag>це ви</Tag>}
        </span>
      ),
    },
    { title: "Курсів", dataIndex: "CourseCount", align: "right", width: 100 },
    {
      title: "Роль",
      dataIndex: "Role",
      width: 260,
      render: (role: UserRole, item) => (
        <Select
          value={role}
          style={{ width: "100%" }}
          disabled={item.IsCurrentUser}
          loading={savingId === item.Id}
          onChange={(value: UserRole) => requestRoleChange(item, value)}
          aria-label={`Роль користувача ${item.Email}`}
          options={ROLE_OPTIONS.map((option) => ({
            value: option.value,
            label: (
              <span title={option.hint}>
                {option.label}
              </span>
            ),
          }))}
        />
      ),
    },
  ];

  if (isInitialized && !isAdmin) {
    return (
      <div className="page">
        <AppHeader />
        <main className="container section">
          <div className="card topic-card">
            <Result
              status="403"
              title="Сторінка доступна лише адміністраторам"
              extra={<Link to="/" className="btn btn--primary">До каталогу</Link>}
            />
          </div>
        </main>
        <AppFooter />
      </div>
    );
  }

  return (
    <div className="page">
      <AppHeader />
      <main className="container section dashboard">
        <div>
          <h1 className="section__title">Користувачі</h1>
          <p className="section__lead">
            Нові користувачі реєструються як читачі. Щоб людина могла створювати курси, призначте їй роль «Викладач».
          </p>
        </div>

        <section className="card dashboard__panel">
          <Input.Search
            placeholder="Пошук за email"
            allowClear
            value={search}
            onChange={(event) => setSearch(event.target.value)}
            style={{ maxWidth: 360 }}
          />
          <Table
            rowKey="Id"
            columns={columns}
            dataSource={data?.Items ?? []}
            loading={isLoading}
            size="middle"
            scroll={{ x: 560 }}
            pagination={{
              current: data?.Page ?? page,
              pageSize: data?.PageSize ?? 20,
              total: data?.Total ?? 0,
              showSizeChanger: false,
              onChange: setPage,
            }}
          />
        </section>
      </main>
      <AppFooter />
      {contextHolder}
    </div>
  );
};

export default AdminUsersPage;
