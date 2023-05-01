using System;

/// <summary>A database entry of a Crowd Control Effect. </summary>
[Serializable]
public class CCEffectEntry {
    /// <summary>Internal ID for this effect. </summary>
    public string ID { get; private set; }
    /// <summary>Which class is this parented under? </summary>
    public string ParentID { get; private set; }

    public CCEffectEntry(string id, string parentID = "") {
        ID = id;
        ParentID = parentID;
    }
}
