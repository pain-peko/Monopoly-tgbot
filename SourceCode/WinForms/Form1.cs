using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Telegram.Bot;
using Telegram.Bot.Args;
using Newtonsoft.Json;
using System.IO;
using Telegram.Bot.Types.Enums;

namespace Monopoly_tgbot
{
    public partial class Form1 : Form
    {
        public static TelegramBotClient Client;

        public Random rand = new Random();

        public static string usersPath = "Files/Users.json";

        public const string SecretPassword = "Альфа Влад";

        const string token = "928805208:AAFqHquYSpuNQxCj7RORd7TzGyTXpsHm44E";

        List<string> CommandNames = new List<string>();
        
        public Form1()
        {
            CommandNames.Add("/add_me");

            InitializeComponent();
            Client = new TelegramBotClient(token);
            Client.OnMessage += MessageHandlerAsync;
            Client.OnMessage += CommandHandlerAsync;
        }

        private async void CommandHandlerAsync(object sender, MessageEventArgs args)
        {
            if (sender is TelegramBotClient && args.Message.Text != null)
            {
                var CommandsList = new Commands();

                if (args.Message.Text == CommandNames[0])
                {
                    if (!CommandsList.ContainsActivatedCommand(CommandNames[0], args))
                    {
                        CommandsList.ActivatedCommandsList.Add(new ActivatedCommand($"{CommandNames[0]}", args));
                        await Client.SendTextMessageAsync(args.Message.Chat.Id, "Напиши секретный пароль");
                    }
                    else
                        await Client.SendTextMessageAsync(args.Message.Chat.Id, "Не повторяй 2 раза одну и ту же команду");
                }
                else if (CommandsList.ContainsActivatedCommand(CommandNames[0], args))
                {
                    if (args.Message.Text == SecretPassword)
                    {
                        await Client.SendTextMessageAsync(args.Message.Chat.Id, "Введи свой никнейм, состоящий из 1 буквы");
                        CommandsList.ActivatedCommandsList.Add(new ActivatedCommand($"Add UserName", args));
                        CommandsList.RemoveCommand(CommandNames[0], args);
                    }
                    else
                    {
                        CommandsList.RemoveCommand(CommandNames[0], args);
                        await Client.SendTextMessageAsync(args.Message.Chat.Id, "Неверно введен пароль, команда была отменена");
                    }
                }
                else if (CommandsList.ContainsActivatedCommand("Add UserName", args))
                {
                    if (args.Message.Text.Length < 2)
                    {
                        var GamerList = JsonConvert.DeserializeObject<List<Gamer>>(File.ReadAllText(usersPath));
                        GamerList.Add(new Gamer(args.Message.Chat.Id, args.Message.Text[0]));
                        File.WriteAllText(usersPath, JsonConvert.SerializeObject(GamerList,Formatting.Indented));

                        CommandsList.RemoveCommand("Add UserName", args);
                    }
                    else
                    {
                        await Client.SendTextMessageAsync(args.Message.Chat.Id, "Должен быть только один символ, попробуйте еще раз");
                    }
                }
                CommandsList.SaveCommands();
            }
        }
        private async void MessageHandlerAsync(object sender, MessageEventArgs args)
        {
            if (sender is TelegramBotClient && args.Message.Text != null)
            {
                var CommandsList = new Commands();

                if (!CommandsList.ContainsActivatedCommand(args) && args.Message.Text[0] != '/')
                {
                    AddText($"Попытка взаимодействия пользователя {args.Message.Chat.FirstName} {args.Message.Chat.LastName} ({args.Message.Chat.Id} - {args.Message.Chat.Username})");

                    var GamerList = JsonConvert.DeserializeObject<List<Gamer>>(File.ReadAllText(usersPath));
                    if (IsPlayerExist(args.Message.Chat.Id, GamerList))
                    {
                        var Me = GetGamer(args.Message.Chat.Id, GamerList);

                        if (IsSendMoneyRequest(args.Message.Text, GamerList))
                        {
                            SendMoneyRequest(args.Message.Text, Me, GamerList, args);
                        }
                        else if (IsHouseRequest(args.Message.Text, Me, args))
                        {
                            string[] cities = GetGoodRequest(GetBuildHouseRequest(args.Message.Text), Me);
                            if (args.Message.Text[0] == '+')
                            {
                                for (int i = 0; i < cities.Length; i++)
                                    Me.BuildHouse(Me.properties.Find(item => item.name == cities[i]));
                            }
                            else
                            {
                                for (int i = 0; i < cities.Length; i++)
                                    Me.DemolishHouse(Me.properties.Find(item => item.name == cities[i]));
                            }
                        }
                        else if (IsPayRentOrBuyCityRequest(args.Message.Text))
                        {
                            if (IsPayRentRequest(args.Message.Text, GamerList))
                                Me.PayRent(GetPayRentProp(args.Message.Text, GamerList),GamerList);
                            else
                                Me.Buy(new Property(args.Message.Text));
                        }
                        else if (IsSendPropertyRequest(args.Message.Text, Me))
                        {
                            var tempGamer = FindGamer(args.Message.Text[0], GamerList);
                            Me.SendPropertyTo(GetProperty(args.Message.Text, Me), tempGamer);
                        }
                        else if(args.Message.Text[0] == '+' || args.Message.Text[0] == '-')
                        {
                            Stonks(args.Message.Text, Me, args);
                        }
                        else if (args.Message.Text == "Вперед")
                        {
                            Me.PayMe(2);
                        }
                        else if(args.Message.Text == "Баланс")
                        {
                            string ans = $"Ваш баланс: {Me.money.ToString("0.000")}M\n";
                            List<float> balance = new List<float>();
                            foreach(Gamer g in GamerList)
                            {
                                balance.Add(g.money);
                            }
                            balance = Shuffle(balance);
                            foreach (float f in balance)
                                ans += $"{RandomString()}: {f.ToString("0.000")}M\n";
                            await Client.SendTextMessageAsync(args.Message.Chat.Id, ans, ParseMode.Default, false, false, 0, KeyboardConstructor.Keyboard());

                        }
                        else
                        {
                            await Client.SendTextMessageAsync(args.Message.Chat.Id, "Неверный ввод", ParseMode.Default, false, false, 0, KeyboardConstructor.Keyboard());
                        }
                        await Client.SendTextMessageAsync(args.Message.Chat.Id, $"Ваш баланс: {Me.money.ToString("0.000")}M", ParseMode.Default, false, false, 0, KeyboardConstructor.Keyboard());
                    }
                    else
                    {
                        await Client.SendTextMessageAsync(args.Message.Chat.Id, "Ты не в игре лол", ParseMode.Default, false, false, 0, KeyboardConstructor.Keyboard());
                    }
                    File.WriteAllText(usersPath, JsonConvert.SerializeObject(GamerList,Formatting.Indented));
                }
            }
        }
        #region MessageHandler Funcs
        private async void Stonks (string text, Gamer me, MessageEventArgs args)
        {
            bool addMoney;
            if (text[0] == '+')
                addMoney = true;
            else if (text[0] == '-')
                addMoney = false;
            else
                throw new ArgumentException("Непонятно, какое действие с деньгами проводить");

            try
            {
                float tempMoney = Convert.ToSingle(text.Remove(0, 1));
                if (addMoney)
                    me.PayMe(tempMoney);
                else
                    me.PayTo(tempMoney);
            }
            catch (FormatException)
            {
                await Client.SendTextMessageAsync(args.Message.Chat.Id, "Неверный ввод", ParseMode.Default, false, false, 0, KeyboardConstructor.Keyboard());
            }
        }


        private bool IsSendMoneyRequest (string text, List<Gamer> list)
        {
            try
            {
                var tempGamer = FindGamer(text[0], list);
                text = text.Remove(0, 1);
                text = text.Trim();
                float tempMoney = Convert.ToSingle(text);
                return true;
            }
            catch (ArgumentException)
            {
                return false;
            }
            catch (FormatException)
            {
                return false;
            }
        }
        private async void SendMoneyRequest(string text, Gamer me, List<Gamer> list, MessageEventArgs args)
        {
            var user = GetGamer(text[0], list);
            text = text.Remove(0, 1);
            text = text.Trim();
            try
            {
                float tempMoney = Convert.ToSingle(text);
                me.PayTo(tempMoney, user);
            }
            catch (FormatException)
            {
                await Client.SendTextMessageAsync(args.Message.Chat.Id, "Неверный ввод", ParseMode.Default, false, false, 0, KeyboardConstructor.Keyboard());
            }
        }


        private bool IsPlayerExist (long id, List<Gamer> list)
        {
            for (int i = 0; i < list.Count(); i++)
                if (list[i].ID == id)
                    return true;
            return false;
        }


        private bool IsSendPropertyRequest(string text, Gamer me)
        {
            if (text.Length < 3)
                return false;

            text = text.Remove(0, 1);
            text = text.Trim();
            for (int i = 0; i < me.properties.Count(); i++)
            {
                if (text == me.properties[i].name)
                    return true;
            }
            return false;
        }
        private Gamer FindGamer(char userName, List<Gamer> list)
        {
            for (int i = 0; i < list.Count(); i++)
                if (list[i].userName == userName)
                    return list[i];
            throw new ArgumentException("Игрока не существует");
        }
        private Property GetProperty(string text, Gamer me)
        {
            text = text.Remove(0, 1);
            text = text.Trim();
            for (int i = 0; i < me.properties.Count(); i++)
            {
                if (text == me.properties[i].name)
                    return me.properties[i];
            }
            throw new ArgumentException("Сюда код не должен доходить");
        }


        private bool IsHouseRequest(string text, Gamer me, MessageEventArgs args)
        {
            if (text.Length < 2 || ((text[0] != '+' && text[0] != '-') || text[1] != ' ') || text.Length < 3)
                return false;
            string[] cities = GetBuildHouseRequest(text);

            string errorRequests = "";
            bool yep = false;
            for (int i = 0; i < cities.Length; i++)
            {
                bool goodRequest = false;
                for (int j = 0; j < me.properties.Count(); j++)
                { 
                    if (me.properties[j].name == cities[i])
                    {
                        yep = true;
                        goodRequest = true;
                    }
                }
                if (!goodRequest)
                    errorRequests += cities[i] + ", ";
            }
            if (errorRequests != "" && yep)
                Client.SendTextMessageAsync(args.Message.Chat.Id, $"Это запрос неверен: {errorRequests}");
            if (yep)
                return true;
            else
                return false;
        }
        private string[] GetBuildHouseRequest(string text)
        {
            text = text.Remove(0, 2);

            char[] separator = new char[2];
            separator[0] = ',';
            separator[1] = ' ';

            return text.Split(separator);
        }
        private string[] GetGoodRequest (string[] request, Gamer me)
        {
            List<string> goodRequests = new List<string>();
            for (int i = 0; i < request.Length; i++)
            {
                for (int j = 0; j < me.properties.Count(); j++)
                {
                    if (me.properties[j].name == request[i])
                    {
                        goodRequests.Add(request[i]);
                    }
                }
            }
            return goodRequests.ToArray();
        }


        private bool IsPayRentOrBuyCityRequest(string text)
        {
            List<PropSave> allProps = new List<PropSave>();
            allProps = JsonConvert.DeserializeObject<List<PropSave>>(File.ReadAllText("Files/Props.json"));
            for (int i = 0; i < allProps.Count; i++)
            {
                if (allProps[i].name == text)
                    return true;
            }
            return false;
        }
        private bool IsPayRentRequest(string text, List<Gamer> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                for (int j = 0; j < list[i].properties.Count; j++)
                {
                    if (list[i].properties[j].name == text)
                        return true;
                }
            }
            return false;
        }
        private Property GetPayRentProp(string text, List<Gamer> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                for (int j = 0; j < list[i].properties.Count; j++)
                {
                    if (list[i].properties[j].name == text)
                        return list[i].properties[j];
                }
            }
            throw new ArgumentException("лол, как сюда код дошел");
        }


        private Gamer GetGamer (char userName, List<Gamer> list)
        {
            return list.Find(item => item.userName == userName);
        }
        private Gamer GetGamer(long id, List<Gamer> list)
        {
            return list.Find(item => item.ID == id);
        }
        #endregion




        //Button click handle
        private void StartStop_Click(object sender, EventArgs e)
        {
            if (Client.IsReceiving)
                Stop();
            else
                Start();
        }

        void Stop()
        { 
            Client.StopReceiving();
            StartStop.Text = "Start";
            AddText("Bot stopped");
        }
        void Start()
        {
            Client.StartReceiving();
            StartStop.Text = "Stop";
            AddText("Bot started");
        }

        delegate void SetTextCallback(string text);

        private void AddText(string text)
        {
            if (this.Log.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(AddText);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.Log.Text += text+"\n";
            }
        }
        public  List<float> Shuffle(List<float> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rand.Next(n + 1);
                float value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
            return list;
        }
        public string RandomString()
        {
            string hash = string.Empty;
            
            for (int i = 0; i < 7; i++)
                hash += (char)rand.Next(36, 122);
            return hash;
        }
        private void Reset_Click(object sender, EventArgs e)
        {
            var list = JsonConvert.DeserializeObject<List<Gamer>>(File.ReadAllText(usersPath));
            foreach (Gamer g in list)
            {
                g.Reset();
            }
            File.WriteAllText(usersPath,JsonConvert.SerializeObject(list,Formatting.Indented));
            AddText("All users have been reseted");
        }
    }
}
