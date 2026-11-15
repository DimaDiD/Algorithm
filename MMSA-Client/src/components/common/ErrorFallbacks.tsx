import { ReactNode } from "react";
import { ReloadOutlined, WarningOutlined } from "@ant-design/icons";
import { useLocation } from "react-router-dom";
import ErrorBoundary, { ErrorFallbackProps, isChunkLoadError } from "./ErrorBoundary";

const reloadPage = () => window.location.reload();

const PageErrorFallback = ({ error }: ErrorFallbackProps) => {
  const isOutdated = isChunkLoadError(error);
  return (
    <main className="page-error" role="alert">
      <div className="card page-error__card">
        <span className="page-error__icon" aria-hidden="true">
          <WarningOutlined />
        </span>
        <h1 className="page-error__title">{isOutdated ? "Сайт оновився" : "Щось пішло не так"}</h1>
        <p className="page-error__text">
          {isOutdated
            ? "Ми щойно випустили нову версію. Оновіть сторінку, щоб продовжити."
            : "Сторінку не вдалося показати. Спробуйте оновити її — якщо помилка повториться, повідомте вчителя."}
        </p>
        <div className="page-error__actions">
          <button type="button" className="btn btn--primary" onClick={reloadPage}>
            <ReloadOutlined /> Оновити сторінку
          </button>
          <a href="/" className="btn btn--ghost">
            На головну
          </a>
        </div>
      </div>
    </main>
  );
};

const BlockErrorFallback = ({ error, reset }: ErrorFallbackProps) => {
  const isOutdated = isChunkLoadError(error);
  return (
    <div className="block-error" role="alert">
      <WarningOutlined className="block-error__icon" aria-hidden="true" />
      <span className="block-error__text">
        {isOutdated ? "Сайт оновився — оновіть сторінку, щоб побачити цей блок." : "Не вдалося показати цей блок."}
      </span>
      <button type="button" className="block-error__button" onClick={isOutdated ? reloadPage : reset}>
        {isOutdated ? "Оновити сторінку" : "Спробувати ще раз"}
      </button>
    </div>
  );
};

export const RouteErrorBoundary = ({ children }: { children: ReactNode }) => {
  const location = useLocation();
  return (
    <ErrorBoundary resetKey={location.pathname} fallback={(props) => <PageErrorFallback {...props} />}>
      {children}
    </ErrorBoundary>
  );
};

export const BlockErrorBoundary = ({ children, resetKey }: { children: ReactNode; resetKey?: unknown }) => (
  <ErrorBoundary resetKey={resetKey} fallback={(props) => <BlockErrorFallback {...props} />}>
    {children}
  </ErrorBoundary>
);
