import { useEffect, useMemo, useRef, useState } from "react";
import { useNavigate, useSearchParams } from "react-router-dom";

import { useAuth } from "../../auth/context/useAuth";
import {
  createChatConnection,
  getChatHistory,
  getConversations,
} from "../services/chatService";

import { requestNavbarNotificationRefresh } from "../../notifications/services/notificationService";

import "../styles/chat.css";

function getProfileImageUrl(url, name) {
  if (!url || url.trim() === "") {
    return (
      "https://ui-avatars.com/api/?background=2563eb&color=ffffff&bold=true&name=" +
      encodeURIComponent(name || "User")
    );
  }

  if (url.startsWith("http")) return url;

  const apiOrigin = import.meta.env.VITE_API_ORIGIN || "http://localhost:8080";
  return `${apiOrigin}${url.startsWith("/") ? "" : "/"}${url}`;
}

function formatTime(value) {
  if (!value) return "";

  return new Intl.DateTimeFormat("hu-HU", {
    month: "2-digit",
    day: "2-digit",
    hour: "2-digit",
    minute: "2-digit",
  }).format(new Date(value));
}

export default function ChatPage() {
  const { userId } = useAuth();
  const navigate = useNavigate();
  const [searchParams, setSearchParams] = useSearchParams();

  const [conversations, setConversations] = useState([]);
  const [selectedFriendId, setSelectedFriendId] = useState(searchParams.get("friendId") || "");
  const [messages, setMessages] = useState([]);
  const [newMessage, setNewMessage] = useState("");
  const [isLoading, setIsLoading] = useState(true);
  const [isHistoryLoading, setIsHistoryLoading] = useState(false);
  const [connectionState, setConnectionState] = useState("Kapcsolódás...");
  const [error, setError] = useState("");

  const connectionRef = useRef(null);
  const messagesEndRef = useRef(null);
  const selectedFriendIdRef = useRef(selectedFriendId);

  const selectedConversation = useMemo(
    () => conversations.find((conversation) => conversation.friendId === selectedFriendId),
    [conversations, selectedFriendId]
  );

  useEffect(() => {
    selectedFriendIdRef.current = selectedFriendId;
  }, [selectedFriendId]);

  useEffect(() => {
    async function loadConversations() {
      try {
        const data = await getConversations();
        setConversations(data);
        requestNavbarNotificationRefresh();

        const friendIdFromUrl = searchParams.get("friendId");
        if (!selectedFriendId && friendIdFromUrl) {
          setSelectedFriendId(friendIdFromUrl);
        } else if (!selectedFriendId && data.length > 0) {
          setSelectedFriendId(data[0].friendId);
          setSearchParams({ friendId: data[0].friendId });
        }
      } catch (loadError) {
        console.error("Beszélgetések betöltési hiba:", loadError);
        setError("Nem sikerült betölteni a beszélgetéseket.");
      } finally {
        setIsLoading(false);
      }
    }

    loadConversations();
  }, []);

  useEffect(() => {
    const connection = createChatConnection();
    connectionRef.current = connection;

    connection.on("ReceiveMessage", (message) => {
      handleIncomingMessage(message);
    });

    connection.on("MessageSent", (message) => {
      handleIncomingMessage(message);
    });

    connection.onreconnecting(() => setConnectionState("Újracsatlakozás..."));
    connection.onreconnected(() => setConnectionState("Online"));
    connection.onclose(() => setConnectionState("Kapcsolat megszakadt"));

    async function startConnection() {
      try {
        await connection.start();
        setConnectionState("Online");
      } catch (connectionError) {
        console.error("SignalR kapcsolódási hiba:", connectionError);
        setConnectionState("Nem sikerült csatlakozni");
      }
    }

    startConnection();

    return () => {
      connection.stop();
    };
  }, []);

  useEffect(() => {
    if (!selectedFriendId) return;

    async function loadHistory() {
      setIsHistoryLoading(true);
      setError("");

      try {
        const data = await getChatHistory(selectedFriendId);
        setMessages(data);
        requestNavbarNotificationRefresh();
        setConversations((prev) =>
          prev.map((conversation) =>
            conversation.friendId === selectedFriendId
              ? { ...conversation, unreadCount: 0 }
              : conversation
          )
        );
      } catch (historyError) {
        console.error("Chat előzmény hiba:", historyError);
        setError("Nem sikerült betölteni az üzeneteket.");
      } finally {
        setIsHistoryLoading(false);
      }
    }

    setSearchParams({ friendId: selectedFriendId });
    loadHistory();
  }, [selectedFriendId, setSearchParams]);

  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: "smooth" });
  }, [messages]);

  function handleIncomingMessage(message) {
    const friendId = message.senderId === userId ? message.receiverId : message.senderId;
    const isOpenConversation = selectedFriendIdRef.current === friendId;

    if (isOpenConversation) {
      setMessages((prev) => {
        if (prev.some((item) => item.id === message.id)) return prev;
        return [...prev, message];
      });
    }

    requestNavbarNotificationRefresh();

    setConversations((prev) =>
      prev
        .map((conversation) => {
          if (conversation.friendId !== friendId) return conversation;

          return {
            ...conversation,
            lastMessageText: message.text,
            lastMessageAt: message.sentAt,
            unreadCount:
              message.receiverId === userId && !isOpenConversation
                ? conversation.unreadCount + 1
                : conversation.unreadCount,
          };
        })
        .sort((a, b) => new Date(b.lastMessageAt || 0) - new Date(a.lastMessageAt || 0))
    );
  }

  async function handleSendMessage(event) {
    event.preventDefault();

    const text = newMessage.trim();
    if (!text || !selectedFriendId || !connectionRef.current) return;

    try {
      setNewMessage("");
      await connectionRef.current.invoke("SendMessage", selectedFriendId, text);
      requestNavbarNotificationRefresh();
    } catch (sendError) {
      console.error("Üzenet küldési hiba:", sendError);
      setError("Nem sikerült elküldeni az üzenetet.");
      setNewMessage(text);
    }
  }

  return (
    <main className="chat-page">
      <div className="chat-shell">
        <section className="chat-hero">
          <h1>Élő chat</h1>
          <p>SignalR alapú valós idejű üzenetküldés a barátaiddal.</p>
        </section>

        <div className="chat-layout">
          <aside className="chat-card">
            <div className="chat-sidebar-header">
              <h2>Beszélgetések</h2>
              <p>{connectionState}</p>
            </div>

            {isLoading ? (
              <div className="chat-status">Beszélgetések betöltése...</div>
            ) : conversations.length === 0 ? (
              <div className="chat-empty-state">
                Még nincs kivel beszélgetni. Először vegyél fel barátokat.
                <br />
                <br />
                <button
                  type="button"
                  className="chat-friends-button"
                  onClick={() => navigate("/friends")}
                >
                  Barátok megnyitása
                </button>
              </div>
            ) : (
              <div className="chat-conversation-list">
                {conversations.map((conversation) => (
                  <button
                    type="button"
                    key={conversation.friendId}
                    className={
                      selectedFriendId === conversation.friendId
                        ? "chat-conversation-button active"
                        : "chat-conversation-button"
                    }
                    onClick={() => setSelectedFriendId(conversation.friendId)}
                  >
                    <img
                      className="chat-avatar"
                      src={getProfileImageUrl(
                        conversation.friendProfilePictureUrl,
                        conversation.friendName
                      )}
                      alt={conversation.friendName}
                    />
                    <span className="chat-conversation-main">
                      <strong>{conversation.friendName}</strong>
                      <small>{conversation.lastMessageText || conversation.friendEmail}</small>
                    </span>
                    {conversation.unreadCount > 0 && (
                      <span className="chat-unread-badge">{conversation.unreadCount}</span>
                    )}
                  </button>
                ))}
              </div>
            )}
          </aside>

          <section className="chat-card chat-window">
            {selectedConversation ? (
              <>
                <div className="chat-window-header">
                  <img
                    className="chat-avatar"
                    src={getProfileImageUrl(
                      selectedConversation.friendProfilePictureUrl,
                      selectedConversation.friendName
                    )}
                    alt={selectedConversation.friendName}
                  />
                  <div>
                    <h2>{selectedConversation.friendName}</h2>
                    <p>{selectedConversation.friendEmail}</p>
                  </div>
                  <div className="chat-header-spacer" />
                  <button
                    type="button"
                    className="chat-friends-button"
                    onClick={() => navigate("/friends")}
                  >
                    Barátok
                  </button>
                </div>

                {error && <div className="chat-error">{error}</div>}

                <div className="chat-messages">
                  {isHistoryLoading ? (
                    <div className="chat-status">Üzenetek betöltése...</div>
                  ) : messages.length === 0 ? (
                    <div className="chat-empty-state">Még nincs üzenet. Írj neki először.</div>
                  ) : (
                    messages.map((message) => (
                      <div
                        className={
                          message.senderId === userId
                            ? "chat-bubble-row own"
                            : "chat-bubble-row"
                        }
                        key={message.id}
                      >
                        <div className="chat-bubble">
                          <p>{message.text}</p>
                          <time>{formatTime(message.sentAt)}</time>
                        </div>
                      </div>
                    ))
                  )}
                  <div ref={messagesEndRef} />
                </div>

                <form className="chat-form" onSubmit={handleSendMessage}>
                  <input
                    value={newMessage}
                    onChange={(event) => setNewMessage(event.target.value)}
                    placeholder="Írj üzenetet..."
                    maxLength={2000}
                  />
                  <button
                    type="submit"
                    className="chat-send-button"
                    disabled={!newMessage.trim() || connectionState !== "Online"}
                  >
                    Küldés
                  </button>
                </form>
              </>
            ) : (
              <div className="chat-empty-state">
                Válassz ki egy beszélgetést, vagy adj hozzá barátokat.
              </div>
            )}
          </section>
        </div>
      </div>
    </main>
  );
}
