import { NavLink, Link } from "react-router-dom";
import { useEffect, useState } from "react";

import { useAuth } from "../../features/auth/context/useAuth";
import "./Navbar.css";

export default function Navbar() {
  const { logout } = useAuth();
  const [isMenuOpen, setIsMenuOpen] = useState(false);
  const [userData, setUserData] = useState({
    name: localStorage.getItem("name") || "Felhasználó",
    profilePictureUrl: localStorage.getItem("profilePictureUrl") || "",
  });

  useEffect(() => {
    function loadUserData() {
      setUserData({
        name: localStorage.getItem("name") || "Felhasználó",
        profilePictureUrl: localStorage.getItem("profilePictureUrl") || "",
      });
    }

    loadUserData();

    window.addEventListener("storage", loadUserData);
    window.addEventListener("profileUpdated", loadUserData);

    return () => {
      window.removeEventListener("storage", loadUserData);
      window.removeEventListener("profileUpdated", loadUserData);
    };
  }, []);

  function getProfileImageUrl(url) {
    if (!url || url.trim() === "") {
      return (
        "https://ui-avatars.com/api/?background=2563eb&color=ffffff&bold=true&name=" +
        encodeURIComponent(userData.name || "User")
      );
    }

    if (url.startsWith("http")) {
      return url;
    }

    const apiOrigin = import.meta.env.VITE_API_ORIGIN || "http://localhost:8080";

    return `${apiOrigin}${url.startsWith("/") ? "" : "/"}${url}`;
  }

  function closeMenu() {
    setIsMenuOpen(false);
  }

  function handleLogout() {
    closeMenu();
    logout();
  }

  const navItems = [
    { to: "/", label: "Főoldal" },
    { to: "/gyms", label: "Konditermek" },
    { to: "/exercises", label: "Gyakorlatok" },
    { to: "/workout-plan", label: "Edzésterv" },
    { to: "/diet-plan", label: "Étrend" },
    { to: "/chat", label: "Chat" },
  ];

  return (
    <header className="neura-navbar-shell">
      <nav className="neura-navbar">
        <Link className="neura-brand" to="/" onClick={closeMenu}>
          <span className="neura-brand-logo-wrap">
            <img src="/src/assets/logo_N.png" alt="NeuraFit logo" className="neura-brand-logo" />
          </span>

          <span className="neura-brand-text">
            <strong>NeuraFit</strong>
            <small>AI Fitness</small>
          </span>
        </Link>

        <button
          type="button"
          className={isMenuOpen ? "neura-menu-button active" : "neura-menu-button"}
          onClick={() => setIsMenuOpen((prev) => !prev)}
          aria-label="Menü megnyitása"
          aria-expanded={isMenuOpen}
        >
          <span />
          <span />
          <span />
        </button>

        <div className={isMenuOpen ? "neura-navbar-content open" : "neura-navbar-content"}>
          <div className="neura-nav-links">
            {navItems.map((item) => (
              <NavLink
                key={item.to}
                to={item.to}
                end={item.to === "/"}
                onClick={closeMenu}
                className={({ isActive }) =>
                  isActive ? "neura-nav-link active" : "neura-nav-link"
                }
              >
                {item.label}
              </NavLink>
            ))}
          </div>

          <div className="neura-user-area">
            <NavLink
              to="/profile"
              onClick={closeMenu}
              className={({ isActive }) =>
                isActive ? "neura-profile-chip active" : "neura-profile-chip"
              }
            >
              <img
                src={getProfileImageUrl(userData.profilePictureUrl)}
                alt="Profilkép"
              />

              <span>
                <strong>{userData.name}</strong>
                <small>Profil</small>
              </span>
            </NavLink>

            <button type="button" className="neura-logout-button" onClick={handleLogout}>
              Kilépés
            </button>
          </div>
        </div>
      </nav>
    </header>
  );
}
