import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../context/useAuth";
import "../styles/loginPage.css";

export default function LoginPage() {
  const navigate = useNavigate();
  const { login } = useAuth();

  const [email, setEmail] = useState("dombovarioliver03@gmail.com");
  const [password, setPassword] = useState("jelszo123");
  const [errorMessage, setErrorMessage] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);

  async function handleSubmit(event) {
    event.preventDefault();

    setErrorMessage("");
    setIsSubmitting(true);

    try {
      await login(email, password);
      navigate("/");
    } catch (error) {
      console.error(error);

      setErrorMessage(
        "Sikertelen bejelentkezés. Ellenőrizd az email címet és a jelszót."
      );
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <main className="login-page">
      <div className="login-bg-orb login-bg-orb-one" />
      <div className="login-bg-orb login-bg-orb-two" />
      <div className="login-bg-grid" />

      <section className="login-shell">
        <div className="login-info-panel">
          <div className="login-badge">
            <span />
            AI Fitness Platform
          </div>

          <h1>
            Lépj be, és kezeld az étrendedet, edzésedet és fejlődésedet egy
            helyen.
          </h1>

          <p>
            A NeuraFit segít átláthatóan követni a napi kalóriákat, makrókat,
            edzésterveket és személyes célokat.
          </p>

          <div className="login-info-cards">
            <div>
              <strong>AI</strong>
              <span>étrendtervezés</span>
            </div>

            <div>
              <strong>24/7</strong>
              <span>követés</span>
            </div>

            <div>
              <strong>100%</strong>
              <span>személyre szabva</span>
            </div>
          </div>
        </div>

        <div className="login-card">
          <div className="login-logo-box">
            <img
              src="/src/assets/logo.png"
              alt="NeuraFit logó"
              className="login-logo-img"
            />
          </div>

          <div className="login-card-header">
            <h2>Üdv újra!</h2>
            <p>Jelentkezz be a folytatáshoz</p>
          </div>

          {errorMessage && (
            <div className="login-error-message">{errorMessage}</div>
          )}

          <form onSubmit={handleSubmit} className="login-form">
            <div className="login-form-group">
              <label>Email cím</label>

              <input
                type="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                placeholder="pelda@email.com"
                required
              />
            </div>

            <div className="login-form-group">
              <label>Jelszó</label>

              <input
                type="password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                placeholder="Add meg a jelszavad"
                required
              />
            </div>

            <button
              type="submit"
              className="login-submit-btn"
              disabled={isSubmitting}
            >
              {isSubmitting ? "Bejelentkezés..." : "Bejelentkezés"}
            </button>
          </form>

          <div className="login-bottom-text">
            NeuraFit · okosabb fitnesz irányítás
          </div>
        </div>
      </section>
    </main>
  );
}