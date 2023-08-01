using UnityEngine;
using System.Collections;

namespace WarpWorld.CrowdControl {
    /// <summary>
    /// Information about a stream user profile.
    /// </summary> 
    public class StreamUser {
        public string name;

        /// <summary>URL to download the profile icon from.</summary>
        public string profileIconUrl;
        /// <summary>Profile icon downloaded into a 2D texture. Can be <see langword="null"/>.</summary>
        public Sprite profileIcon;
        /// <summary>Roles of the user</summary>
        public string[] roles = new string[0];
        /// <summary>Subscriptions the user has</summary>
        public string[] subscriptions = new string[0];
        /// <summary>Coins Spent</summary>
        public uint coinsSpent;

        public string displayName;
        public string originSite = "";
        public string email = "";
        public string originID = "";

        public StreamUser(JSONUserInfo.JSONUserInfoProfile user) {
            originSite = user.m_type;
            name = user.m_name;
            displayName = user.m_originData.m_user.m_display_name;
            email = user.m_originData.m_user.m_email;
            profileIconUrl = user.m_image;
            roles = user.m_roles;
            subscriptions = user.m_subscriptions;
            originID = user.m_originID;
        }

        public StreamUser(JSONEffectRequest.JSONUser user) {
            name = user.m_name;
            profileIconUrl = user.m_image;
            roles = user.m_roles;
        }

        public StreamUser(string viewerName, Sprite icon) {
            name = viewerName;
            profileIcon = icon;
        }

        public IEnumerator DownloadSprite() {
            if (string.IsNullOrEmpty(profileIconUrl))
                yield break;

            WWW www = new WWW(profileIconUrl);

            yield return www;

            if (string.IsNullOrEmpty(www.error))
                profileIcon = Sprite.Create(www.texture, new Rect(0, 0, www.texture.width, www.texture.height), Vector2.zero);

            yield return null;
        }
    }
}
