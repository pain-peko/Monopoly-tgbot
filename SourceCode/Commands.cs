using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Telegram.Bot.Args;

namespace Monopoly_tgbot
{
    [Serializable]
    abstract class Command
    {
        public string Name { get; set; }
        public bool IsActivated { get; set; }
        public Command(string name, bool isActivated)
        {
            Name = name;
            IsActivated = isActivated;
        }
        public Command() { }
    }
    [Serializable]
    class ActivatedCommand : Command
    {
        public long Activator { get; set; }
        public ActivatedCommand(string name, MessageEventArgs args) : base(name, true)
        {
            Activator = args.Message.Chat.Id;
        }
        public ActivatedCommand() { }
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

                if (ActivatedCommandsList == null)
                    ActivatedCommandsList = new List<ActivatedCommand>();
            }
            catch (FileNotFoundException)
            {
                throw new ArgumentException("Файла Commands.json не существует");
            }
        }
        public void SaveCommands()
        {
            File.WriteAllText(FilePath, JsonConvert.SerializeObject(ActivatedCommandsList));
        }

        public bool ContainsActivatedCommand (string name, MessageEventArgs args)
        {
            for (int i = 0; i < ActivatedCommandsList.Count(); i++)
                if (ActivatedCommandsList[i].Name == name && ActivatedCommandsList[i].Activator == args.Message.Chat.Id)
                    return true;

            return false;
        }
        public bool ContainsActivatedCommand(MessageEventArgs args)
        {
            for (int i = 0; i < ActivatedCommandsList.Count(); i++)
                if (ActivatedCommandsList[i].Activator == args.Message.Chat.Id)
                    return true;

            return false;
        }
        public void RemoveCommand (string name, MessageEventArgs args)
        {
            for (int i = 0; i < ActivatedCommandsList.Count(); i++)
                if (ActivatedCommandsList[i].Name == name && ActivatedCommandsList[i].Activator == args.Message.Chat.Id)
                    ActivatedCommandsList.RemoveAt(i);
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
