namespace WarpWorld.CrowdControl
{
    [System.Serializable]
    public class ParameterOption
    {
        public uint ID { get; private set; }
        public string Name { get; private set; }
        public uint ParentID { get; private set; }

        public ParameterOption(string name, uint parentID)
        {
            ID = Utils.ComputeMd5Hash(name + parentID);
            Name = name;
            ParentID = parentID;
        }
    }
}
