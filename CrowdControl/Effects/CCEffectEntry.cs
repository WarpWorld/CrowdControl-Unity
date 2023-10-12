using System;

[Serializable]
/// <summary>A database entry of a Crowd Control Effect. </summary>
public class CCEffectEntry {
    /// <summary>Internal id for this effect. </summary>
    public string ID { get; private set; }
    /// <summary>Effect's unique parent ID </summary>
    public string ParentID { get; private set; }

    public CCEffectEntry(string id, string parentID = "") {
        ID = id;
        ParentID = parentID;
    }
}
