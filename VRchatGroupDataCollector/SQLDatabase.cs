using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRchatGroupDataCollector.Models;

namespace VRchatGroupDataCollector
{
    internal class SQLDatabase
    {
        static void DatabaseStore(LinkedList<vrChatLogItem> vrChatLogItemsToKeep)
        {
            postgresContext database = new postgresContext();
            var players = database.Players;


            /*
            Player me = new Player();
            me.Name = "magmaoverflow";
            me.IsFormerStaff = false;
            me.IsCurrentStaff = true;
            database.Add(me);
            database.SaveChanges();
            */


            foreach (var item in vrChatLogItemsToKeep)
            {
                Event aEvent = new Event();
                var eventCheck = database.Events.Where(x => x.InstanceId == item.instance.instanceID & item.instance.worldID == x.WorlddataNavigation.VrchatWorldId);
                if (eventCheck.Count() == 0)
                {
                    Console.WriteLine("adding new event");
                    aEvent.InstanceId = item.instance.instanceID;
                    aEvent.Regon = item.instance.region;
                    var worldcheck = database.WorldData.Where(x => x.VrchatWorldId == item.instance.worldID);
                    if (worldcheck.Count() == 0)
                    {
                        Console.WriteLine("adding new world");
                        WorldDatum worldData = new WorldDatum();
                        worldData.VrchatWorldId = item.instance.worldID;
                        worldData.Name = item.instance.roomName;
                        aEvent.WorlddataNavigation = worldData;
                        database.WorldData.Add(worldData);
                    }
                    else
                    {
                        aEvent.WorlddataNavigation = worldcheck.First();
                    }

                    database.Events.Add(aEvent);
                }
                else
                {
                    aEvent = eventCheck.First();
                }

                if (item.item.Contains("OnPlayerJoined") | item.item.Contains("OnPlayerLeft"))
                {
                    string playername = "";
                    bool isJoining = false;
                    if (item.item.Contains("OnPlayerJoined"))
                    {
                        playername = item.item.Replace("OnPlayerJoined", "");
                        isJoining = true;

                    }
                    else
                    {
                        playername = item.item.Replace("OnPlayerLeft", "");
                    }

                    playername = playername.Trim();
                    Player player = new Player();
                    var playercheck = database.Players.Where(x => x.Name == playername);
                    if (playercheck.Count() == 0)
                    {
                        Console.WriteLine("adding new player  " + playername);
                        player.Name = playername;
                        database.Players.Add(player);
                    }
                    else
                    {
                        player = playercheck.First();
                    }

                    PlayerActivity playerActivity = new PlayerActivity();
                    var time = item.time.ToUniversalTime();

                    playerActivity.Player = player;
                    playerActivity.Event = aEvent;
                    playerActivity.Time = time;
                    playerActivity.Joined = isJoining;
                    playerActivity.TotalPlayerCount = item.playerCount;

                    var historycheck = database.PlayerActivities.Where(x => x.Event == aEvent & x.Player == player & x.Joined == isJoining);
                    bool shouldAdd = true;
                    foreach (var historyitem in historycheck)
                    {
                        var timeDiffrence = historyitem.Time - playerActivity.Time;
                        if (timeDiffrence.TotalMicroseconds < 30000)
                        {
                            shouldAdd = false;
                        }
                    }
                    if (shouldAdd)
                    {
                        database.PlayerActivities.Add(playerActivity);
                        Console.WriteLine("adding PlayerJoined event");
                        Console.WriteLine(item.RawText);
                    }
                }

                database.SaveChanges();
            }
        }
    }
}
