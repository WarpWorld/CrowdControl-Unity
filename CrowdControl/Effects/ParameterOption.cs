using System.Text.RegularExpressions;

namespace WarpWorld.CrowdControl {
    [System.Serializable]
    /// <summary>A parameter option</summary>
    public class ParameterOption {
        /// <summary>ID of the Option</summary>
        public string ID { get; private set; }
        /// <summary>Name of the Option</summary>
        public string Name { get; private set; }
        /// <summary>Option's parent id</summary>
        public string ParentID { get; private set; }

        public ParameterOption(string name, string parentID) {
            Regex rgx = new Regex("[^a-z0-9-]");
            string entryID = rgx.Replace(name.ToLower(), "");

            ID = $"{parentID}_{entryID}";
            Name = name;
            ParentID = parentID;
        }
    }
}
