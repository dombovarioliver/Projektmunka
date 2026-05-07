import { NavLink, Link } from "react-router-dom";

export default function Navbar() {
  return (
    <nav className="navbar navbar-expand-lg navbar-dark bg-dark">
      <div className="container">
        <Link className="navbar-brand fw-bold" to="/">
          NeuraFit
        </Link>

        <button
          className="navbar-toggler"
          type="button"
          data-bs-toggle="collapse"
          data-bs-target="#mainNavbar"
          aria-controls="mainNavbar"
          aria-expanded="false"
          aria-label="Menü megnyitása"
        >
          <span className="navbar-toggler-icon"></span>
        </button>

        <div className="collapse navbar-collapse" id="mainNavbar">
          <ul className="navbar-nav ms-auto mb-2 mb-lg-0">
            <li className="nav-item">
              <NavLink className="nav-link" to="/">
                Főoldal
              </NavLink>
            </li>

            <li className="nav-item">
                <NavLink className="nav-link" to="/gyms">
                 Konditermek
                </NavLink>
            </li>

            <li className="nav-item">
              <NavLink className="nav-link" to="/exercises">
                Gyakorlatok
              </NavLink>
            </li>

            <li className="nav-item">
              <NavLink className="nav-link" to="/workout-plan">
                Edzésterv
              </NavLink>
            </li>

            <li className="nav-item">
              <NavLink className="nav-link" to="/chat">
                Chat
              </NavLink>
            </li>

            <li className="nav-item">
              <NavLink className="nav-link" to="/profile">
                Profil
              </NavLink>
            </li>
          </ul>
        </div>
      </div>
    </nav>
  );
}