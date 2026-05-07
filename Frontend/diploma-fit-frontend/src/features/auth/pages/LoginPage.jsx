import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../context/useAuth";

export default function LoginPage() {
  const navigate = useNavigate();
  const { login } = useAuth();

  const [email, setEmail] = useState("anna@example.com");
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
    <main className="min-vh-100 d-flex align-items-center justify-content-center bg-light">
      <div className="card shadow-sm border-0" style={{ width: "420px" }}>
        <div className="card-body p-4">
          <h1 className="h3 fw-bold mb-2 text-center">NeuraFit</h1>

          <p className="text-muted text-center mb-4">
            Jelentkezz be a folytatáshoz
          </p>

          {errorMessage && (
            <div className="alert alert-danger">{errorMessage}</div>
          )}

          <form onSubmit={handleSubmit}>
            <div className="mb-3">
              <label className="form-label">Email cím</label>

              <input
                type="email"
                className="form-control"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                required
              />
            </div>

            <div className="mb-3">
              <label className="form-label">Jelszó</label>

              <input
                type="password"
                className="form-control"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                required
              />
            </div>

            <button
              type="submit"
              className="btn btn-dark w-100"
              disabled={isSubmitting}
            >
              {isSubmitting ? "Bejelentkezés..." : "Bejelentkezés"}
            </button>
          </form>
        </div>
      </div>
    </main>
  );
}