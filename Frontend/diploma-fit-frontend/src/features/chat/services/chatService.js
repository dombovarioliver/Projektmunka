import * as signalR from "@microsoft/signalr";
import axiosClient from "../../../api/axiosClient";

const apiOrigin = import.meta.env.VITE_API_ORIGIN || "http://localhost:8080";

export async function getConversations() {
  const response = await axiosClient.get("/api/chat/conversations");
  return response.data;
}

export async function getChatHistory(friendId) {
  const response = await axiosClient.get(`/api/chat/history/${friendId}`);
  return response.data;
}

export function createChatConnection() {
  return new signalR.HubConnectionBuilder()
    .withUrl(`${apiOrigin}/hubs/chat`, {
      accessTokenFactory: () => localStorage.getItem("accessToken") || "",
    })
    .withAutomaticReconnect()
    .build();
}
