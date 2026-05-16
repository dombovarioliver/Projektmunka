import axiosClient from "../../../api/axiosClient";

export async function getFriends() {
  const response = await axiosClient.get("/api/friends");
  return response.data;
}

export async function getIncomingFriendRequests() {
  const response = await axiosClient.get("/api/friends/requests/incoming");
  return response.data;
}

export async function getOutgoingFriendRequests() {
  const response = await axiosClient.get("/api/friends/requests/outgoing");
  return response.data;
}

export async function searchUsers(query) {
  const response = await axiosClient.get("/api/friends/search", {
    params: { query },
  });
  return response.data;
}

export async function sendFriendRequest(addresseeId) {
  const response = await axiosClient.post("/api/friends/requests", {
    addresseeId,
  });
  return response.data;
}

export async function acceptFriendRequest(friendshipId) {
  const response = await axiosClient.post(
    `/api/friends/requests/${friendshipId}/accept`
  );
  return response.data;
}

export async function rejectFriendRequest(friendshipId) {
  const response = await axiosClient.post(
    `/api/friends/requests/${friendshipId}/reject`
  );
  return response.data;
}

export async function removeFriend(friendId) {
  await axiosClient.delete(`/api/friends/${friendId}`);
}
