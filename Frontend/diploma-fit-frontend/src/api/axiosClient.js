import axios from "axios";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || "https://localhost:7281";

const axiosClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    "Content-Type": "application/json",
  },
});

function saveAuthData(data) {
  if (!data) return;

  if (data.accessToken) {
    localStorage.setItem("accessToken", data.accessToken);
  }

  if (data.accessTokenExpiresAt) {
    localStorage.setItem("accessTokenExpiresAt", data.accessTokenExpiresAt);
  }

  if (data.refreshToken) {
    localStorage.setItem("refreshToken", data.refreshToken);
  }

  if (data.refreshTokenExpiresAt) {
    localStorage.setItem("refreshTokenExpiresAt", data.refreshTokenExpiresAt);
  }

  if (data.userId) {
    localStorage.setItem("userId", data.userId);
  }

  if (data.email) {
    localStorage.setItem("email", data.email);
  }

  if (data.name) {
    localStorage.setItem("name", data.name);
  }

  if (data.profilePictureUrl !== undefined) {
    localStorage.setItem("profilePictureUrl", data.profilePictureUrl || "");
  }

  window.dispatchEvent(new Event("profileUpdated"));
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

  window.dispatchEvent(new Event("profileUpdated"));
}

axiosClient.interceptors.request.use((config) => {
  const accessToken = localStorage.getItem("accessToken");

  if (accessToken) {
    config.headers.Authorization = `Bearer ${accessToken}`;
  }

  return config;
});

axiosClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;

    if (
      error.response?.status !== 401 ||
      originalRequest?._retry ||
      originalRequest?.url?.includes("/api/Auth/login") ||
      originalRequest?.url?.includes("/api/Auth/register") ||
      originalRequest?.url?.includes("/api/Auth/refresh")
    ) {
      return Promise.reject(error);
    }

    const userId = localStorage.getItem("userId");
    const refreshToken = localStorage.getItem("refreshToken");

    if (!userId || !refreshToken) {
      clearAuthData();
      return Promise.reject(error);
    }

    try {
      originalRequest._retry = true;

      const response = await axios.post(`${API_BASE_URL}/api/Auth/refresh`, {
        userId,
        refreshToken,
      });

      saveAuthData(response.data);

      originalRequest.headers.Authorization = `Bearer ${response.data.accessToken}`;

      return axiosClient(originalRequest);
    } catch (refreshError) {
      clearAuthData();
      return Promise.reject(refreshError);
    }
  }
);

export default axiosClient;
