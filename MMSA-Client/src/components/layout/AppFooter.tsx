import { Link } from "react-router-dom";

const AppFooter = () => (
  <footer className="app-footer">
    <div className="container app-footer__inner">
      <div style={{ display: "flex", flexDirection: "column", gap: 6 }}>
        <span className="app-footer__title">Основи програмування</span>
        <span>Мультимовна навчальна платформа · {new Date().getFullYear()}</span>
      </div>
      <nav aria-label="Посилання у футері" style={{ display: "flex", flexWrap: "wrap", gap: 20, alignItems: "center" }}>
        <Link to="/#courses">Каталог курсів</Link>
        <Link to="/about">Про сайт і конфіденційність</Link>
      </nav>
    </div>
  </footer>
);

export default AppFooter;
