using System;
using System.Collections.Generic;
using Telegram.Bot;
using Telegram.Bot.Args;

namespace Monopoly_tgbot
{
    partial class Program
    {
        static private async void MessageHandlerAsync(object sender, MessageEventArgs args)
        {
            if (sender is TelegramBotClient && args.Message.Text != null)
            {
                await Client.SendTextMessageAsync(args.Message.Chat.Id, "hey b0ss");
                //тут обработка сообщений
            }
        }
    }
}
