using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace VRchatLogDataModel
{
    public class LogProcessor
    {
        public static string[] ListVrchatLogFiles() 
        {
            string vrchatLogDir = System.IO.Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)) + $"{Path.DirectorySeparatorChar}LocalLow{Path.DirectorySeparatorChar}VRChat{Path.DirectorySeparatorChar}vrchat";
            var files = Directory.GetFiles(vrchatLogDir);

            IEnumerable<string> logfiles =
                from file in files
                where Regex.IsMatch(file, "output_log_.+\\.txt")
                orderby file
                select file;

            return logfiles.ToArray();
        }
        public static vrChatLogItem[] processVRchatLogFile(StreamReader reader) 
        {
            LinkedList<vrChatLogItem> vrChatLogItems = new LinkedList<vrChatLogItem>();
            string currentLogText = "";
            int playerCount = 0;
            vrChatinstance currentInstance = new vrChatinstance();
            while (!reader.EndOfStream)
            {

                var text = reader.ReadLine();

                currentLogText += text;
                if (text.Equals(""))
                {
                    if (!currentLogText.Equals(""))
                    {
                        //Console.WriteLine(currentLogText);
                        vrChatLogItem currentLogitem = new vrChatLogItem(currentLogText);
                        if (currentLogitem.item.Contains("Joining wrld_"))
                        {
                            currentInstance = currentInstance.buildfromlogString(currentLogitem.RawText);
                            playerCount = 0;
                        }
                        string joiningorCreatingRoom = "Joining or Creating Room: ";
                        if (currentLogitem.item.Contains(joiningorCreatingRoom))
                        {

                            string roomName = currentLogitem.item.Substring(joiningorCreatingRoom.Length);
                            currentInstance.roomName = roomName;
                            playerCount = 0;
                        }
                        if (currentLogitem.item.Contains("OnLeftRoom"))
                        {
                            currentInstance = new vrChatinstance();
                            playerCount = 0;
                        }
                        if (currentLogitem.item.Contains("OnPlayerJoined"))
                        {
                            playerCount += 1;
                        }
                        if (currentLogitem.item.Contains("OnPlayerLeft "))
                        {
                            playerCount -= 1;
                        }
                        currentLogitem.instance = currentInstance;
                        currentLogitem.playerCount = playerCount;
                        vrChatLogItems.AddLast(currentLogitem);

                    }
                    currentLogText = "";
                }

            }
        return vrChatLogItems.ToArray();
        }
        public static bool shouldkeep(vrChatLogItem item)
        {
            string[] keepitems = {  " [Behaviour] OnPlayerJoined ",  "[Behaviour] OnPlayerLeft "};
            string Groupid = "grp_8250f46e-7b91-4244-95ec-ebdeb87d9be8";
            if (!item.instance.CreatorID.Contains(Groupid))
            {
                return false;
            }

            foreach (var pattern in keepitems)
            {
                if (item.RawText.Contains(pattern))
                {
                    return true;
                }

            }
            return false;
        }
        public static vrChatLogItem[] FilterToOnlyPlayerJoinOrLeave(vrChatLogItem[] itemsToFilter)
        {
            LinkedList<vrChatLogItem> vrChatLogItemsToKeep = new LinkedList<vrChatLogItem>();
            foreach (var item in itemsToFilter)
            {
                if (shouldkeep(item))
                {
                    vrChatLogItemsToKeep.AddLast(item);
                }
            }
            return vrChatLogItemsToKeep.ToArray();
        }

        public static string CreateMD5(string input)
        {
            //https://stackoverflow.com/questions/11454004/calculate-a-md5-hash-from-a-string
            // Use input string to calculate MD5 hash
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                return Convert.ToHexString(hashBytes).ToLower(); // .NET 5 +

                // Convert the byte array to hexadecimal string prior to .NET 5
                // StringBuilder sb = new System.Text.StringBuilder();
                // for (int i = 0; i < hashBytes.Length; i++)
                // {
                //     sb.Append(hashBytes[i].ToString("X2"));
                // }
                // return sb.ToString();
            }
        }
    }




}
