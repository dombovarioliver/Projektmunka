import { useContext } from "react";
import { AuthContext } from "./authContextInstance";

export function useAuth() {
  const context = useContext(AuthContext);

  if (!context) {
    throw new Error("useAuth csak AuthProvider-en belül használható.");
  }

  return context;
}
