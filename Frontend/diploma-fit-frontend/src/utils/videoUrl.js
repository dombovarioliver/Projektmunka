const API_ORIGIN = import.meta.env.VITE_API_ORIGIN || "http://localhost:8080";

export function getVideoUrl(videoUrl) {
  if (!videoUrl) return "";

  if (videoUrl.startsWith("http")) {
    return videoUrl;
  }

  return `${API_ORIGIN}${videoUrl.startsWith("/") ? "" : "/"}${videoUrl}`;
}