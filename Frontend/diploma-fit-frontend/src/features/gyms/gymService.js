import axiosClient from "../../api/axiosClient";

export async function getBudapestGyms() {
  const response = await axiosClient.get("/api/Gyms/budapest");
  return response.data;
}