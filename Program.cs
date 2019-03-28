using Bdq.Core;
using Bdq.Services;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Interactivity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bdq
{
    public class Program
    {
        public CancellationTokenSource Cts
        {
            get;
            private set;
        }

        public IServiceProvider Services
        {
            get;
            private set;
        }

        public DiscordShardedClient Discord
        {
            get;
            private set;
        }

        public static Program Instance
        {
            get;
            private set;
        }

        public Program()
        {
            Instance = this;

            Console.WriteLine("[Boot] GetUnixTime(): {0}", Utilities.GetUnixTime(DateTimeOffset.Now));

            this.Cts = new CancellationTokenSource();

            this.Services = new ServiceCollection()
                .AddSingleton(this)
                .AddSingleton<DatabaseService>()
                .BuildServiceProvider(true);

            this.Discord = new DiscordShardedClient(new DiscordConfiguration
            {
#if DEBUG
                LogLevel = LogLevel.Debug,
#else
                LogLevel = LogLevel.Warning,
#endif

                AutomaticGuildSync = true,
                AutoReconnect = true,
                GatewayCompressionLevel = GatewayCompressionLevel.Stream,
                HttpTimeout = TimeSpan.FromSeconds(45),
                Token = Environment.GetEnvironmentVariable("BDQ_TOKEN") ?? "undefined",
                TokenType = TokenType.Bot
            });

            this.Discord.DebugLogger.LogMessageReceived += (sender, e) =>
            {
                Console.WriteLine($"[{e.Application}]: {e.Message}");
            };
        }

        public async Task RunAsync()
        {
            Console.CancelKeyPress += (sender, e) =>
            {
                if (!this.Cts.IsCancellationRequested)
                    this.Cts.Cancel();

                e.Cancel = true;
            };
                        
            await this.SetupAsync();
            await this.Discord.StartAsync();

            while (!this.Cts.IsCancellationRequested)
                await Task.Delay(100);

            foreach (var (_, shard) in this.Discord.ShardClients)
                await shard.DisconnectAsync();
        }

        async Task SetupAsync()
        {
            await this.Discord.UseInteractivityAsync(new InteractivityConfiguration()
            {
                PaginationBehavior = TimeoutBehaviour.DeleteMessage
            });

            var modules = await this.Discord.UseCommandsNextAsync(new CommandsNextConfiguration
            {
                Services = this.Services,
                StringPrefixes = new[] { "bdq!", "bdq " },
                EnableDms = false
            });

            foreach(var (_, cnext) in modules)
            {
                cnext.CommandExecuted += async e =>
                {
                    Console.WriteLine("[CommandsNext] (Executed) {0} ({1}): #{2} ({3}) -> {4}#{5} ({6}): {7}",
                        e.Context.Guild.Name,
                        e.Context.Guild.Id,
                        e.Context.Channel.Name,
                        e.Context.Channel.Id,
                        e.Context.User.Username,
                        e.Context.User.Discriminator,
                        e.Context.User.Id,
                        e.Context.Message.Content);
                };

                cnext.CommandErrored += async e =>
                {
                    var ex = e.Exception;

                    while (ex is AggregateException)
                        ex = ex.InnerException;

                    if (ex is CommandNotFoundException)
                        return;

                    Console.WriteLine("[CommandsNext] (Failed) {0} ({1}): #{2} ({3}) -> {4}#{5} ({6}): {7}",
                        e.Context.Guild.Name,
                        e.Context.Guild.Id,
                        e.Context.Channel.Name,
                        e.Context.Channel.Id,
                        e.Context.User.Username,
                        e.Context.User.Discriminator,
                        e.Context.User.Id,
                        e.Context.Message.Content,
                        ex);

                    await e.Context.RespondAsync($"{e.Context.User.Mention} :x: Houve um erro durante a execução deste comando: {Formatter.InlineCode(ex.Message)}");
                };

                cnext.RegisterCommands(typeof(Program).Assembly);
                cnext.SetHelpFormatter<BdqHelpFormatter>();
            }
        }

        static void Main(string[] args)
        {
            Console.Clear();

            try
            {
                MainAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        static async Task MainAsync()
        {
            var app = new Program();
            await app.RunAsync();
        }
    }
}
