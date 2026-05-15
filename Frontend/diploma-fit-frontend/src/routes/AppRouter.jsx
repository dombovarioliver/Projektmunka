import { BrowserRouter, Navigate, Route, Routes } from "react-router-dom";

import MainLayout from "../components/layout/MainLayout";

import HomePage from "../features/home/pages/HomePage";
import ExercisesPage from "../features/exercises/pages/ExercisesPage";
import WorkoutPlanPage from "../features/workoutPlans/pages/WorkoutPlanPage";
import ChatPage from "../features/chat/pages/ChatPage";
import ProfilePage from "../features/users/pages/ProfilePage";
import GymsMapPage from "../features/gyms/pages/GymsMapPage";
import LoginPage from "../features/auth/pages/LoginPage";
import DietPlanPage from "../features/diet/pages/DietPlanPage";

import { useAuth } from "../features/auth/context/useAuth";

function ProtectedRoute({ children }) {
  const { isAuthenticated, isLoading } = useAuth();

  if (isLoading) {
    return <div className="container py-5">Betöltés...</div>;
  }

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  return children;
}

export default function AppRouter() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/login" element={<LoginPage />} />

        <Route
          element={
            <ProtectedRoute>
              <MainLayout />
            </ProtectedRoute>
          }
        >
          <Route path="/" element={<HomePage />} />
          <Route path="/gyms" element={<GymsMapPage />} />
          <Route path="/exercises" element={<ExercisesPage />} />
          <Route path="/workout-plan" element={<WorkoutPlanPage />} />
          <Route path="/diet-plan" element={<DietPlanPage />} />
          <Route path="/chat" element={<ChatPage />} />
          <Route path="/profile" element={<ProfilePage />} />
        </Route>
      </Routes>
    </BrowserRouter>
  );
}