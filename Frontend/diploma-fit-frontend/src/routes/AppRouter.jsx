import { BrowserRouter, Routes, Route } from "react-router-dom";
import MainLayout from "../components/layout/MainLayout";

import HomePage from "../features/home/pages/HomePage";
import ExercisesPage from "../features/exercises/pages/ExercisesPage";
import WorkoutPlanPage from "../features/workoutPlans/pages/WorkoutPlanPage";
import ChatPage from "../features/chat/pages/ChatPage";
import ProfilePage from "../features/users/pages/ProfilePage";
import GymsMapPage from "../features/gyms/pages/GymsMapPage";

export default function AppRouter() {
  return (
    <BrowserRouter>
      <Routes>
        <Route element={<MainLayout />}>
          <Route path="/" element={<HomePage />} />
          <Route path="/exercises" element={<ExercisesPage />} />
          <Route path="/workout-plan" element={<WorkoutPlanPage />} />
          <Route path="/chat" element={<ChatPage />} />
          <Route path="/profile" element={<ProfilePage />} />
          <Route path="/gyms" element={<GymsMapPage />} />
        </Route>
      </Routes>
    </BrowserRouter>
  );
}