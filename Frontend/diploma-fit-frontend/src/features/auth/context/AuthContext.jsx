import { useEffect, useState } from "react";

import {
  getCurrentUser,
  login as loginRequest,
} from "../services/authService";

import { AuthContext } from "./authContextInstance";

export function AuthProvider({ children }) {
  const [userEmail, setUserEmail] = useState(null);
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
        setUserEmail(email);
      } catch {
        localStorage.removeItem("accessToken");
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

    setUserEmail(email);
  }

  function logout() {
    localStorage.removeItem("accessToken");
    setUserEmail(null);
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