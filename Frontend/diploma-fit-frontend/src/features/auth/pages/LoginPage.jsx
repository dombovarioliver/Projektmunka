import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { useAuth } from "../context/useAuth";
import "../styles/authPages.css";

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
        error?.response?.data ||
          "Sikertelen bejelentkezés. Ellenőrizd az email címet és a jelszót."
      );
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <main className="auth-page">
      <div className="auth-bg-orb auth-bg-orb-one" />
      <div className="auth-bg-orb auth-bg-orb-two" />
      <div className="auth-bg-grid" />

      <section className="auth-shell">
        <div className="auth-info-panel">
          <div className="auth-badge">
            <span />
            AI Fitness Platform
          </div>

          <h1>Lépj be, és kezeld az étrendedet, edzésedet és fejlődésedet.</h1>

          <p>
            A NeuraFit segít átláthatóan követni a napi kalóriákat, makrókat,
            edzésterveket és személyes célokat.
          </p>

          <div className="auth-info-cards">
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

        <div className="auth-card">
          <div className="auth-logo-box">
            <div className="auth-logo-mark">N</div>
            <div className="auth-logo-text">
              <strong>NeuraFit</strong>
              <span>AI Fitness</span>
            </div>
          </div>

          <div className="auth-card-header">
            <h2>Üdv újra!</h2>
            <p>Jelentkezz be a folytatáshoz</p>
          </div>

          {errorMessage && (
            <div className="auth-error-message">{errorMessage}</div>
          )}

          <form onSubmit={handleSubmit} className="auth-form">
            <div className="auth-form-group">
              <label>Email cím</label>

              <input
                type="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                required
              />
            </div>

            <div className="auth-form-group">
              <label>Jelszó</label>

              <input
                type="password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                required
              />
            </div>

            <button
              type="submit"
              className="auth-submit-btn"
              disabled={isSubmitting}
            >
              {isSubmitting ? "Bejelentkezés..." : "Bejelentkezés"}
            </button>
          </form>

          <div className="auth-bottom-text">
            Még nincs fiókod? <Link to="/register">Regisztráció</Link>
          </div>
        </div>
      </section>
    </main>
  );
}
