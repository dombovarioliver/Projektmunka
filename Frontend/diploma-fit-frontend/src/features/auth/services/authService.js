import axiosClient from "../../../api/axiosClient";

export async function login(email, password) {
  const response = await axiosClient.post("/api/Auth/login", {
    email,
    password,
  });

  return response.data;
}

export async function getCurrentUser() {
  const response = await axiosClient.get("/api/Auth/me");
  return response.data;
}