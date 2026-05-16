import { useEffect, useMemo, useRef, useState } from "react";
import { useNavigate, useSearchParams } from "react-router-dom";

import { useAuth } from "../../auth/context/useAuth";
import { getFriends } from "../../friends/services/friendService";
import {
  createChatConnection,
  createGroupConversation,
  getConversationMessages,
  getConversations,
  updateConversationSettings,
} from "../services/chatService";

import "../styles/chat.css";

const EMOJIS = ["👍", "❤️", "😂", "🔥", "💪", "👏", "😎", "😅", "🥹", "😡", "✅", "🙏", "🎉", "🤝", "👀", "🚀", "🏋️", "🍗"];

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

function getConversationAvatar(conversation) {
  if (conversation.isGroup) return null;
  return getProfileImageUrl(
    conversation.friendProfilePictureUrl,
    conversation.displayName || conversation.friendName
  );
}

export default function ChatPage() {
  const { userId } = useAuth();
  const navigate = useNavigate();
  const [searchParams, setSearchParams] = useSearchParams();

  const [conversations, setConversations] = useState([]);
  const [selectedConversationId, setSelectedConversationId] = useState(
    searchParams.get("conversationId") || ""
  );
  const [messages, setMessages] = useState([]);
  const [newMessage, setNewMessage] = useState("");
  const [isLoading, setIsLoading] = useState(true);
  const [isHistoryLoading, setIsHistoryLoading] = useState(false);
  const [connectionState, setConnectionState] = useState("Kapcsolódás...");
  const [error, setError] = useState("");
  const [showEmojiPicker, setShowEmojiPicker] = useState(false);
  const [showSettings, setShowSettings] = useState(false);
  const [nickname, setNickname] = useState("");
  const [quickEmoji, setQuickEmoji] = useState("👍");
  const [friends, setFriends] = useState([]);
  const [showGroupModal, setShowGroupModal] = useState(false);
  const [groupTitle, setGroupTitle] = useState("");
  const [selectedGroupMembers, setSelectedGroupMembers] = useState([]);

  const connectionRef = useRef(null);
  const messagesEndRef = useRef(null);
  const selectedConversationIdRef = useRef(selectedConversationId);

  const selectedConversation = useMemo(
    () => conversations.find((conversation) => conversation.conversationId === selectedConversationId),
    [conversations, selectedConversationId]
  );

  useEffect(() => {
    selectedConversationIdRef.current = selectedConversationId;
  }, [selectedConversationId]);

  useEffect(() => {
    async function loadInitialData() {
      try {
        const [conversationData, friendData] = await Promise.all([
          getConversations(),
          getFriends(),
        ]);

        setConversations(conversationData);
        setFriends(friendData);

        const conversationIdFromUrl = searchParams.get("conversationId");
        const friendIdFromUrl = searchParams.get("friendId");
        const urlConversation = conversationData.find(
          (conversation) => conversation.conversationId === conversationIdFromUrl
        );
        const oldUrlConversation = conversationData.find(
          (conversation) => conversation.friendId === friendIdFromUrl
        );

        if (!selectedConversationId && urlConversation) {
          setSelectedConversationId(urlConversation.conversationId);
        } else if (!selectedConversationId && oldUrlConversation) {
          setSelectedConversationId(oldUrlConversation.conversationId);
          setSearchParams({ conversationId: oldUrlConversation.conversationId });
        } else if (!selectedConversationId && conversationData.length > 0) {
          setSelectedConversationId(conversationData[0].conversationId);
          setSearchParams({ conversationId: conversationData[0].conversationId });
        }
      } catch (loadError) {
        console.error("Beszélgetések betöltési hiba:", loadError);
        setError("Nem sikerült betölteni a beszélgetéseket.");
      } finally {
        setIsLoading(false);
      }
    }

    loadInitialData();
  }, []);

  useEffect(() => {
    const connection = createChatConnection();
    connectionRef.current = connection;

    connection.on("ReceiveConversationMessage", (message) => {
      handleIncomingMessage(message);
    });

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
    if (!selectedConversationId) return;

    async function loadHistory() {
      setIsHistoryLoading(true);
      setError("");

      try {
        const data = await getConversationMessages(selectedConversationId);
        setMessages(data);
        setConversations((prev) =>
          prev.map((conversation) =>
            conversation.conversationId === selectedConversationId
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

    setSearchParams({ conversationId: selectedConversationId });
    loadHistory();
  }, [selectedConversationId, setSearchParams]);

  useEffect(() => {
    if (!selectedConversation) return;

    setNickname(selectedConversation.currentUserNickname || "");
    setQuickEmoji(selectedConversation.currentUserQuickEmoji || "👍");
  }, [selectedConversation]);

  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: "smooth" });
  }, [messages]);

  function handleIncomingMessage(message) {
    const conversationId = message.conversationId;
    const isOpenConversation = selectedConversationIdRef.current === conversationId;

    if (isOpenConversation) {
      setMessages((prev) => {
        if (prev.some((item) => item.id === message.id)) return prev;
        return [...prev, message];
      });
    }

    setConversations((prev) => {
      let found = false;
      const updated = prev.map((conversation) => {
        if (conversation.conversationId !== conversationId) return conversation;
        found = true;

        return {
          ...conversation,
          lastMessageText: message.text,
          lastMessageAt: message.sentAt,
          unreadCount:
            message.senderId !== userId && !isOpenConversation
              ? conversation.unreadCount + 1
              : conversation.unreadCount,
        };
      });

      return found
        ? updated.sort((a, b) => new Date(b.lastMessageAt || 0) - new Date(a.lastMessageAt || 0))
        : updated;
    });
  }

  async function refreshConversations() {
    const data = await getConversations();
    setConversations(data);
    return data;
  }

  async function handleSendMessage(event) {
    event.preventDefault();
    await sendText(newMessage);
  }

  async function sendText(textToSend) {
    const text = textToSend.trim();
    if (!text || !selectedConversationId || !connectionRef.current) return;

    try {
      setNewMessage("");
      setShowEmojiPicker(false);
      await connectionRef.current.invoke("SendConversationMessage", selectedConversationId, text);
    } catch (sendError) {
      console.error("Üzenet küldési hiba:", sendError);
      setError("Nem sikerült elküldeni az üzenetet.");
      setNewMessage(text);
    }
  }

  async function handleSaveSettings() {
    if (!selectedConversationId) return;

    try {
      const updatedConversation = await updateConversationSettings(selectedConversationId, {
        nickname,
        quickEmoji,
      });

      setConversations((prev) =>
        prev.map((conversation) =>
          conversation.conversationId === selectedConversationId
            ? updatedConversation
            : conversation
        )
      );
      setShowSettings(false);
    } catch (settingsError) {
      console.error("Chat beállítás mentési hiba:", settingsError);
      setError("Nem sikerült menteni a chat beállításokat.");
    }
  }

  async function handleCreateGroup(event) {
    event.preventDefault();

    if (!groupTitle.trim() || selectedGroupMembers.length === 0) {
      setError("Adj nevet a csoportnak, és válassz legalább egy barátot.");
      return;
    }

    try {
      const conversation = await createGroupConversation(groupTitle, selectedGroupMembers);
      const refreshed = await refreshConversations();
      const created = refreshed.find((item) => item.conversationId === conversation.conversationId) || conversation;
      setSelectedConversationId(created.conversationId);
      setGroupTitle("");
      setSelectedGroupMembers([]);
      setShowGroupModal(false);
      setError("");
    } catch (groupError) {
      console.error("Csoport létrehozási hiba:", groupError);
      setError("Nem sikerült létrehozni a csoportot.");
    }
  }

  function toggleGroupMember(friendId) {
    setSelectedGroupMembers((prev) =>
      prev.includes(friendId)
        ? prev.filter((id) => id !== friendId)
        : [...prev, friendId]
    );
  }

  return (
    <main className="chat-page">
      <div className="chat-shell">
        <section className="chat-hero">
          <div>
            <h1>Élő chat</h1>
            <p>Privát és csoportos beszélgetések, becenevek, gyors emoji és emoji választó.</p>
          </div>
          <button
            type="button"
            className="chat-primary-outline-button"
            onClick={() => setShowGroupModal(true)}
          >
            + Új csoport
          </button>
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
                    key={conversation.conversationId}
                    className={
                      selectedConversationId === conversation.conversationId
                        ? "chat-conversation-button active"
                        : "chat-conversation-button"
                    }
                    onClick={() => setSelectedConversationId(conversation.conversationId)}
                  >
                    {conversation.isGroup ? (
                      <span className="chat-group-avatar">👥</span>
                    ) : (
                      <img
                        className="chat-avatar"
                        src={getConversationAvatar(conversation)}
                        alt={conversation.displayName || conversation.friendName}
                      />
                    )}
                    <span className="chat-conversation-main">
                      <strong>{conversation.displayName || conversation.friendName}</strong>
                      <small>
                        {conversation.lastMessageText ||
                          (conversation.isGroup ? `${conversation.members?.length || 0} tag` : conversation.friendEmail)}
                      </small>
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
                  {selectedConversation.isGroup ? (
                    <span className="chat-group-avatar big">👥</span>
                  ) : (
                    <img
                      className="chat-avatar"
                      src={getConversationAvatar(selectedConversation)}
                      alt={selectedConversation.displayName || selectedConversation.friendName}
                    />
                  )}
                  <div>
                    <h2>{selectedConversation.displayName || selectedConversation.friendName}</h2>
                    <p>
                      {selectedConversation.isGroup
                        ? `${selectedConversation.members?.length || 0} tag · ${selectedConversation.members
                            ?.map((member) => member.displayName)
                            .join(", ")}`
                        : selectedConversation.friendEmail}
                    </p>
                  </div>
                  <div className="chat-header-spacer" />
                  <button
                    type="button"
                    className="chat-friends-button"
                    onClick={() => setShowSettings((prev) => !prev)}
                  >
                    Beállítások
                  </button>
                </div>

                {showSettings && (
                  <div className="chat-settings-panel">
                    <label>
                      Beceneved ebben a beszélgetésben
                      <input
                        value={nickname}
                        onChange={(event) => setNickname(event.target.value)}
                        placeholder="Pl. Oli"
                        maxLength={80}
                      />
                    </label>
                    <label>
                      Gyors emoji
                      <div className="chat-quick-emoji-row">
                        <input
                          value={quickEmoji}
                          onChange={(event) => setQuickEmoji(event.target.value)}
                          maxLength={20}
                        />
                        <div className="chat-settings-emojis">
                          {EMOJIS.slice(0, 10).map((emoji) => (
                            <button
                              key={emoji}
                              type="button"
                              onClick={() => setQuickEmoji(emoji)}
                            >
                              {emoji}
                            </button>
                          ))}
                        </div>
                      </div>
                    </label>
                    <button type="button" className="chat-send-button" onClick={handleSaveSettings}>
                      Mentés
                    </button>
                  </div>
                )}

                {error && <div className="chat-error">{error}</div>}

                <div className="chat-messages">
                  {isHistoryLoading ? (
                    <div className="chat-status">Üzenetek betöltése...</div>
                  ) : messages.length === 0 ? (
                    <div className="chat-empty-state">Még nincs üzenet. Írj először.</div>
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
                          {selectedConversation.isGroup && message.senderId !== userId && (
                            <strong className="chat-sender-name">
                              {message.senderDisplayName || message.senderName}
                            </strong>
                          )}
                          <p>{message.text}</p>
                          <time>{formatTime(message.sentAt)}</time>
                        </div>
                      </div>
                    ))
                  )}
                  <div ref={messagesEndRef} />
                </div>

                <form className="chat-form" onSubmit={handleSendMessage}>
                  <div className="chat-input-wrap">
                    <input
                      value={newMessage}
                      onChange={(event) => setNewMessage(event.target.value)}
                      placeholder="Írj üzenetet..."
                      maxLength={2000}
                    />
                    {showEmojiPicker && (
                      <div className="chat-emoji-picker">
                        {EMOJIS.map((emoji) => (
                          <button
                            type="button"
                            key={emoji}
                            onClick={() => setNewMessage((prev) => `${prev}${emoji}`)}
                          >
                            {emoji}
                          </button>
                        ))}
                      </div>
                    )}
                  </div>
                  <button
                    type="button"
                    className="chat-emoji-button"
                    onClick={() => setShowEmojiPicker((prev) => !prev)}
                    title="Emoji választó"
                  >
                    😊
                  </button>
                  <button
                    type="button"
                    className="chat-emoji-button"
                    onClick={() => sendText(quickEmoji)}
                    disabled={connectionState !== "Online"}
                    title="Gyors emoji"
                  >
                    {quickEmoji || "👍"}
                  </button>
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
                Válassz ki egy beszélgetést, hozz létre csoportot, vagy adj hozzá barátokat.
              </div>
            )}
          </section>
        </div>
      </div>

      {showGroupModal && (
        <div className="chat-modal-backdrop" onClick={() => setShowGroupModal(false)}>
          <form className="chat-modal" onSubmit={handleCreateGroup} onClick={(event) => event.stopPropagation()}>
            <div className="chat-modal-header">
              <h2>Új csoport létrehozása</h2>
              <button type="button" onClick={() => setShowGroupModal(false)}>×</button>
            </div>

            <label>
              Csoport neve
              <input
                value={groupTitle}
                onChange={(event) => setGroupTitle(event.target.value)}
                placeholder="Pl. Edzőtársak"
                maxLength={120}
              />
            </label>

            <div className="chat-group-member-list">
              {friends.length === 0 ? (
                <div className="chat-empty-state">Nincs még barátod, akit hozzáadhatnál.</div>
              ) : (
                friends.map((friend) => (
                  <label className="chat-group-member" key={friend.userId}>
                    <input
                      type="checkbox"
                      checked={selectedGroupMembers.includes(friend.userId)}
                      onChange={() => toggleGroupMember(friend.userId)}
                    />
                    <img
                      className="chat-avatar"
                      src={getProfileImageUrl(friend.profilePictureUrl, friend.name)}
                      alt={friend.name}
                    />
                    <span>
                      <strong>{friend.name}</strong>
                      <small>{friend.email}</small>
                    </span>
                  </label>
                ))
              )}
            </div>

            <button type="submit" className="chat-send-button">
              Csoport létrehozása
            </button>
          </form>
        </div>
      )}
    </main>
  );
}
