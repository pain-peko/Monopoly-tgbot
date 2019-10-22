using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Monopoly_tgbot
{
    [Serializable]
    public class Gamer
    {
        public long ID;
        public char userName;
        public float money;
        public List<Property> properties;
        public Gamer() { }
        public Gamer (long id,char name)
        {
            ID = id;
            userName = name;
            money = 15;
            properties = new List<Property>();
        }
        public void PayTo(float amount,Gamer g = null)
        {
            if (money >= amount)
            {
                money -= amount;
                if (g != null)
                    g.money += amount;
            }
            else
            {
                NotEnoughMoney();
            }
        }
        public void PayMe(float amount)
        {
            money += amount;
        }
        public void Buy(Property prop)
        {
            PayTo(prop.cost);
            prop.ownerID = ID;
            properties.Add(prop);
        }
        public void PayRent(Property prop)
        {
            PayTo(prop.tiersCost[prop.Tier], GetGamerByID(prop.ownerID));
        }
        public void BuildHouse(Property prop)
        {
            if (prop.tag == "Transport" || prop.tag == "Electricity")
            {
                return;
            }
            if (prop.ownerID == ID)
            {
                PayTo(prop.HouseCost);
                prop.Tier += 1;
            }
        }
        public void DemolishHouse(Property prop)
        {

            if (prop.Tier > 0 && prop.ownerID == ID)
            {
                PayMe(prop.HouseCost / 2f);
                prop.Tier -= 1;
            }
        }
        public void SendPropertyTo(Property prop, Gamer gamer)
        {
            if (prop.ownerID == ID)
            {
                properties.Remove(prop);
                prop.ownerID = gamer.ID;
                gamer.properties.Add(prop);
            }
        }
        public async void NotEnoughMoney()
        {
            await Form1.Client.SendTextMessageAsync(ID, "Нужно больше золота");
        }
        public void Reset()
        {
            money = 15f;
            properties = new List<Property>();
        }
        public static Gamer GetGamerByID(long ID)
        {
            var tmp = JsonConvert.DeserializeObject<List<Gamer>>(File.ReadAllText(Form1.usersPath));
            return tmp.Find(item => item.ID == ID);
        }
    }
}
