import axiosClient from "../../../api/axiosClient";

export async function generateDietPlan(userId) {
  const response = await axiosClient.post(`/api/diet-plans/generate/${userId}`);
  return response.data;
}

export async function getFoods() {
  const response = await axiosClient.get("/api/foods");
  return response.data;
}
