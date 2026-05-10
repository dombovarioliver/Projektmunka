import axiosClient from "../../../api/axiosClient";

export async function getUsers() {
  const response = await axiosClient.get("/api/users");
  return response.data;
}

export async function getUserById(userId) {
  const response = await axiosClient.get(`/api/users/${userId}`);
  return response.data;
}

export async function updateUser(userId, payload) {
  const response = await axiosClient.put(`/api/users/${userId}`, payload, {
    headers: {
      "Content-Type": "application/json",
    },
  });

  return response.data;
}

export async function uploadProfilePicture(userId, file) {
  const formData = new FormData();
  formData.append("file", file);

  const response = await axiosClient.post(
    `/api/users/${userId}/profile-picture`,
    formData,
    {
      headers: {
        "Content-Type": "multipart/form-data",
      },
    }
  );

  return response.data;
}