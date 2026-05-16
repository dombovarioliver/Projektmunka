import { Link } from "react-router-dom";
import "../styles/home.css";

export default function HomePage() {
  const stats = [
    { value: "AI", label: "étrend és edzésterv" },
    { value: "24/7", label: "haladáskövetés" },
    { value: "100%", label: "személyre szabható" },
  ];

  const features = [
    {
      title: "Okos étrendtervezés",
      text: "Generált étrend napi bontásban, kalória- és makrókövetéssel.",
      icon: "🍽️",
    },
    {
      title: "Edzésterv cél alapján",
      text: "Otthoni vagy edzőtermi gyakorlatok izomcsoportokra bontva.",
      icon: "🏋️",
    },
    {
      title: "Profil és fejlődés",
      text: "Saját adatok, profilkép és személyes célok egy helyen.",
      icon: "📈",
    },
    {
      title: "Edzőtermek térképen",
      text: "Budapesti konditermek keresése és megjelenítése térképes nézetben.",
      icon: "📍",
    },
  ];

  return (
    <main className="home-page">
      <section className="home-hero">
        <div className="home-hero-bg home-hero-bg-one" />
        <div className="home-hero-bg home-hero-bg-two" />

        <div className="home-hero-content">
          <div className="home-eyebrow">
            <span />
            NeuraFit dashboard
          </div>

          <h1>
            Építs jobb formát egy okosabb, személyre szabott rendszerrel.
          </h1>

          <p>
            Kövesd az étrendedet, kezeld az edzéseidet, nézd a napi céljaidat,
            és tartsd kézben a fejlődésedet egy modern fitnesz felületen.
          </p>

          <div className="home-actions">
            <Link to="/diet-plan" className="home-primary-btn">Étrend megnyitása</Link>
            <Link to="/workout-plan" className="home-secondary-btn">Edzésterv megnyitása</Link>
          </div>

          <div className="home-stats">
            {stats.map((item) => (
              <div className="home-stat-card" key={item.label}>
                <strong>{item.value}</strong>
                <span>{item.label}</span>
              </div>
            ))}
          </div>
        </div>

        <div className="home-hero-panel">
          <div className="home-panel-top">
            <div>
              <span>Mai cél</span>
              <strong>2 450 kcal</strong>
            </div>
            <div className="home-panel-badge">Aktív</div>
          </div>

          <div className="home-ring-wrap">
            <div className="home-kcal-ring">
              <div>
                <strong>68%</strong>
                <span>kész</span>
              </div>
            </div>
          </div>

          <div className="home-macro-list">
            <div>
              <span>Fehérje</span>
              <strong>142 g</strong>
            </div>
            <div>
              <span>Szénhidrát</span>
              <strong>218 g</strong>
            </div>
            <div>
              <span>Zsír</span>
              <strong>71 g</strong>
            </div>
          </div>
        </div>
      </section>

      <section className="home-section-head">
        <span>Funkciók</span>
        <h2>Minden fontos rész egy letisztult kezdőlapon</h2>
      </section>

      <section className="home-feature-grid">
        {features.map((feature) => (
          <article className="home-feature-card" key={feature.title}>
            <div className="home-feature-icon">{feature.icon}</div>
            <h3>{feature.title}</h3>
            <p>{feature.text}</p>
          </article>
        ))}
      </section>

      <section className="home-bottom-grid">
        <article className="home-wide-card">
          <div>
            <span className="home-card-label">Mai fókusz</span>
            <h2>Legyen egyszerű követni, hogy mit ettél és mit edzettél.</h2>
            <p>
              A főoldal gyors áttekintést ad a napi állapotodról, innen pedig
              egy kattintással eléred az étrendet, edzéstervet és profilodat.
            </p>
          </div>
          <Link to="/profile" className="home-small-link">
            Profil megnyitása
          </Link>
        </article>

        <article className="home-mini-card">
          <span>Heti aktivitás</span>
          <strong>5 / 7 nap</strong>
          <div className="home-mini-progress">
            <span />
          </div>
        </article>
      </section>
    </main>
  );
}
