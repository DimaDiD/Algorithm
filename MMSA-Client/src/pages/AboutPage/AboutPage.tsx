import { useEffect, useState } from "react";
import { Modal } from "antd";
import { DeleteOutlined } from "@ant-design/icons";
import { useLocation } from "react-router-dom";
import AppHeader from "../../components/layout/AppHeader";
import AppFooter from "../../components/layout/AppFooter";
import ExerciseService from "../../services/exerciseService";
import { resetClientId } from "../../services/clientId";
import notification from "../../notification/notificationMessage";
import { CONTACT_EMAIL } from "../../config";
import { pluralize } from "../../course/courseUtils";

const UPDATED_AT = "1 листопада 2026";

const ANTHROPIC_RETENTION_URL = "https://privacy.claude.com/en/articles/7996866";

const SUMMARY = [
  "Учням не потрібно реєструватися — читати й проходити вправи можна анонімно.",
  "Ми не просимо і не зберігаємо ваше ім'я, email, телефон чи адресу.",
  "Відкриті відповіді та код перевіряє штучний інтелект (Claude). Його оцінку може переглянути викладач.",
  "Немає реклами, аналітики й сторонніх трекерів.",
  "Свої відповіді можна будь-коли видалити кнопкою внизу сторінки.",
];

const AboutPage = () => {
  const location = useLocation();
  const [modal, contextHolder] = Modal.useModal();
  const [isDeleting, setIsDeleting] = useState(false);

  useEffect(() => {
    document.title = "Про сайт і конфіденційність — Основи програмування";
  }, []);

  useEffect(() => {
    if (location.hash) document.getElementById(location.hash.slice(1))?.scrollIntoView();
  }, [location.hash]);

  const deleteMyData = () =>
    modal.confirm({
      title: "Видалити ваші відповіді?",
      content: "Усі відповіді, надіслані з цього браузера, буде видалено з сервера, а ваш прогрес у вправах обнулиться.",
      okText: "Видалити",
      okButtonProps: { danger: true },
      cancelText: "Скасувати",
      onOk: async () => {
        setIsDeleting(true);
        try {
          const deleted = await ExerciseService.deleteMyData();
          resetClientId();
          notification(
            "success",
            deleted > 0 ? `Видалено ${pluralize(deleted, "спробу", "спроби", "спроб")}.` : "На сервері не було ваших відповідей."
          );
        } catch {
          notification("error", "Не вдалося видалити дані. Спробуйте пізніше.");
        } finally {
          setIsDeleting(false);
        }
      },
    });

  return (
    <div className="page">
      <AppHeader />

      <main className="container section about">
        <header className="about__head">
          <span className="eyebrow">Оновлено {UPDATED_AT}</span>
          <h1 className="section__title">Про сайт і конфіденційність</h1>
          <p className="section__lead">
            «Основи програмування» — навчальний сайт, створений у межах магістерської роботи. Викладачі публікують тут
            курси з поясненнями та прикладами коду кількома мовами, а учні читають теми й перевіряють себе у вправах.
          </p>
        </header>

        <section className="card about__summary" aria-labelledby="summary">
          <h2 id="summary" className="about__title">Коротко</h2>
          <ul className="about__list">
            {SUMMARY.map((item) => (
              <li key={item}>{item}</li>
            ))}
          </ul>
        </section>

        <section className="about__section" aria-labelledby="data">
          <h2 id="data" className="about__title">Які дані ми зберігаємо</h2>

          <h3 className="about__subtitle">Учні та всі відвідувачі</h3>
          <ul className="about__list">
            <li>
              <strong>У вашому браузері</strong> — випадковий ідентифікатор (він не містить жодних даних про вас) і обрану мову
              програмування. Ідентифікатор потрібен, щоб показувати ваш найкращий результат у вправах.
            </li>
            <li>
              <strong>На сервері</strong> — ваші відповіді на вправи, отримані бали й час надсилання. Вони пов'язані лише з
              ідентифікатором браузера, а не з вашим ім'ям. Викладач курсу бачить відповіді анонімно, щоб перевірити відкриті
              завдання.
            </li>
            <li>
              <strong>IP-адресу</strong> ми не записуємо в базу даних. Сервер тимчасово тримає її в пам'яті, щоб обмежувати
              надто часті запити, і забуває протягом доби.
            </li>
          </ul>

          <h3 className="about__subtitle">Викладачі</h3>
          <ul className="about__list">
            <li>Email, роль і захищений хеш пароля — сам пароль ми не зберігаємо.</li>
            <li>
              Після входу браузер отримує службовий cookie, який дозволяє не вводити пароль щоразу. Він працює лише на цьому
              сайті й недоступний скриптам.
            </li>
          </ul>

          <h3 className="about__subtitle">Чого ми не робимо</h3>
          <ul className="about__list">
            <li>Не показуємо рекламу і не продаємо та не передаємо дані для маркетингу.</li>
            <li>Не використовуємо аналітику, трекери та сторонні cookie. Шрифти завантажуються з цього ж сайту.</li>
            <li>
              Збираємо лише те, без чого сайт не працюватиме, — відповідно до принципу мінімізації даних Закону України «Про
              захист персональних даних».
            </li>
          </ul>
        </section>

        <section className="about__section" aria-labelledby="ai">
          <h2 id="ai" className="about__title">Перевірка за допомогою штучного інтелекту</h2>
          <p className="about__text">
            Відповіді «своїми словами» та завдання з кодом перевіряє модель <strong>Claude</strong> компанії Anthropic. Біля
            таких відповідей ви побачите позначку «Відгук AI».
          </p>
          <ul className="about__list">
            <li>
              <strong>Що передається:</strong> умова завдання, еталонна відповідь викладача і ваша відповідь.
            </li>
            <li>
              <strong>Що не передається:</strong> ідентифікатор браузера, IP-адреса, email чи будь-які інші дані про вас.
            </li>
            <li>
              За умовами Anthropic для API вхідні та вихідні дані автоматично видаляються протягом 30 днів (крім окремих
              випадків, наприклад порушення правил використання).{" "}
              <a href={ANTHROPIC_RETENTION_URL} target="_blank" rel="noreferrer">
                Докладніше в Anthropic
              </a>
              .
            </li>
            <li>
              Щоб однакові відповіді не надсилати повторно, результат перевірки зберігається в пам'яті нашого сервера до 24
              годин.
            </li>
            <li>
              ШІ може помилятися, тому його оцінка — рекомендаційна. Викладач може переглянути відповідь і змінити бал, а ви
              побачите його коментар біля завдання.
            </li>
            <li>Код не запускається: ШІ аналізує лише його логіку.</li>
          </ul>
        </section>

        <section className="about__section" aria-labelledby="safety">
          <h2 id="safety" className="about__title">Безпека учнів</h2>
          <ul className="about__list">
            <li>
              <strong>Не пишіть у відповідях</strong> своє ім'я, телефон, адресу, назву школи, паролі чи інші особисті дані —
              вони не потрібні для перевірки.
            </li>
            <li>ШІ налаштований відповідати лише про завдання, коротко, дружньо і без посилань.</li>
            <li>Якщо щось на сайті вас засмутило або здалося дивним — розкажіть учителю або батькам.</li>
          </ul>
        </section>

        <section className="card about__delete" aria-labelledby="delete">
          <div>
            <h2 id="delete" className="about__title">Видалити мої відповіді</h2>
            <p className="about__text">
              Видалить з сервера всі відповіді, надіслані з цього браузера, і створить новий ідентифікатор. Якщо ви просто
              очистите дані браузера, відповіді на сервері залишаться, але їх уже не можна буде пов'язати з вами.
            </p>
          </div>
          <button type="button" className="btn btn--danger" onClick={deleteMyData} disabled={isDeleting}>
            <DeleteOutlined /> Видалити мої відповіді
          </button>
        </section>

        <section className="about__section" aria-labelledby="contact">
          <h2 id="contact" className="about__title">Контакти</h2>
          <p className="about__text">
            Питання щодо даних, видалення облікового запису викладача або помилки на сайті —{" "}
            {CONTACT_EMAIL ? <a href={`mailto:${CONTACT_EMAIL}`}>{CONTACT_EMAIL}</a> : "зверніться до свого вчителя інформатики"}
            .
          </p>
        </section>
      </main>

      <AppFooter />
      {contextHolder}
    </div>
  );
};

export default AboutPage;
