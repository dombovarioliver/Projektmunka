import { NavLink, Link, useLocation } from "react-router-dom";
import { useCallback, useEffect, useRef, useState } from "react";

import { useAuth } from "../../features/auth/context/useAuth";
import { createChatConnection } from "../../features/chat/services/chatService";
import {
  getNavbarNotificationCounts,
  NAVBAR_NOTIFICATIONS_REFRESH_EVENT,
} from "../../features/notifications/services/notificationService";
import "./Navbar.css";

const initialNotificationCounts = {
  friendRequestCount: 0,
  unreadMessageCount: 0,
};

export default function Navbar() {
  const { logout } = useAuth();
  const location = useLocation();
  const connectionRef = useRef(null);

  const [isMenuOpen, setIsMenuOpen] = useState(false);
  const [notificationCounts, setNotificationCounts] = useState(initialNotificationCounts);
  const [userData, setUserData] = useState({
    name: localStorage.getItem("name") || "Felhasználó",
    profilePictureUrl: localStorage.getItem("profilePictureUrl") || "",
  });

  const loadNotificationCounts = useCallback(async () => {
    const accessToken = localStorage.getItem("accessToken");

    if (!accessToken) {
      setNotificationCounts(initialNotificationCounts);
      return;
    }

    try {
      const counts = await getNavbarNotificationCounts();
      setNotificationCounts(counts);
    } catch (error) {
      console.error("Navbar értesítések betöltési hiba:", error);
    }
  }, []);

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

  useEffect(() => {
    loadNotificationCounts();

    window.addEventListener(NAVBAR_NOTIFICATIONS_REFRESH_EVENT, loadNotificationCounts);
    window.addEventListener("storage", loadNotificationCounts);

    const refreshTimer = window.setInterval(loadNotificationCounts, 30000);

    return () => {
      window.removeEventListener(NAVBAR_NOTIFICATIONS_REFRESH_EVENT, loadNotificationCounts);
      window.removeEventListener("storage", loadNotificationCounts);
      window.clearInterval(refreshTimer);
    };
  }, [loadNotificationCounts]);

  useEffect(() => {
    const accessToken = localStorage.getItem("accessToken");
    if (!accessToken) return undefined;

    const connection = createChatConnection();
    connectionRef.current = connection;

    connection.on("ReceiveMessage", () => {
      loadNotificationCounts();
    });

    connection.on("MessageSent", () => {
      loadNotificationCounts();
    });

    connection.onreconnected(() => {
      loadNotificationCounts();
    });

    async function startConnection() {
      try {
        await connection.start();
      } catch (error) {
        console.error("Navbar SignalR kapcsolódási hiba:", error);
      }
    }

    startConnection();

    return () => {
      connection.stop();
      connectionRef.current = null;
    };
  }, [loadNotificationCounts]);

  useEffect(() => {
    loadNotificationCounts();
  }, [location.pathname, location.search, loadNotificationCounts]);

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
    setNotificationCounts(initialNotificationCounts);
    logout();
  }

  function renderNavLabel(item) {
    const count =
      item.badgeKey === "friendRequestCount"
        ? notificationCounts.friendRequestCount
        : item.badgeKey === "unreadMessageCount"
          ? notificationCounts.unreadMessageCount
          : 0;

    return (
      <span className="neura-nav-link-content">
        <span>{item.label}</span>
        {count > 0 && (
          <span className="neura-nav-badge" aria-label={`${count} új értesítés`}>
            {count > 99 ? "99+" : count}
          </span>
        )}
      </span>
    );
  }

  const navItems = [
    { to: "/", label: "Főoldal" },
    { to: "/gyms", label: "Konditermek" },
    { to: "/exercises", label: "Gyakorlatok" },
    { to: "/workout-plan", label: "Edzésterv" },
    { to: "/diet-plan", label: "Étrend" },
    { to: "/friends", label: "Barátok", badgeKey: "friendRequestCount" },
    { to: "/chat", label: "Chat", badgeKey: "unreadMessageCount" },
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
                {renderNavLabel(item)}
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
