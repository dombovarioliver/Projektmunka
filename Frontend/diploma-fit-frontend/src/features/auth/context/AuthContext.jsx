import { useEffect, useMemo, useState } from "react";

import {
  getCurrentUser,
  login as loginRequest,
  logoutRequest,
  register as registerRequest,
} from "../services/authService";

import { AuthContext } from "./authContextInstance";

function getStoredAuthState() {
  return {
    accessToken: localStorage.getItem("accessToken"),
    accessTokenExpiresAt: localStorage.getItem("accessTokenExpiresAt"),
    refreshToken: localStorage.getItem("refreshToken"),
    refreshTokenExpiresAt: localStorage.getItem("refreshTokenExpiresAt"),
    userId: localStorage.getItem("userId"),
    email: localStorage.getItem("email"),
    name: localStorage.getItem("name"),
    profilePictureUrl: localStorage.getItem("profilePictureUrl"),
  };
}

function saveAuthData(data, fallbackEmail = "") {
  localStorage.setItem("accessToken", data.accessToken || "");
  localStorage.setItem("accessTokenExpiresAt", data.accessTokenExpiresAt || "");
  localStorage.setItem("refreshToken", data.refreshToken || "");
  localStorage.setItem("refreshTokenExpiresAt", data.refreshTokenExpiresAt || "");
  localStorage.setItem("userId", data.userId || "");
  localStorage.setItem("email", data.email || fallbackEmail || "");
  localStorage.setItem("name", data.name || "Felhasználó");
  localStorage.setItem("profilePictureUrl", data.profilePictureUrl || "");
}

function clearAuthData() {
  localStorage.removeItem("accessToken");
  localStorage.removeItem("accessTokenExpiresAt");
  localStorage.removeItem("refreshToken");
  localStorage.removeItem("refreshTokenExpiresAt");
  localStorage.removeItem("userId");
  localStorage.removeItem("email");
  localStorage.removeItem("name");
  localStorage.removeItem("profilePictureUrl");
}

export function AuthProvider({ children }) {
  const [authState, setAuthState] = useState(() => getStoredAuthState());
  const [isLoading, setIsLoading] = useState(true);

  const isAuthenticated = Boolean(authState.accessToken);

  useEffect(() => {
    let ignore = false;

    async function loadUser() {
      const stored = getStoredAuthState();

      if (!stored.accessToken) {
        if (!ignore) {
          setAuthState(stored);
          setIsLoading(false);
        }
        return;
      }

      try {
        const currentUser = await getCurrentUser();

        if (ignore) return;

        const updatedState = {
          ...getStoredAuthState(),
          userId: currentUser?.userId || stored.userId,
          email: currentUser?.email || stored.email,
          name: currentUser?.name || stored.name || "Felhasználó",
          profilePictureUrl:
            currentUser?.profilePictureUrl || stored.profilePictureUrl || "",
        };

        localStorage.setItem("userId", updatedState.userId || "");
        localStorage.setItem("email", updatedState.email || "");
        localStorage.setItem("name", updatedState.name || "Felhasználó");
        localStorage.setItem(
          "profilePictureUrl",
          updatedState.profilePictureUrl || ""
        );

        setAuthState(updatedState);
        window.dispatchEvent(new Event("profileUpdated"));
      } catch (error) {
        console.error("Aktuális felhasználó betöltési hiba:", error);

        clearAuthData();

        if (!ignore) {
          setAuthState(getStoredAuthState());
        }
      } finally {
        if (!ignore) {
          setIsLoading(false);
        }
      }
    }

    loadUser();

    return () => {
      ignore = true;
    };
  }, []);

  async function login(email, password) {
    const data = await loginRequest(email, password);

    saveAuthData(data, email);

    const updatedState = getStoredAuthState();
    setAuthState(updatedState);

    window.dispatchEvent(new Event("profileUpdated"));

    return data;
  }

  async function register(registerData) {
    const data = await registerRequest(registerData);

    saveAuthData(data, registerData.email);

    const updatedState = getStoredAuthState();
    setAuthState(updatedState);

    window.dispatchEvent(new Event("profileUpdated"));

    return data;
  }

  async function logout() {
    try {
      if (localStorage.getItem("accessToken")) {
        await logoutRequest();
      }
    } catch (error) {
      console.warn("Szerver oldali kijelentkezés sikertelen:", error);
    } finally {
      clearAuthData();
      setAuthState(getStoredAuthState());
      window.dispatchEvent(new Event("profileUpdated"));
      window.location.href = "/login";
    }
  }

  const value = useMemo(
    () => ({
      accessToken: authState.accessToken,
      refreshToken: authState.refreshToken,
      userEmail: authState.email,
      userName: authState.name,
      userId: authState.userId,
      profilePictureUrl: authState.profilePictureUrl,
      isAuthenticated,
      isLoading,
      login,
      register,
      logout,
    }),
    [authState, isAuthenticated, isLoading]
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}
