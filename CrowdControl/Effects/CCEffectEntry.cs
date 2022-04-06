using System;

/// <summary>A database entry of a Crowd Control Effect. </summary>
[Serializable]
public class CCEffectEntry
{
    /// <summary>Internal ID for this effect. </summary>
    public uint ID { get; private set; }
    /// <summary>Which class does this effect use? </summary>
    public string ClassName;
    /// <summary>Which class is this parented under? </summary>
    public uint ParentID { get; private set; }

    public CCEffectEntry(uint id, string className, uint parentID = UInt32.MaxValue)
    {
        ID = id;
        ClassName = className;
        ParentID = parentID;
    }
}
