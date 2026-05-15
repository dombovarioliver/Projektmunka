import { NavLink, Link } from "react-router-dom";
import { useEffect, useState } from "react";

import { useAuth } from "../../features/auth/context/useAuth";

export default function Navbar() {
  const { logout } = useAuth();

  const [userData, setUserData] = useState({
    name: localStorage.getItem("name") || "Felhasználó",
    profilePictureUrl: localStorage.getItem("profilePictureUrl") || "",
  });

  useEffect(() => {
    function loadUserData() {
      setUserData({
        name: localStorage.getItem("name") || "Felhasználó",
        profilePictureUrl:
          localStorage.getItem("profilePictureUrl") || "",
      });
    }

    loadUserData();

    window.addEventListener("storage", loadUserData);

    return () => {
      window.removeEventListener("storage", loadUserData);
    };
  }, []);

  function getProfileImageUrl(url) {
    if (!url || url.trim() === "") {
      return (
        "https://ui-avatars.com/api/?name=" +
        encodeURIComponent(userData.name || "User")
      );
    }

    if (url.startsWith("http")) {
      return url;
    }

    const apiOrigin =
      import.meta.env.VITE_API_ORIGIN || "http://localhost:8080";

    return `${apiOrigin}${url.startsWith("/") ? "" : "/"}${url}`;
  }

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

            <li className="nav-item d-flex align-items-center ms-lg-3 gap-2">
              <img
                src={getProfileImageUrl(userData.profilePictureUrl)}
                alt="Profilkép"
                style={{
                  width: "38px",
                  height: "38px",
                  borderRadius: "50%",
                  objectFit: "cover",
                  border: "2px solid rgba(255,255,255,0.3)",
                }}
                />
                

              <span className="navbar-text fw-semibold text-white">
                {userData.name}
              </span>
             
              <button
                className="btn btn-outline-light btn-sm ms-2"
                onClick={logout}
              >
                Kilépés
              </button>
            </li>
          </ul>
        </div>
      </div>
    </nav>
  );
}