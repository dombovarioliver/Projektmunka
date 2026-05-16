import axiosClient from "../../../api/axiosClient";

export async function getExercises() {
  const response = await axiosClient.get("/api/exercises");
  return response.data;
}