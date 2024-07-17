using System.Text.RegularExpressions;

namespace VRchatLogDataModel
{
    public class vrChatinstance
    {

        public string worldID;
        public int instanceID;
        public string instanceType;
        public string CreatorID { get; private set; }
        public string GroupAccessType { get; private set; }
        public string region;
        public string roomName;

        public vrChatinstance()
        {
            this.worldID = "";
            this.instanceID = 0;
            this.instanceType = "";
            this.CreatorID = "";
            this.GroupAccessType = "";
            this.region = "";
            this.roomName = "";
        }
            public vrChatinstance buildfromlogString(string item)
        {
             string pattern = "(\\d\\d\\d\\d)\\.(\\d\\d)\\.(\\d\\d)([^\\d]+)(\\d+):(\\d+):(\\d+) Log        -  \\[Behaviour\\] Joining (wrld_[\\d\\w-]+):(\\d+)~(\\w+)\\((\\w\\w\\w_[\\d\\w-]+)\\)(~[\\w\\(\\)]+)?~region\\((\\w+)\\)";

            var PatternMacth = Regex.Match(item, pattern);
            if (PatternMacth.Success)
            {
                var logParts = PatternMacth.Groups;
                this.worldID= logParts[8].Value;
                this.instanceID= int.Parse( logParts[9].Value);
                this.instanceType = logParts[10].Value;
                this.CreatorID = logParts[11].Value;
                this.GroupAccessType= logParts[12].Value;
                this.region = logParts[13].Value;
            }

            return this;
        }
        public override string ToString()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }
    }
}