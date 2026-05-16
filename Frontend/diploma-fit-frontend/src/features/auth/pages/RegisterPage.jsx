import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { useAuth } from "../context/useAuth";
import "../styles/authPages.css";

const DEFAULT_FORM = {
  name: "",
  email: "",
  password: "",
  confirmPassword: "",
  gender: 0,
  age: 20,
};

export default function RegisterPage() {
  const navigate = useNavigate();
  const { register } = useAuth();

  const [formData, setFormData] = useState(DEFAULT_FORM);
  const [errorMessage, setErrorMessage] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);

  function handleChange(event) {
    const { name, value } = event.target;

    setFormData((prev) => ({
      ...prev,
      [name]: value,
    }));
  }

  function buildPayload() {
    return {
      name: formData.name.trim(),
      email: formData.email.trim(),
      password: formData.password,
      confirmPassword: formData.confirmPassword,
      gender: Number(formData.gender),
      age: Number(formData.age),
    };
  }

  async function handleSubmit(event) {
    event.preventDefault();

    setErrorMessage("");

    if (formData.password !== formData.confirmPassword) {
      setErrorMessage("A két jelszó nem egyezik.");
      return;
    }

    try {
      setIsSubmitting(true);
      await register(buildPayload());
      navigate("/");
    } catch (error) {
      console.error(error);

      if (error?.response?.status === 409) {
        setErrorMessage("Ezzel az email címmel már létezik felhasználó.");
        return;
      }

      setErrorMessage(
        error?.response?.data ||
          "Sikertelen regisztráció. Ellenőrizd a megadott adatokat."
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
            Új NeuraFit profil
          </div>

          <h1>Kezdjük az alapadataiddal, hogy személyre szabott tervet kapj.</h1>

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
            <h2>Regisztráció</h2>
            <p>Hozd létre az új fiókodat</p>
          </div>

          {errorMessage && (
            <div className="auth-error-message">{errorMessage}</div>
          )}

          <form onSubmit={handleSubmit} className="auth-form">
            <div className="auth-form-group">
              <label>Név</label>

              <input
                type="text"
                name="name"
                value={formData.name}
                onChange={handleChange}
                placeholder="Teljes név"
                required
              />
            </div>

            <div className="auth-form-group">
              <label>Email cím</label>

              <input
                type="email"
                name="email"
                value={formData.email}
                onChange={handleChange}
                placeholder="pelda@email.com"
                required
              />
            </div>

            <div className="auth-form-grid two-cols">
              <div className="auth-form-group">
                <label>Jelszó</label>

                <input
                  type="password"
                  name="password"
                  value={formData.password}
                  onChange={handleChange}
                  placeholder="Minimum 6 karakter"
                  required
                />
              </div>

              <div className="auth-form-group">
                <label>Jelszó újra</label>

                <input
                  type="password"
                  name="confirmPassword"
                  value={formData.confirmPassword}
                  onChange={handleChange}
                  placeholder="Jelszó ismétlése"
                  required
                />
              </div>

              <div className="auth-form-group">
                <label>Nem</label>

                <select
                  name="gender"
                  value={formData.gender}
                  onChange={handleChange}
                >
                  <option value={0}>Férfi</option>
                  <option value={1}>Nő</option>
                </select>
              </div>

              <div className="auth-form-group">
                <label>Életkor</label>

                <input
                  type="number"
                  name="age"
                  value={formData.age}
                  onChange={handleChange}
                  min="1"
                  max="100"
                  required
                />
              </div>
            </div>

            <button
              type="submit"
              className="auth-submit-btn"
              disabled={isSubmitting}
            >
              {isSubmitting ? "Regisztráció..." : "Regisztráció"}
            </button>
          </form>

          <div className="auth-bottom-text">
            Már van fiókod? <Link to="/login">Bejelentkezés</Link>
          </div>
        </div>
      </section>
    </main>
  );
}
