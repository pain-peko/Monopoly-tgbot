﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monopoly_tgbot
{
    [Serializable]
    public class Gamer
    {
        public long ID;
        public char userName;
        public float money;
        public List<Property> properties;
        public string Money { get { return "Баланс: " + money.ToString("0.000"); } }
        public Gamer(long id,char name)
        {
            ID = id;
            userName = name;
            money = 15;
            properties = new List<Property>();
        }
        public void PayTo(float amount,Gamer g = null)
        {
            if (money > amount)
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
            prop.owner = this;
            properties.Add(prop);
        }
        public void PayRent(Property prop)
        {
            PayTo(prop.tiersCost[prop.Tier], prop.owner);
        }

        public void BuildHouse(Property prop)
        {
            PayTo(prop.HouseCost);
            prop.Tier += 1;
        }
        public async void NotEnoughMoney()
        {
            await Form1.Client.SendTextMessageAsync(ID, "Нужно больше золота");
        }
    }
}
