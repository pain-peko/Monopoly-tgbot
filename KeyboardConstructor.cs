using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;

namespace Monopoly_tgbot
{
    public class KeyboardConstructor
    {
        public static ReplyKeyboardMarkup Keyboard()
        {
            var rmu = new ReplyKeyboardMarkup();
            rmu.Keyboard = new KeyboardButton[][]
            {
                new KeyboardButton[]
                {
                    new KeyboardButton("Вперед"),
                },new KeyboardButton[]
                {
                    new KeyboardButton("Баланс"),
                }
            };
            return rmu;
        }
    }
}
