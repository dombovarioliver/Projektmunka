import { BrowserRouter, Routes, Route } from "react-router-dom";
import HomePage from "../features/home/pages/HomePage";

export default function AppRouter() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<HomePage />} />
      </Routes>
    </BrowserRouter>
  );
}