import { useEffect, useState } from "react";

import {
  getCurrentUser,
  login as loginRequest,
} from "../services/authService";

import { AuthContext } from "./authContextInstance";

export function AuthProvider({ children }) {
  const [userEmail, setUserEmail] = useState(
    localStorage.getItem("email")
  );

  const [isLoading, setIsLoading] = useState(true);

  const isAuthenticated = Boolean(localStorage.getItem("accessToken"));

  useEffect(() => {
    async function loadUser() {
      const token = localStorage.getItem("accessToken");

      if (!token) {
        setIsLoading(false);
        return;
      }

      try {
        const email = await getCurrentUser();

        setUserEmail(email || localStorage.getItem("email"));
      } catch {
        localStorage.removeItem("accessToken");
        localStorage.removeItem("userId");
        localStorage.removeItem("email");

        setUserEmail(null);
      } finally {
        setIsLoading(false);
      }
    }

    loadUser();
  }, []);

  async function login(email, password) {
    const data = await loginRequest(email, password);

    localStorage.setItem("accessToken", data.token);
    localStorage.setItem("email", data.email || email);

    if (data.userId || data.id) {
      localStorage.setItem("userId", data.userId || data.id);
    }

    setUserEmail(data.email || email);

    return data;
  }

  function logout() {
    localStorage.removeItem("accessToken");
    localStorage.removeItem("userId");
    localStorage.removeItem("email");

    setUserEmail(null);

    window.location.href = "/login";
  }

  return (
    <AuthContext.Provider
      value={{
        userEmail,
        isAuthenticated,
        isLoading,
        login,
        logout,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}