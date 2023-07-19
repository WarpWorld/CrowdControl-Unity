using UnityEngine;
using System.Collections;

namespace WarpWorld.CrowdControl {
    /// <summary>
    /// Information about a stream user profile.
    /// </summary> 
    public class StreamViewer {
        /// <summary>Unique Twitch user name. Always lowercase.</summary>
        public string name;
        /// <summary>URL to download the profile icon from.</summary>
        public string profileIconUrl;
        /// <summary>Profile icon downloaded into a 2D texture. Can be <see langword="null"/>.</summary>
        public Sprite profileIcon;
        /// <summary>Roles of the user</summary>
        public string [] roles;
        /// <summary>Coins Spent</summary>
        public uint coinsSpent;

        public StreamViewer(JSONEffectRequest.JSONUser user) {
            name = user.m_name;
            profileIconUrl = user.m_image;
            roles = user.m_roles;
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
