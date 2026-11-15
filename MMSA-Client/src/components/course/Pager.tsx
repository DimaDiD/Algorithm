import { Link } from "react-router-dom";
import { Topic } from "../../course/courseUtils";

type Props = {
  previous?: Topic;
  next?: Topic;
};

const Pager = ({ previous, next }: Props) => {
  if (!previous && !next) return null;

  return (
    <nav className="pager" aria-label="Перехід між темами">
      {previous && (
        <Link to={previous.path} className="pager__link">
          <span className="pager__hint">← Попередня</span>
          <span className="pager__title">{previous.title}</span>
        </Link>
      )}
      {next && (
        <Link to={next.path} className="pager__link pager__link--next">
          <span className="pager__hint">Наступна →</span>
          <span className="pager__title">{next.title}</span>
        </Link>
      )}
    </nav>
  );
};

export default Pager;
