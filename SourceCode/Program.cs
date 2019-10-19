using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Telegram.Bot;

namespace Monopoly_tgbot
{
    partial class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
