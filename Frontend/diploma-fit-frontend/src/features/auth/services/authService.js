import axiosClient from "../../../api/axiosClient";

export async function login(email, password) {
  const response = await axiosClient.post("/Auth/login", {
    email,
    password,
  });

  return response.data;
}

export async function getCurrentUser() {
  const response = await axiosClient.get("/Auth/me");
  return response.data;
}