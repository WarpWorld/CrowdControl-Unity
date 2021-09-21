using System;

/// <summary>A database entry of a Crowd Control Effect. </summary>
[Serializable]
public class CCEffectEntry
{
    /// <summary>Internal ID for this effect. </summary>
    public uint id;
    /// <summary>Which class does this effect use? </summary>
    public string className;
}