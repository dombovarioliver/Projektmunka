import axiosClient from "../../api/axiosClient";

export async function getBudapestGyms() {
  const response = await axiosClient.get("/Gyms/budapest");
  return response.data;
}