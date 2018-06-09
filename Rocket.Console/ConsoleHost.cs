﻿using System;
using System.Collections.Generic;
using System.IO;
using Rocket.API;
using Rocket.API.Commands;
using Rocket.API.Logging;
using Rocket.API.Plugins;
using Rocket.Core.Logging;

namespace Rocket.Console
{
    public class ConsoleHost : IHost
    {
        private readonly ILogger logger;

        public ConsoleHost(IRuntime runtime)
        {
            logger = runtime.Container.Resolve<ILogger>();
            Console = new RocketConsole(runtime.Container);
        }
        public IEnumerable<string> Capabilities => new List<string>();
        public string Name => "ConsoleHost";

        public string WorkingDirectory { get; set; } = Path.Combine(Environment.CurrentDirectory, "Rocket");

        public void Init(IRuntime runtime)
        {
            runtime.Container.Resolve<IPluginManager>().Init();
            ICommandHandler cmdHandler = runtime.Container.Resolve<ICommandHandler>();

            Directory.SetCurrentDirectory(WorkingDirectory);

            logger.LogInformation("Loaded; type \"help\" for help or \"exit\" to exit.");

            System.Console.ForegroundColor = ConsoleColor.Gray;
            System.Console.Write("> ");
            System.Console.ForegroundColor = ConsoleColor.White;

            string line;
            while (!(line = System.Console.ReadLine())?.Equals("exit", StringComparison.OrdinalIgnoreCase) ?? false)
            {
                if (!cmdHandler.HandleCommand(Console, line, ""))
                    System.Console.WriteLine("Command not found: " + line);
                System.Console.Write("> ");
            }
        }

        public void Shutdown()
        {
            Environment.Exit(0);
        }

        public string InstanceId => "console";
        public string ServerName => "Rocket Console";
        public ushort ServerPort => 0;

        public void Reload() { }

        public IConsole Console { get; private set; }

        public bool IsAlive => true;

        public string ConfigurationName => "ConsoleHost";
    }
}