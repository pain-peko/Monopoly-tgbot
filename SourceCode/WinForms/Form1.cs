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

namespace Monopoly_tgbot
{
    public partial class Form1 : Form
    {
        public static TelegramBotClient Client;
        const string token = "928805208:AAFqHquYSpuNQxCj7RORd7TzGyTXpsHm44E";       
        public Form1()
        {
            InitializeComponent();
            Client = new TelegramBotClient(token);
            Client.OnMessage += MessageHandlerAsync;
        }


        static private async void MessageHandlerAsync(object sender, MessageEventArgs args)
        {
            if (sender is TelegramBotClient && args.Message.Text != null)
            {
                var GamerList = new List<Gamer>(); //временно
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
                    else
                    {
                        await Client.SendTextMessageAsync(args.Message.Chat.Id, "Неверный ввод");
                    }
                }
                else
                {
                    await Client.SendTextMessageAsync(args.Message.Chat.Id, "Ты не в игре лол");
                }
            }
        }
        #region MessageHandler Funcs
        static private async void Stonks (string text, Gamer me, MessageEventArgs args)
        {
            bool addMoney;
            if (text[0] == '+')
                addMoney = true;
            else if (text[0] == '-')
                addMoney = false;
            else
                throw new ArgumentException("Непонятно, какое действие с деньгами проводить");

            text.Remove(0, 1);

            try
            {
                float tempMoney = Convert.ToSingle(text);
                if (addMoney)
                    me.PayMe(tempMoney);
                else if (!addMoney)
                    me.PayTo(tempMoney);
            }
            catch (FormatException)
            {
                await Client.SendTextMessageAsync(args.Message.Chat.Id, "Неверный ввод");
            }
        }
        static private bool IsSendMoneyRequest (string text, List<Gamer> list)
        {
            for (int i = 0; i < list.Count(); i++)
                if (list[i].userName == text[0])
                    return true;
            return false;
        }
        static private bool IsPlayerExist (long id, List<Gamer> list)
        {
            for (int i = 0; i < list.Count(); i++)
                if (list[i].ID == id)
                    return true;
            return false;
        }
        static private async void SendMoneyRequest (string text, Gamer me, List<Gamer> list, MessageEventArgs args)
        {
            var user = GetGamer(text[0], list);
            text.Remove(0, 1);
            if (text[0] == ' ')
                text.Remove(0, 1);
            try
            {
                float tempMoney = Convert.ToSingle(text);
                me.PayTo(tempMoney, user);
            }
            catch(FormatException)
            {
                await Client.SendTextMessageAsync(args.Message.Chat.Id, "Неверный ввод");
            }
        }
        static private Gamer GetGamer (char userName, List<Gamer> list)
        {
            for (int i = 0; i < list.Count(); i++)
            {
                if (list[i].userName == userName)
                    return list[i];
            }
            throw new ArgumentException("Сюда код не должен доходить, чини");
        }
        static private Gamer GetGamer(long id, List<Gamer> list)
        {
            for (int i = 0; i < list.Count(); i++)
            {
                if (list[i].ID == id)
                    return list[i];
            }
            throw new ArgumentException("Сюда код не должен доходить, чини");
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
    }
}
