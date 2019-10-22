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
                        File.WriteAllText(usersPath, JsonConvert.SerializeObject(GamerList));

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

                        if (args.Message.Text[0] == '+' || args.Message.Text[0] == '-')
                        {
                            Stonks(args.Message.Text, Me, args);
                        }
                        else if (IsSendMoneyRequest(args.Message.Text, GamerList))
                        {
                            SendMoneyRequest(args.Message.Text, Me, GamerList, args);
                        }
                        else if (args.Message.Text == "Баланс")
                        {
                            await Client.SendTextMessageAsync(args.Message.Chat.Id, $"Ваш баланс: {Me.money}M", ParseMode.Default, false, false, 0, KeyboardConstructor.Keyboard());
                        }
                        else if (args.Message.Text == "Вперед")
                        {
                            Me.PayMe(2);
                            await Client.SendTextMessageAsync(args.Message.Chat.Id, $"Ваш баланс: {Me.money}M", ParseMode.Default, false, false, 0, KeyboardConstructor.Keyboard());
                        }
                        else
                        {
                            await Client.SendTextMessageAsync(args.Message.Chat.Id, "Неверный ввод", ParseMode.Default, false, false, 0, KeyboardConstructor.Keyboard());
                        }
                    }
                    else
                    {
                        await Client.SendTextMessageAsync(args.Message.Chat.Id, "Ты не в игре лол", ParseMode.Default, false, false, 0, KeyboardConstructor.Keyboard());
                    }
                    File.WriteAllText(usersPath, JsonConvert.SerializeObject(GamerList));
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
            for (int i = 0; i < list.Count(); i++)
                if (list[i].userName == text[0])
                    return true;
            return false;
        }
        private bool IsPlayerExist (long id, List<Gamer> list)
        {
            for (int i = 0; i < list.Count(); i++)
                if (list[i].ID == id)
                    return true;
            return false;
        }
        private async void SendMoneyRequest (string text, Gamer me, List<Gamer> list, MessageEventArgs args)
        {
            var user = GetGamer(text[0], list);
            text = text.Remove(0, 1);
            text = text.Trim();
            try
            {
                float tempMoney = Convert.ToSingle(text);
                me.PayTo(tempMoney, user);
            }
            catch(FormatException)
            {
                await Client.SendTextMessageAsync(args.Message.Chat.Id, "Неверный ввод", ParseMode.Default, false, false, 0, KeyboardConstructor.Keyboard());
            }
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

        private void Reset_Click(object sender, EventArgs e)
        {
            var list = JsonConvert.DeserializeObject<List<Gamer>>(File.ReadAllText(usersPath));
            foreach (Gamer g in list)
            {
                g.Reset();
            }
            File.WriteAllText(usersPath,JsonConvert.SerializeObject(list));
            AddText("All users have been reseted");
        }
    }
}
