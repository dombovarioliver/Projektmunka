import axiosClient from "../../../api/axiosClient";

export async function generateWorkoutPlan(payload) {
  const response = await axiosClient.post(
    "/api/workout-plans/generate",
    payload,
    {
      headers: {
        "Content-Type": "application/json",
      },
    }
  );

  return response.data;
}