using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Telegram.Bot.Args;

namespace Monopoly_tgbot
{
    abstract class Command
    {
        public string Name { get; private set; }
        public bool IsActivated { get; protected set; }
        public Command(string name, bool isActivated)
        {
            Name = name;
            IsActivated = isActivated;
        }
    }
    class ActivatedCommand : Command
    {
        public long Activator { get; private set; }
        public ActivatedCommand(string name, MessageEventArgs args) : base(name, true)
        {
            Activator = args.Message.Chat.Id;
        }
    }
    class DisactivatedCommand : Command
    {
        public DisactivatedCommand(string name) : base(name, false) { }
    }
    class Commands
    {
        const string FilePath = "Files/Commands.json";

        public List<ActivatedCommand> ActivatedCommandsList { get; private set; }

        public Commands ()
        {
            try
            {
                ActivatedCommandsList = JsonConvert.DeserializeObject<List<ActivatedCommand>>(File.ReadAllText(FilePath));
            }
            catch (FileNotFoundException)
            {
                throw new ArgumentException("Файла Commands.json не существует");
            }
        }
        ~Commands()
        {
            File.WriteAllText(FilePath, JsonConvert.SerializeObject(ActivatedCommandsList));
        }

        static public bool IsCommand(string text)
        {
            if (text != null && text[0] == '/')
                return true;
            else
                return false;
        }
        public bool ContainsActivatedCommand(List<Command> list)
        {
            for (int i = 0; i < list.Count(); i++)
                if (list[i].IsActivated)
                    return true;
            return false;
        }
    }
}
