﻿/*
 * SPDX-FileCopyrightText: 2019 Daniele Corsini <daniele.corsini@corsinvest.it>
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: GPL-3.0-only
 */

using Corsinvest.ProxmoxVE.Api.Extension.Utils;
using Corsinvest.ProxmoxVE.Api.Shared;
using Corsinvest.ProxmoxVE.Api.Shared.Utils;
using Microsoft.Extensions.Logging;
using System;
using System.CommandLine;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Corsinvest.ProxmoxVE.Api.Shell.Helpers
{
    /// <summary>
    /// Command option shell extension.
    /// </summary>
    public static class CommandOptionExtension
    {
        /// <summary>
        /// Add fullname and logo
        /// </summary>
        /// <param name="command"></param>
        public static void AddFullNameLogo(this Command command)
        {
            command.Description = ConsoleHelper.MakeLogoAndTitle(command.Description) + $@"

{command.Name} is a part of suite cv4pve.
For more information visit https://www.cv4pve-tools.com";
        }

        /// <summary>
        /// Get option
        /// </summary>
        /// <param name="command"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Option GetOption(this Command command, string name)
            => command.Options.FirstOrDefault(a => a.Name == name || a.Aliases.Contains(name));

        /// <summary>
        /// Get option
        /// </summary>
        /// <param name="command"></param>
        /// <param name="name"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Option<T> GetOption<T>(this Command command, string name) => (Option<T>)command.GetOption(name);


        /// <summary>
        /// Dryrun is active
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public static bool DryRunIsActive(this Command command) => command.GetOption<bool>("dry-run").GetValue();

        /// <summary>
        /// Debug is active
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public static bool DebugIsActive(this Command command) => command.GetOption<bool>(DebugOptionName).GetValue();

        /// <summary>
        /// Get LogLevel from debug
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public static LogLevel GetLogLevelFromDebug(this Command command)
            => command.DebugIsActive()
                ? LogLevel.Trace
                : LogLevel.Warning;

        /// <summary>
        /// Debug option
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public static Option<bool> DebugOption(this Command command)
        {
            var opt = new Option<bool>($"--{DebugOptionName}", "Debug application")
            {
                IsHidden = true
            };
            command.AddGlobalOption(opt);
            return opt;
        }

        /// <summary>
        /// Dry run option
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public static Option<bool> DryRunOption(this Command command)
        {
            var opt = new Option<bool>("--dry-run", "Dry run application")
            {
                IsHidden = true,
            };
            command.AddGlobalOption(opt);
            return opt;
        }

        /// <summary>
        /// Get Value from option
        /// </summary>
        /// <param name="option"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetValue<T>(this Option<T> option)
            => option.Parse(Environment.GetCommandLineArgs()).GetValueForOption(option);

        /// <summary>
        /// Get value
        /// </summary>
        /// <param name="option"></param>
        /// <returns></returns>
        public static string GetValue(this Option option)
            => option.Parse(Environment.GetCommandLineArgs()).GetValueForOption(option) + "";

        /// <summary>
        /// Get value
        /// </summary>
        /// <param name="argument"></param>
        /// <returns></returns>
        public static string GetValue(this Argument argument)
            => argument.Parse(Environment.GetCommandLineArgs()).GetValueForArgument(argument) + "";

        /// <summary>
        /// Get value
        /// </summary>
        /// <param name="argument"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetValue<T>(this Argument<T> argument)
            => argument.Parse(Environment.GetCommandLineArgs()).GetValueForArgument(argument);

        /// <summary>
        /// Return if a option has value
        /// </summary>
        /// <param name="option"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool HasValue<T>(this Option<T> option) => option.GetValue() != null;

        /// <summary>
        /// Has value
        /// </summary>
        /// <param name="option"></param>
        /// <returns></returns>
        public static bool HasValue(this Option option) => !string.IsNullOrWhiteSpace(option.GetValue());

        /// <summary>
        /// Id or name option
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public static Option VmIdOrNameOption(this Command command) => command.AddOption("--vmid", "The id or name VM/CT");

        /// <summary>
        /// Ids or names option
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public static Option VmIdsOrNamesOption(this Command command)
        {
            var opt = command.VmIdOrNameOption();
            opt.Description = @"The id or name VM/CT comma separated (eg. 100,101,102,TestDebian)
-vmid or -name exclude (e.g. -200,-TestUbuntu)
range 100:107,-105,200:204
'@pool-???' for all VM/CT in specific pool (e.g. @pool-customer1),
'@all-???' for all VM/CT in specific host (e.g. @all-pve1, @all-\$(hostname)),
'@all' for all VM/CT in cluster";

            return opt;
        }

        /// <summary>
        /// Get Api token
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public static Option GetApiToken(this Command command) => command.GetOption(ApiTokenOptionName);

        /// <summary>
        /// Get username
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public static Option GetUsername(this Command command) => command.GetOption(UsernameOptionName);

        /// <summary>
        /// Get password
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public static Option GetPassword(this Command command) => command.GetOption(PasswordOptionName);

        /// <summary>
        /// Get host
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public static Option GetHost(this Command command) => command.GetOption(HostOptionName);

        #region Login option
        /// <summary>
        /// Add options login
        /// </summary>
        /// <param name="command"></param>
        public static void AddLoginOptions(this Command command)
        {
            var optApiToken = command.AddOption($"--{ApiTokenOptionName}", "Api token format 'USER@REALM!TOKENID=UUID'. Require Proxmox VE 6.2 or later");
            var optUsername = command.AddOption($"--{UsernameOptionName}", "User name <username>@<realm>");
            var optPassword = command.AddOption($"--{PasswordOptionName}", "The password. Specify 'file:path_file' to store password in file.");

            var optHost = new Option<string>($"--{HostOptionName}",
                                             parseArgument: (e) =>
                                             {
                                                 if (e.FindResultFor(optApiToken) == null && e.FindResultFor(optUsername) == null)
                                                 {
                                                     e.ErrorMessage = $"Option '--{optUsername.Name}' or '--{optApiToken.Name}' is required!";
                                                 }
                                                 else if (e.FindResultFor(optUsername) != null && e.FindResultFor(optPassword) == null)
                                                 {
                                                     e.ErrorMessage = $"Option '--{optPassword.Name}' is required!";
                                                 }

                                                 return e.Tokens.Single().Value;
                                             }, description: "The host name host[:port],host1[:port],host2[:port]")
            {
                IsRequired = true
            };
            command.AddOption(optHost);
        }

        /// <summary>
        /// Api Token
        /// </summary>
        public static readonly string ApiTokenOptionName = "api-token";

        /// <summary>
        /// Host option
        /// </summary>
        public static readonly string HostOptionName = "host";

        /// <summary>
        /// Username option
        /// </summary>
        public static readonly string UsernameOptionName = "username";

        /// <summary>
        /// Password option
        /// </summary>
        public static readonly string PasswordOptionName = "password";

        /// <summary>
        /// Password option
        /// </summary>
        public static readonly string DebugOptionName = "debug";

        /// <summary>
        /// Host option
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public static Option HostOption(this Command command)
        {
            var option = command.AddOption($"--{HostOptionName}", "The host name host[:port],host1[:port],host2[:port]");
            option.IsRequired = true;
            return option;
        }

        /// <summary>
        /// Table output enum
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public static Option<TableGenerator.Output> TableOutputOption(this Command command)
        {
            var opt = command.AddOption<TableGenerator.Output>("--output|-o", "Type output");
            opt.SetDefaultValue(TableGenerator.Output.Text);
            return opt;
        }

        /// <summary>
        /// username options
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public static Option Username(this Command command) => command.AddOption($"--{UsernameOptionName}", "User name");

        /// <summary>
        /// Verbose
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public static Option<bool> VerboseOption(this Command command) => command.AddOption<bool>($"--verbose|-v", "Verbose.");

        /// <summary>
        /// Try login client api
        /// </summary>
        /// <param name="command"></param>
        /// <param name="loggerFactory"></param>
        /// <returns></returns>
        public static async Task<PveClient> ClientTryLogin(this Command command, ILoggerFactory loggerFactory)
        {
            var error = "Problem connection!";
            try
            {
                var client = ClientHelper.GetClientFromHA(command.GetHost().GetValue());
                client.LoggerFactory = loggerFactory;

                if (command.GetApiToken().HasValue())
                {
                    //use api token
                    client.ApiToken = command.GetApiToken().GetValue();

                    //check is valid API
                    var ver = await client.Version.Version();
                    if (ver.IsSuccessStatusCode) { return client; }
                }
                else
                {
                    //use user and password
                    //try login
                    if (await client.Login(command.GetUsername().GetValue(), GetPasswordFromOption(command)))
                    {
                        return client;
                    }

                }

                if (!client.LastResult.IsSuccessStatusCode) { error += " " + client.LastResult.ReasonPhrase; }
            }
            catch (Exception ex)
            {
                throw new PveException(error, ex);
            }

            throw new PveException(error);
        }

        /// <summary>
        /// Return password from option
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public static string GetPasswordFromOption(this Command command)
        {
            const string key = "012345678901234567890123";

            var password = command.GetPassword().GetValue();
            if (!string.IsNullOrWhiteSpace(password))
            {
                password = password.Trim();

                //check if file
                if (password.StartsWith("file:"))
                {
                    var fileName = password[5..];
                    if (File.Exists(fileName))
                    {
                        password = StringHelper.Decrypt(File.ReadAllText(fileName, Encoding.UTF8), key);
                    }
                    else
                    {
                        Console.WriteLine("Password:");
                        password = ConsoleHelper.ReadPassword();
                        File.WriteAllText(fileName, StringHelper.Encrypt(password, key), Encoding.UTF8);
                    }
                }
            }

            return password;
        }
        #endregion

        /// <summary>
        /// Vm Id option
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public static Option<int> VmIdOption(this Command command) => command.AddOption<int>("--vmid", "The id VM/CT");

        /// <summary>
        /// Add option
        /// </summary>
        /// <param name="command"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        public static Option AddOption(this Command command, string name, string description)
            => command.AddOption<string>(name, description);

        /// <summary>
        /// Add argument
        /// </summary>
        /// <param name="command"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        public static Argument AddArgument(this Command command, string name, string description)
            => command.AddArgument<string>(name, description);

        /// <summary>
        /// Add argument
        /// </summary>
        /// <param name="command"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Argument<T> AddArgument<T>(this Command command, string name, string description)
        {
            var argument = new Argument<T>(name, description);
            command.AddArgument(argument);
            return argument;
        }


        /// <summary>
        /// Add command
        /// </summary>
        /// <param name="command"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        public static Command AddCommand(this Command command, string name, string description)
        {
            var cmd = CreateObject<Command>(name, description);
            command.AddCommand(cmd);
            return cmd;
        }

        private static T CreateObject<T>(string name, string description) where T : IdentifierSymbol
        {
            var names = name.Split("|");
            var obj = (T)Activator.CreateInstance(typeof(T), new[] { names[0], description });
            for (int i = 1; i < names.Length; i++) { obj.AddAlias(names[i]); }
            return obj;
        }

        /// <summary>
        /// Add option
        /// </summary>
        /// <param name="command"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Option<T> AddOption<T>(this Command command, string name, string description)
        {
            var option = CreateObject<Option<T>>(name, description);
            command.AddOption(option);
            return option;
        }

        /// <summary>
        /// Add validator range
        /// </summary>
        /// <param name="option"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static Option<int> AddValidatorRange(this Option<int> option, int min, int max)
        {
            option.AddValidator(e =>
            {
                var value = e.GetValueOrDefault<int>();
                if (value < min || value > max) { e.ErrorMessage = $"Option {e.Token.Value} whit value '{value}' is not in range!"; }
            });

            return option;
        }

        /// <summary>
        /// Add validator exist file
        /// </summary>
        /// <param name="option"></param>
        public static Option AddValidatorExistFile(this Option option)
        {
            option.AddValidator(e =>
            {
                var value = e.GetValueOrDefault<string>();
                if (!File.Exists(value)) { e.ErrorMessage = $"Option {e.Token.Value} whit value '{value}' is not a valid file!"; }
            });

            return option;
        }

        /// <summary>
        /// Add validator exist directory
        /// </summary>
        /// <param name="option"></param>
        public static Option AddValidatorExistDirectory(this Option option)
        {
            option.AddValidator(e =>
            {
                var value = e.GetValueOrDefault<string>();
                if (!Directory.Exists(value)) { e.ErrorMessage = $"Option {e.Token.Value} whit value '{value}' is not a valid file!"; }
            });

            return option;
        }

        /// <summary>
        /// Command check update application
        /// </summary>
        /// <param name="rootCommand"></param>
        public static void CheckUpdateApp(this RootCommand rootCommand)
        {
            var cmd = new Command("app-check-update", "Check update application");
            cmd.SetHandler(async () =>
            {
                Console.Out.Write((await UpdateHelper.GetInfo(rootCommand.Name)).Info);
            });
            rootCommand.AddCommand(cmd);
        }

        /// <summary>
        /// Upgrade application
        /// </summary>
        /// <param name="rootCommand"></param>
        public static void UpgradeApp(this RootCommand rootCommand)
        {
            const string appUpgradeFinish = "app-upgrade-finish";

            var optQuiet = new Option<bool>("--quiet", "Non-interactive mode, does not request confirmation");
            var cmdUpgrade = new Command("app-upgrade", "Upgrade application")
            {
                optQuiet
            };

            cmdUpgrade.SetHandler(async () =>
            {
                var (Info, IsNewVersion, BrowserDownloadUrl) = await UpdateHelper.GetInfo(rootCommand.Name);
                Console.Out.WriteLine(Info);

                if (IsNewVersion)
                {
                    if (!optQuiet.HasValue() && !ConsoleHelper.ReadYesNo("Confirm upgrade application?", false))
                    {
                        Console.Out.WriteLine("Upgrade abort!");
                        return;
                    }

                    Console.Out.WriteLine($"Download {BrowserDownloadUrl} ....");

                    var fileNameNew = await UpdateHelper.UpgradePrepare(BrowserDownloadUrl, Environment.ProcessPath);

                    Process.Start(fileNameNew, appUpgradeFinish);
                }
            });
            rootCommand.AddCommand(cmdUpgrade);

            //finish upgrade application
            var cmdUpgradeFinish = new Command(appUpgradeFinish)
            {
                IsHidden = true
            };
            cmdUpgradeFinish.SetHandler(() =>
            {
                UpdateHelper.UpgradeFinish(Environment.ProcessPath);

                Console.Out.WriteLine("Upgrade completed!");
            });
        }

        /// <summary>
        /// Script file option
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public static Option ScriptFileOption(this Command command)
            => command.AddOption("--script", "Use specified hook script").AddValidatorExistFile();

        /// <summary>
        /// Timeout operation
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public static Option<long> TimeoutOption(this Command command) => command.AddOption<long>("--timeout", "Timeout operation in seconds");
    }
}