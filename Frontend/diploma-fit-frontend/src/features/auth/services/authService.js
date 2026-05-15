import axiosClient from "../../../api/axiosClient";

export async function login(email, password) {
  const response = await axiosClient.post("/api/Auth/login", {
    email,
    password,
  });

  return response.data;
}

export async function register(registerData) {
  const response = await axiosClient.post("/api/Auth/register", registerData);
  return response.data;
}

export async function getCurrentUser() {
  const response = await axiosClient.get("/api/Auth/me");
  return response.data;
}

export async function refreshToken(userId, refreshTokenValue) {
  const response = await axiosClient.post("/api/Auth/refresh", {
    userId,
    refreshToken: refreshTokenValue,
  });

  return response.data;
}

export async function logoutRequest() {
  const response = await axiosClient.post("/api/Auth/logout");
  return response.data;
}
