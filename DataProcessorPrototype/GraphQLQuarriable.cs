using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DataProcessorPrototype
{    

    public abstract class GraphQLQuarriable
    {
        private bool IsInDatabase=false;
        public int _version = 0;
        public virtual string GetGraphQLSuffix() 
        {
            return "";
        }
        public virtual string GetCreateQuarry()
        {
            return
                $"mutation Create{GetGraphQLSuffix()} {{\r\n" +
                $"    create{GetGraphQLSuffix()}(input: {GetGraphQLData()}) {{\r\n" +
                ListGraphQLProperties()+
                $"    }}\r\n}}";
        }

        public string GetGraphQLData()
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            //GraphQL.Client.Serializer.Newtonsoft
            string json = JsonConvert.SerializeObject(this);
            string pattern = "\"([^: ,]+)\":([^: ,]+)";
            var matchs=Regex.Matches(json,pattern);
            foreach (Match match in matchs) 
            {
                
            }
            json=Regex.Replace(json,pattern,"$1:$2");
            return json;
        }
        public virtual string ListGraphQLProperties()
        {
            var items= this.GetType().GetFields();
            string output = "";
            foreach (var item in items) 
            {
                var temp = item.ToString();
                temp = temp.Split(" ")[1];
                if (!temp.Equals("IsInDatabase")) 
                {
                    output += " \n" + temp;
                }
                
            }
            return output;
        }

        public virtual string GetUpdateQuarry()
        {
            return
                $"mutation Update{GetGraphQLSuffix()} {{\r\n" +
                $"    update{GetGraphQLSuffix()}(input: {GetGraphQLData()}) {{\r\n" +
                ListGraphQLProperties() +
                $"    }}\r\n}}";
        }

        public virtual string GetGetField() 
        {
            return ("");
        }
        public virtual string GetGetQuarry()
        {
            return GetGetQuarry(GetGetField());
        }

        public virtual string GetGetQuarry(string quarry)
        {
            return
                $"query Get{GetGraphQLSuffix()} {{\r\n" +
                $"    get{GetGraphQLSuffix()}({quarry}) {{\r\n" +
                ListGraphQLProperties() +
                $"    }}\r\n}}";
        }

        public virtual bool IsEmpty() 
        {
            return true;
        }
        public virtual bool GetIsInDatabase()
        {
            return IsInDatabase;
        }
        public virtual void SetIsInDatabase(bool value)
        {
             IsInDatabase = value;
        }

        public abstract string GetTableName();

        public abstract string GetHashKey();
    }
}
