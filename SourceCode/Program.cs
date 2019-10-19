using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Telegram.Bot;

namespace Monopoly_tgbot
{
    partial class Program
    {
        static TelegramBotClient Client;

        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            const string token = "928805208:AAFqHquYSpuNQxCj7RORd7TzGyTXpsHm44E";

            Client = new TelegramBotClient(token);

            Telegram.Bot.Types.User Banker = Client.GetMeAsync().Result;

            Client.OnMessage += MessageHandlerAsync;

            Client.StartReceiving();

            #region фиг знает, куда ставить эти штуки с вин формс и че они делают
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
            #endregion
        }
    }
}
