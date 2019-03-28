using Bdq.Entities.Profile;
using Bdq.Extensions;
using Bdq.Services;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bdq.Modules
{
    public class BdqModule : BaseCommandModule
    {
        public DatabaseService Database { get; set; }

        [Command, Aliases("ping")]
        public async Task Latencia(CommandContext ctx)
        {
            var watch = Stopwatch.StartNew();
            await ctx.TriggerTypingAsync();
            watch.Stop();

            var avg = watch.ElapsedMilliseconds + ctx.Client.Ping / 2;

            await ctx.RespondAsync(embed: new DiscordEmbedBuilder()
                .WithRequestedBy(ctx.User)
                .WithColor(DiscordColor.Yellow)
                .WithAuthor("BDQ: Latência", iconUrl: ctx.Client.CurrentUser.GetAvatarUrl(ImageFormat.Jpeg))
                .WithField(":incoming_envelope: Rest", $"{watch.ElapsedMilliseconds}ms")
                .WithField(":satellite_orbital: Gateway", $"{ctx.Client.Ping}ms")
                .WithField(":ping_pong: Média", $"{avg}ms")
            );
        }

        [Command, Aliases("profile")]
        public async Task Perfil(CommandContext ctx)
        {
            var profile = this.Database.Profiles.Find(x => x.Id == ctx.User.Id)
                .FirstOrDefault();

            if (profile == null)
            {
                profile = new PerfilUsuario();
                profile.Id = ctx.User.Id;
                profile.Experience = 0;
                profile.Pontos = 0;

                this.Database.Profiles.InsertOne(profile);
            }

            var respostas = this.Database.Answers.Find(x => x.UsuarioId == ctx.User.Id)
                .ToEnumerable();

            var total = respostas.Count();
            var acertou = respostas.Where(x => x.Acertou).Count();
            var errou = respostas.Where(x => !x.Acertou).Count();

            var simulados = this.Database.Simulados.Find(xs => xs.UsuarioId == ctx.User.Id)
                .ToList();

            total += simulados.SelectMany(x => x.Respostas)
                .Count();

            acertou += simulados.SelectMany(x => x.Respostas)
                .Where(x => x.Acertou)
                .Count();

            errou += simulados.SelectMany(x => x.Respostas)
                .Where(x => !x.Acertou)
                .Count();

            await ctx.RespondAsync(embed: new DiscordEmbedBuilder()
                .WithRequestedBy(ctx.User, true)
                .WithAuthor("BDQ: Seu perfil", iconUrl: ctx.Client.CurrentUser.GetAvatarUrl(ImageFormat.Png))
                .WithInlineField(":musical_score: Experiência", $"{profile.Experience:#,0}")
                .WithInlineField(":diamond_shape_with_a_dot_inside: Pontos Acumulados", $"{profile.Pontos:#,0}")
                .WithInlineField(":question: Perguntas Respondidas", $"{total:#,0}")
                .WithInlineField(":green_book: Perguntas Acertadas", $"{acertou:#,0}")
                .WithInlineField(":closed_book: Perguntas Erradas", $"{errou:#,0}")
                .WithInlineField(":books: Simulados Feitos", $"{simulados.Count:#,0}")
                .WithColor(DiscordColor.Yellow)
            );
        }
    }
}