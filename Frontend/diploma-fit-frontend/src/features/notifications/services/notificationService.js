import { getConversations } from "../../chat/services/chatService";
import { getIncomingFriendRequests } from "../../friends/services/friendService";

export const NAVBAR_NOTIFICATIONS_REFRESH_EVENT = "navbarNotificationsRefresh";

export function requestNavbarNotificationRefresh() {
  window.dispatchEvent(new Event(NAVBAR_NOTIFICATIONS_REFRESH_EVENT));
}

export async function getNavbarNotificationCounts() {
  const [friendRequestsResult, conversationsResult] = await Promise.allSettled([
    getIncomingFriendRequests(),
    getConversations(),
  ]);

  const friendRequestCount =
    friendRequestsResult.status === "fulfilled" && Array.isArray(friendRequestsResult.value)
      ? friendRequestsResult.value.length
      : 0;

  const unreadMessageCount =
    conversationsResult.status === "fulfilled" && Array.isArray(conversationsResult.value)
      ? conversationsResult.value.reduce(
          (sum, conversation) => sum + Number(conversation.unreadCount || 0),
          0
        )
      : 0;

  return {
    friendRequestCount,
    unreadMessageCount,
  };
}
