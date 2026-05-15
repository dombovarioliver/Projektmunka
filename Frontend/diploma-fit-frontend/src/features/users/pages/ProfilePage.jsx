import { useEffect, useRef, useState } from "react";
import {
  getUsers,
  getUserById,
  updateUser,
  uploadProfilePicture,
} from "../services/profileService";

import "../styles/profile.css";

export default function ProfilePage() {
  const fileInputRef = useRef(null);

  const [user, setUser] = useState(null);
  const [formData, setFormData] = useState(null);
  const [isEditing, setIsEditing] = useState(false);
  const [isSaving, setIsSaving] = useState(false);
  const [isUploadingImage, setIsUploadingImage] = useState(false);

  useEffect(() => {
    let ignore = false;

    const userId = localStorage.getItem("userId");
    const email = localStorage.getItem("email");

    const request = userId
      ? getUserById(userId)
      : getUsers().then((users) => {
          const currentUser = users.find(
            (u) => u.email?.toLowerCase() === email?.toLowerCase()
          );

          if (currentUser) {
            localStorage.setItem("userId", currentUser.id);
          }

          return currentUser;
        });

    request
      .then((data) => {
        if (ignore || !data) return;

        setUser(data);
        setFormData(data);

        localStorage.setItem("name", data.name || "Felhasználó");
        localStorage.setItem("profilePictureUrl", data.profilePictureUrl || "");
      })
      .catch((err) => {
        console.error("Profil betöltési hiba:", err);
      });

    return () => {
      ignore = true;
    };
  }, []);

  function getProfileImageUrl(url) {
    if (!url || url.trim() === "") {
      return (
        "https://ui-avatars.com/api/?name=" +
        encodeURIComponent(formData?.name || "User")
      );
    }

    if (url.startsWith("http")) {
      return url;
    }

    const apiOrigin =
      import.meta.env.VITE_API_ORIGIN || "http://localhost:8080";

    return `${apiOrigin}${url.startsWith("/") ? "" : "/"}${url}`;
  }

  function updateField(name, value) {
    setFormData((prev) => ({
      ...prev,
      [name]:
        name === "name" || name === "email"
          ? value
          : Number(value),
    }));
  }

  function handleAvatarClick() {
    if (!isEditing || isUploadingImage) return;
    fileInputRef.current?.click();
  }

  async function handleProfilePictureUpload(e) {
    const file = e.target.files?.[0];

    if (!file) return;

    try {
      setIsUploadingImage(true);

      const currentUserId = localStorage.getItem("userId") || user?.id;
      const result = await uploadProfilePicture(currentUserId, file);

      const updatedData = {
        ...formData,
        profilePictureUrl: result.profilePictureUrl,
      };

      setFormData(updatedData);
      setUser(updatedData);

      localStorage.setItem("profilePictureUrl", result.profilePictureUrl || "");
    } catch (err) {
      console.error("Profilkép feltöltési hiba:", err);
    } finally {
      setIsUploadingImage(false);
      e.target.value = "";
    }
  }

  async function handleSave(e) {
    e.preventDefault();

    try {
      setIsSaving(true);

      const currentUserId = localStorage.getItem("userId") || user?.id;
      const updatedUser = await updateUser(currentUserId, formData);

      setUser(updatedUser);
      setFormData(updatedUser);
      setIsEditing(false);

      localStorage.setItem("name", updatedUser.name || "Felhasználó");
      localStorage.setItem(
        "profilePictureUrl",
        updatedUser.profilePictureUrl || ""
      );
    } catch (err) {
      console.error("Profil mentési hiba:", err);
    } finally {
      setIsSaving(false);
    }
  }

  if (!user || !formData) {
    return <div className="profile-page">Profil betöltése...</div>;
  }

  const profileImage = getProfileImageUrl(formData.profilePictureUrl);

  return (
    <div className="profile-page">
      <div className="profile-card">
        <div className="profile-header">
          <div
            className={isEditing ? "profile-avatar-wrapper editable" : "profile-avatar-wrapper"}
            onClick={handleAvatarClick}
          >
            <img className="profile-avatar" src={profileImage} alt="Profilkép" />

            {isEditing && (
              <div className="profile-avatar-overlay">
                {isUploadingImage ? "Feltöltés..." : "Kép módosítása"}
              </div>
            )}
          </div>

          <input
            ref={fileInputRef}
            type="file"
            accept="image/png,image/jpeg,image/webp"
            hidden
            onChange={handleProfilePictureUpload}
            disabled={!isEditing || isUploadingImage}
          />

          <h1>{formData.name}</h1>
          <p>{formData.email}</p>

          {!isEditing && (
            <button
              type="button"
              className="profile-edit-btn"
              onClick={() => setIsEditing(true)}
            >
              Profil szerkesztése
            </button>
          )}
        </div>

        <div className="profile-divider" />

        <form className="profile-form" onSubmit={handleSave}>
          <h2>Személyes adatok</h2>

          <div className="profile-form-grid">
            <div className="profile-form-group">
              <label>Név</label>
              <input
                value={formData.name || ""}
                disabled={!isEditing}
                onChange={(e) => updateField("name", e.target.value)}
              />
            </div>

            <div className="profile-form-group">
              <label>Email</label>
              <input
                value={formData.email || ""}
                disabled={!isEditing}
                onChange={(e) => updateField("email", e.target.value)}
              />
            </div>

            <div className="profile-form-group">
              <label>Nem</label>
              <select
                value={formData.gender ?? 0}
                disabled={!isEditing}
                onChange={(e) => updateField("gender", e.target.value)}
              >
                <option value={0}>Férfi</option>
                <option value={1}>Nő</option>
              </select>
            </div>

            <div className="profile-form-group">
              <label>Kor</label>
              <input
                type="number"
                value={formData.age ?? ""}
                disabled={!isEditing}
                onChange={(e) => updateField("age", e.target.value)}
              />
            </div>

            <div className="profile-form-group">
              <label>Magasság (cm)</label>
              <input
                type="number"
                value={formData.heightCm ?? ""}
                disabled={!isEditing}
                onChange={(e) => updateField("heightCm", e.target.value)}
              />
            </div>

            <div className="profile-form-group">
              <label>Súly (kg)</label>
              <input
                type="number"
                value={formData.weightKg ?? ""}
                disabled={!isEditing}
                onChange={(e) => updateField("weightKg", e.target.value)}
              />
            </div>

            <div className="profile-form-group">
              <label>Testzsír %</label>
              <input
                type="number"
                value={formData.bodyfatPercent ?? ""}
                disabled={!isEditing}
                onChange={(e) => updateField("bodyfatPercent", e.target.value)}
              />
            </div>

            <div className="profile-form-group">
              <label>Aktivitási szint (1: nem aktív - 5: nagyon aktív)</label>
              <input
                type="number"
                min={1}
                max={5}
                value={formData.activityLevel ?? ""}
                disabled={!isEditing}
                onChange={(e) => updateField("activityLevel", e.target.value)}
              />
            </div>

            <div className="profile-form-group">
              <label>Cél típusa</label>
              <select
                value={formData.goalType ?? 0}
                disabled={!isEditing}
                onChange={(e) => updateField("goalType", e.target.value)}
              >
                <option value={0}>Szinten tartás</option>
                <option value={1}>Szálkásítás</option>
                <option value={2}>Tömegelés</option>
              </select>
            </div>

            <div className="profile-form-group">
              <label>Cél delta kg</label>
              <input
                type="number"
                value={formData.goalDeltaKg ?? ""}
                disabled={!isEditing}
                onChange={(e) => updateField("goalDeltaKg", e.target.value)}
              />
            </div>

            <div className="profile-form-group">
              <label>Cél időtartam (hét)</label>
              <input
                type="number"
                value={formData.goalTimeWeeks ?? ""}
                disabled={!isEditing}
                onChange={(e) => updateField("goalTimeWeeks", e.target.value)}
              />
            </div>
          </div>

          {isEditing && (
            <div className="profile-actions">
              <button
                type="button"
                className="profile-cancel-btn"
                onClick={() => {
                  setFormData(user);
                  setIsEditing(false);
                }}
              >
                Mégse
              </button>

              <button
                type="submit"
                className="profile-save-btn"
                disabled={isSaving || isUploadingImage}
              >
                {isSaving ? "Mentés..." : "Mentés"}
              </button>
            </div>
          )}
        </form>
      </div>
    </div>
  );
}