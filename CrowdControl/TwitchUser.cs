using UnityEngine;

namespace WarpWorld.CrowdControl
{
    // TODO share with other WarpWorld components? WarpWorld.Twitch maybe?
    /// <summary>
    /// Information about a Twitch user profile.
    /// </summary> 
    public class TwitchUser
    {
        /// <summary>Unique Twitch user identifier.</summary>
        public ulong id;
        /// <summary>Unique Twitch user name. Always lowercase.</summary>
        public string name;
        /// <summary>Pretty printed user name.</summary>
        public string displayName;
        /// <summary>URL to download the profile icon from.</summary>
        public string profileIconUrl;
        /// <summary>Profile icon downloaded into a 2D texture. Can be <see langword="null"/>.</summary>
        public Sprite profileIcon;
        /// <summary>Color to tint the profile icon's background with.</summary>
        public Color profileIconColor = Color.white;
    }
}
