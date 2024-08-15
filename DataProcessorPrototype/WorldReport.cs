﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRchatLogDataModel;

namespace DataProcessorPrototype
{
    public class WorldReport : GraphQLQuarriable, IFromvrChatLogitemJOSN, IFirstLast
    {
        public string worldID ="";
        public string worldName ="";
        public long firstused =long.MaxValue;
        public long lastused =0;
        public int timesused =0;
        LinkedList<string> events= new LinkedList<string>();
        public WorldReport() { }

        public void firstLastUpdate(long time)
        {
            if (firstused > time) 
            {
                firstused = time;
            }
            if (lastused < time) 
            {
                lastused = time;
            }
        }

        public void fromvrChatLogitemJOSN(vrChatLogitemJOSN item)
        {
            worldID = item.worldID;
            worldName = item.roomName;
            firstused = item.time;
            lastused = item.time;
            timesused = 0;
            events.AddFirst(item.EventID);
        }
        public override string GetGraphQLSuffix()
        {
            return "WorldReport";
        }
        public override string ListGraphQLProperties()
        {
            return base.ListGraphQLProperties();
        }
        public override string GetGetField()
        {
            return $"worldID: \"{worldID}\"";
        }
        public override bool IsEmpty()
        {
            return worldID.Equals("");
        }

        public override string GetHashKey()
        {
          return worldID;
        }

        public override string GetTableName()
        {
           return "WorldReport-yqjhtslhmngtjgdb3t5ifhsbza-master";
        }
    }
}