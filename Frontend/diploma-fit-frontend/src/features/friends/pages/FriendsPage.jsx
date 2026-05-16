import { useEffect, useMemo, useState } from "react";
import { useNavigate } from "react-router-dom";

import {
  acceptFriendRequest,
  getFriends,
  getIncomingFriendRequests,
  getOutgoingFriendRequests,
  rejectFriendRequest,
  removeFriend,
  searchUsers,
  sendFriendRequest,
} from "../services/friendService";

import { requestNavbarNotificationRefresh } from "../../notifications/services/notificationService";

import "../styles/friends.css";

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

export default function FriendsPage() {
  const navigate = useNavigate();

  const [friends, setFriends] = useState([]);
  const [incomingRequests, setIncomingRequests] = useState([]);
  const [outgoingRequests, setOutgoingRequests] = useState([]);
  const [searchTerm, setSearchTerm] = useState("");
  const [searchResults, setSearchResults] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isSearching, setIsSearching] = useState(false);
  const [message, setMessage] = useState("");

  const hasSearchTerm = useMemo(() => searchTerm.trim().length >= 2, [searchTerm]);

  async function loadFriendsData() {
    setIsLoading(true);
    try {
      const [friendsData, incomingData, outgoingData] = await Promise.all([
        getFriends(),
        getIncomingFriendRequests(),
        getOutgoingFriendRequests(),
      ]);

      setFriends(friendsData);
      setIncomingRequests(incomingData);
      setOutgoingRequests(outgoingData);
      requestNavbarNotificationRefresh();
    } catch (error) {
      console.error("Barátlista betöltési hiba:", error);
      setMessage("Nem sikerült betölteni a barátlistát.");
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => {
    loadFriendsData();
  }, []);

  useEffect(() => {
    let ignore = false;

    async function runSearch() {
      if (!hasSearchTerm) {
        setSearchResults([]);
        return;
      }

      setIsSearching(true);

      try {
        const data = await searchUsers(searchTerm.trim());
        if (!ignore) setSearchResults(data);
      } catch (error) {
        console.error("Felhasználó keresési hiba:", error);
        if (!ignore) setMessage("Nem sikerült keresni a felhasználók között.");
      } finally {
        if (!ignore) setIsSearching(false);
      }
    }

    const timer = setTimeout(runSearch, 350);

    return () => {
      ignore = true;
      clearTimeout(timer);
    };
  }, [searchTerm, hasSearchTerm]);

  async function handleSendRequest(userId) {
    try {
      const result = await sendFriendRequest(userId);
      setMessage(result?.message || "Barátkérelem elküldve.");
      await loadFriendsData();
      requestNavbarNotificationRefresh();
      if (hasSearchTerm) setSearchResults(await searchUsers(searchTerm.trim()));
    } catch (error) {
      setMessage(error.response?.data || "Nem sikerült elküldeni a barátkérelmet.");
    }
  }

  async function handleAccept(friendshipId) {
    try {
      const result = await acceptFriendRequest(friendshipId);
      setMessage(result?.message || "Barátkérelem elfogadva.");
      await loadFriendsData();
      requestNavbarNotificationRefresh();
    } catch (error) {
      setMessage(error.response?.data || "Nem sikerült elfogadni a kérelmet.");
    }
  }

  async function handleReject(friendshipId) {
    try {
      const result = await rejectFriendRequest(friendshipId);
      setMessage(result?.message || "Barátkérelem elutasítva.");
      await loadFriendsData();
      requestNavbarNotificationRefresh();
    } catch (error) {
      setMessage(error.response?.data || "Nem sikerült elutasítani a kérelmet.");
    }
  }

  async function handleRemove(friendId) {
    try {
      await removeFriend(friendId);
      setMessage("Barát törölve.");
      await loadFriendsData();
      requestNavbarNotificationRefresh();
    } catch (error) {
      setMessage(error.response?.data || "Nem sikerült törölni a barátot.");
    }
  }

  function renderPerson(person, actions) {
    return (
      <div className="friend-row" key={person.userId || person.friendshipId}>
        <div className="friend-person">
          <img
            className="friend-avatar"
            src={getProfileImageUrl(person.profilePictureUrl, person.name)}
            alt={person.name}
          />
          <div>
            <strong>{person.name}</strong>
            <small>{person.email}</small>
          </div>
        </div>

        <div className="friend-actions">{actions}</div>
      </div>
    );
  }

  return (
    <main className="friends-page">
      <div className="friends-shell">
        <section className="friends-hero">
          <h1>Barátok</h1>
          <p>Keresd meg az ismerőseidet, küldj barátkérelmet, majd írj nekik élő chaten.</p>
        </section>

        {message && <div className="friends-message">{message}</div>}

        <div className="friends-grid">
          <section className="friends-card">
            <div className="friends-card-header">
              <h2>Barátlista</h2>
              <span className="friend-count-badge">{friends.length} barát</span>
            </div>

            {isLoading ? (
              <div className="friends-status">Betöltés...</div>
            ) : friends.length === 0 ? (
              <div className="friends-status">Még nincs barátod. Keress rá valakire jobb oldalt.</div>
            ) : (
              <div className="friends-list">
                {friends.map((friend) =>
                  renderPerson(friend, [
                    <button
                      type="button"
                      className="friends-primary-btn"
                      key="chat"
                      onClick={() => navigate(`/chat?friendId=${friend.userId}`)}
                    >
                      Üzenet
                    </button>,
                    <button
                      type="button"
                      className="friends-danger-btn"
                      key="remove"
                      onClick={() => handleRemove(friend.userId)}
                    >
                      Törlés
                    </button>,
                  ])
                )}
              </div>
            )}

            <h3 className="friend-section-title">Bejövő kérelmek</h3>
            {incomingRequests.length === 0 ? (
              <div className="friends-status">Nincs bejövő barátkérelem.</div>
            ) : (
              <div className="friends-list">
                {incomingRequests.map((request) =>
                  renderPerson(request, [
                    <button
                      type="button"
                      className="friends-primary-btn"
                      key="accept"
                      onClick={() => handleAccept(request.friendshipId)}
                    >
                      Elfogadás
                    </button>,
                    <button
                      type="button"
                      className="friends-secondary-btn"
                      key="reject"
                      onClick={() => handleReject(request.friendshipId)}
                    >
                      Elutasítás
                    </button>,
                  ])
                )}
              </div>
            )}
          </section>

          <aside className="friends-card">
            <div className="friends-card-header">
              <h2>Felhasználó keresése</h2>
            </div>

            <div className="friends-search-row">
              <input
                value={searchTerm}
                onChange={(event) => setSearchTerm(event.target.value)}
                placeholder="Név vagy email alapján..."
              />
            </div>

            {!hasSearchTerm && (
              <div className="friends-status">Legalább 2 karaktert írj be a kereséshez.</div>
            )}

            {isSearching && <div className="friends-status">Keresés...</div>}

            {hasSearchTerm && !isSearching && searchResults.length === 0 && (
              <div className="friends-status">Nincs találat.</div>
            )}

            <div className="friends-list">
              {searchResults.map((user) =>
                renderPerson(user, [
                  <button
                    type="button"
                    className="friends-primary-btn"
                    key="add"
                    disabled={user.relationshipStatus !== "none"}
                    onClick={() => handleSendRequest(user.userId)}
                  >
                    {user.relationshipStatus === "friends" && "Már barát"}
                    {user.relationshipStatus === "pending_sent" && "Elküldve"}
                    {user.relationshipStatus === "pending_received" && "Válaszra vár"}
                    {user.relationshipStatus === "none" && "Hozzáadás"}
                  </button>,
                ])
              )}
            </div>

            <h3 className="friend-section-title">Elküldött kérelmek</h3>
            {outgoingRequests.length === 0 ? (
              <div className="friends-status">Nincs függőben lévő elküldött kérelem.</div>
            ) : (
              <div className="friends-list">
                {outgoingRequests.map((request) =>
                  renderPerson(request, [
                    <span className="friend-count-badge" key="pending">Függőben</span>,
                  ])
                )}
              </div>
            )}
          </aside>
        </div>
      </div>
    </main>
  );
}
