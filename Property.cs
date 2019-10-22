using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monopoly_tgbot
{
    [Serializable]
    public class Property
    {
        public long ownerID;
        public string name;
        public float cost;
        public string tag;
        private int tier;
        public int Tier
        { get
            {
                
                if(tag == "Transport" || tag == "Electricity")
                {
                    tier = -1;
                    foreach (var prp in Gamer.GetGamerByID(ownerID).properties)
                    {
                        if (prp.tag == tag)
                            tier++;
                    }
                }
                return tier;
            }
            set
            {
                if (tag != "Transport" && tag != "Electricity")
                    tier = value;
            }
        }
        public float[] tiersCost;
        public float HouseCost
        {
            get
            {
                if(tag == "Brown"|| tag == "Cyan")
                {
                    return 0.5f;
                }
                else if (tag == "Magenta" || tag == "Orange")
                {
                    return 1f;
                }
                else if (tag == "Red" || tag == "Yellow")
                {
                    return 1.5f;
                }
                else if (tag == "Green" || tag == "Blue")
                {
                    return 2f;
                }
                else
                {
                    return 0f;
                }
            }
        }
        public Property() { }
        public Property(string propName)
        {
            List<PropSave> allProps = new List<PropSave>();
            allProps = JsonConvert.DeserializeObject<List<PropSave>>(File.ReadAllText("Files/Props.json"));
            var ps = allProps.Find(item => item.name == propName);
            name = ps.name;
            cost = ps.cost;
            tag = ps.tag;
            tiersCost = ps.tiersCost;
            Tier = 0;
        }
    }
    [Serializable]
    public class PropSave
    {
        public string name;
        public float cost;
        public string tag;
        public float[] tiersCost;
    }

}
