using System.Text.RegularExpressions;

namespace WarpWorld.CrowdControl {
    [System.Serializable]
    public class ParameterOption {
        public string ID { get; private set; }
        public string Name { get; private set; }
        public string ParentID { get; private set; }

        public ParameterOption(string name, string parentID) {
            Regex rgx = new Regex("[^a-z0-9-]");
            string entryKey = rgx.Replace(name.ToLower(), "");

            ID = $"{parentID}_{entryKey}";
            Name = name;
            ParentID = parentID;
        }
    }
}
