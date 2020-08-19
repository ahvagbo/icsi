#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ICsi.Core
{
    internal sealed partial class Repl
    {
        private IDictionary<string, MethodInfo> _commands;

        private static class ReplCommands
        {
            [ReplMetaCommand(".cls", "Clears the screen")]
            public static void ClsCommand()
            {
                Console.Clear();
            }

            [ReplMetaCommand(".quit", "Exits the REPL")]
            public static void QuitCommand()
            {
                Environment.Exit(0);
            }

            [ReplMetaCommand(".help", "Prints this help message")]
            public static void HelpCommand()
            {
                Console.WriteLine("Keys:");
                Console.WriteLine("PageUp      Loads previous submission");
                Console.WriteLine("PageDown    Loads next submission");
                Console.WriteLine("Enter       Executes the given submission");
                Console.WriteLine("Ctrl+Enter  Inserts a new line");
                Console.WriteLine("Arrows      Browse through the code");

                Console.WriteLine();

                Console.WriteLine("C#-specific directives:");
                Console.WriteLine("#r\tReferences a metadata file");
                Console.WriteLine("#load\tLoads a C# script");

                Console.WriteLine();

                IEnumerable<MethodInfo> methods = from m in (typeof(ReplCommands).GetMethods(BindingFlags.Public
                                                                                           | BindingFlags.Static)).AsEnumerable()
                                                  where m.GetCustomAttribute(typeof(ReplMetaCommandAttribute)) != null
                                                  select m;
                
                IEnumerable<ReplMetaCommandAttribute> GetCommands()
                {
                    foreach (MethodInfo method in methods)
                    {
                        yield return (ReplMetaCommandAttribute)method.GetCustomAttribute(typeof(ReplMetaCommandAttribute))!;
                    }
                }

                Console.WriteLine("REPL-specific commands");
                foreach (ReplMetaCommandAttribute command in GetCommands())
                    Console.WriteLine($"{command.Command}\t{command.Description}");
            }
        }

        private void RegisterMetaCommands()
        {
            _commands = new Dictionary<string, MethodInfo>();

            MethodInfo[] methods = typeof(ReplCommands).GetMethods(BindingFlags.Public
                                                                 | BindingFlags.Static);
            
            foreach (MethodInfo method in methods)
            {
                var attribute = (ReplMetaCommandAttribute)method.GetCustomAttribute(typeof(ReplMetaCommandAttribute))!;

                if (attribute == null)
                    continue;
                
                _commands.Add(attribute.Command, method);
            }
        }

        private void ExecuteCommand(string command)
        {
            if (_commands.ContainsKey(command))
                _commands[command].Invoke(null, new object[0]);
            else
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.Error.WriteLine($"Unrecognized command: {command}. Type .help to know which commands are available.");
                Console.ResetColor();
            }
        }
    }
}