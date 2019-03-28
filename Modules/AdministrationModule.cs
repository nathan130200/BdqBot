using Bdq.Entities.QA;
using Bdq.Extensions;
using Bdq.Services;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Bdq.Modules
{
    [Group, Aliases("admin")]
    public class AdministrationModule : BaseCommandModule
    {
        public DatabaseService Database { get; set; }

        [Command("+questao")]
        public async Task AdicionarQuestao(CommandContext ctx, string codigo = null, string texto = null, string categoria = null, string subcategoria = null, int pontos = 1)
        {
            if (string.IsNullOrEmpty(codigo))
            {
                await ctx.RespondAsync($"{ctx.User.Mention} :x: Você deve fornecer o código da questão.");
                return;
            }

            if (string.IsNullOrEmpty(texto))
            {
                await ctx.RespondAsync($"{ctx.User.Mention} :x: Você deve fornecer o título da questão.");
                return;
            }

            if (string.IsNullOrEmpty(categoria))
            {
                await ctx.RespondAsync($"{ctx.User.Mention} :x: Você deve fornecer a categoria da questão.\n**EXEMPLO**: `Desenvolvimento de Software`");
                return;
            }

            if (string.IsNullOrEmpty(subcategoria))
            {
                await ctx.RespondAsync($"{ctx.User.Mention} :x: Você deve fornecer a sub categoria da questão.\n**EXEMPLO:** `Visual Basic`");
                return;
            }

            try
            {
                var qs = new Questao();
                qs.Id = codigo;
                qs.Texto = texto;
                qs.Categoria = categoria;
                qs.SubCategoria = subcategoria;
                qs.Pontos = pontos;
                qs.Alternativas = new List<QuestaoAlternativa>();

                this.Database.Questions.InsertOne(qs);

                await ctx.RespondAsync($"{ctx.User.Mention} :white_check_mark: Questão criada com sucesso e pronta para editar.\nUtilzie os comandos `+alternativa` e `-alternativa` para adicionar e remover alternativas.");
                return;
            }
            catch
            {
                await ctx.RespondAsync($"{ctx.User.Mention} :x: Erro interno do servidor.");
                return;
            }
        }

        [Command("-questao")]
        public async Task RemoverQuestao(CommandContext ctx, string codigo = null)
        {
            if (string.IsNullOrEmpty(codigo))
            {
                await ctx.RespondAsync($"{ctx.User.Mention} :x: Você deve fornecer o código da questão.");
                return;
            }

            var questao = this.Database.Questions.Find(x => x.Id == codigo)
                .FirstOrDefault();

            if (questao == null)
            {
                await ctx.RespondAsync($"{ctx.User.Mention} :x: Questão com o código fornecido não existe.");
                return;
            }

            try
            {
                var dr = this.Database.Questions.DeleteOne(x => x.Id == codigo);
                if (dr.DeletedCount >= 1)
                {
                    await ctx.RespondAsync($"{ctx.User.Mention} :white_check_mark: Questão removida no banco.");
                    return;
                }
            }
            catch
            {
                await ctx.RespondAsync($"{ctx.User.Mention} :x: Erro interno do servidor.");
                return;
            }
        }

        [Command("+alternativa")]
        public async Task AdicionarAlternativa(CommandContext ctx, string codigo = null, string texto = null, bool correta = false)
        {
            if (string.IsNullOrEmpty(codigo))
            {
                await ctx.RespondAsync($"{ctx.User.Mention} :x: Você deve fornecer o código da questão.");
                return;
            }

            if (string.IsNullOrEmpty(texto))
            {
                await ctx.RespondAsync($"{ctx.User.Mention} :x: Você deve fornecer o código da questão.");
                return;
            }

            var qs = this.Database.Questions.Find(x => x.Id == codigo)
                .FirstOrDefault();

            if (qs == null)
            {
                await ctx.RespondAsync($"{ctx.User.Mention} :x: Questão com o código fornecido não existe.");
                return;
            }

            if (qs.Alternativas.Count >= 9)
            {
                await ctx.RespondAsync($"{ctx.User.Mention} :x: Número máximo de alternativas alcançado.");
                return;
            }

            var interactivity = ctx.Client.GetInteractivity();

            if (qs.Alternativas.Any(x => x.Valida) && correta)
            {
                var msg = await ctx.RespondAsync($"{ctx.User.Mention} :x: Já existe uma alternativa definida como `correta`. Continuar?\n**Responda**: _`Sim`_ ou _`Não`_");
                var mctx = await interactivity.WaitForMessageAsync(x => x.Author == ctx.User);
                if(mctx == null)
                {
                    await ctx.RespondAsync($"{ctx.User.Mention} :x: Tempo limite esgotado.");
                    return;
                }

                var continua = false;

                switch (mctx.Message.Content.ToLowerInvariant())
                {
                    case "s":
                    case "si":
                    case "sim":
                    case "y":
                        continua = true;
                        break;

                    case "não":
                    case "n":
                    case "nops":
                    case "nop":
                    default:
                        continua = false;
                        break;
                }

                if (!continua)
                {
                    await ctx.RespondAsync($"{ctx.User.Mention} :x: Operação cancelada pelo usuário.");
                    return;
                }

                var alt = qs.Alternativas.Where(x => x.Valida)
                    .FirstOrDefault();

                alt.Valida = false;

                qs.Alternativas.Add(new QuestaoAlternativa
                {
                    Texto = texto,
                    Valida = correta
                });

                try
                {
                    var update = Builders<Questao>.Update
                        .Set(x => x.Alternativas, qs.Alternativas);

                    var ur = this.Database.Questions.UpdateOne(fd => fd.Id == codigo, update);
                    if(ur.ModifiedCount >= 1)
                    {
                        await ctx.RespondAsync($"{ctx.User.Mention} :white_check_mark: Alternativa adicionada com sucesso.");
                        return;
                    }
                    else
                    {
                        await ctx.RespondAsync($"{ctx.User.Mention} :x: Erro interno do servidor.");
                        return;
                    }
                }
                catch
                {
                    await ctx.RespondAsync($"{ctx.User.Mention} :x: Erro interno do servidor.");
                    return;
                }
            }

            else
            {
                var alt = new QuestaoAlternativa();
                alt.Texto = texto;
                alt.Valida = correta;
                qs.Alternativas.Add(alt);

                var ud = Builders<Questao>.Update
                    .Set(x => x.Alternativas, qs.Alternativas);

                try
                {
                    var update = Builders<Questao>.Update
                        .Set(x => x.Alternativas, qs.Alternativas);

                    var ur = this.Database.Questions.UpdateOne(fd => fd.Id == codigo, update);
                    if (ur.ModifiedCount >= 1)
                    {
                        await ctx.RespondAsync($"{ctx.User.Mention} :white_check_mark: Alternativa adicionada com sucesso.");
                        return;
                    }
                    else
                    {
                        await ctx.RespondAsync($"{ctx.User.Mention} :x: Erro interno do servidor.");
                        return;
                    }
                }
                catch
                {
                    await ctx.RespondAsync($"{ctx.User.Mention} :x: Erro interno do servidor.");
                    return;
                }
            }
        }

        [Command("-alternativa")]
        public async Task RemoverAlternativa(CommandContext ctx, string codigo)
        {
            if (string.IsNullOrEmpty(codigo))
            {
                await ctx.RespondAsync($"{ctx.User.Mention} :x: Você deve fornecer o código da questão.");
                return;
            }

            var qs = this.Database.Questions.Find(x => x.Id == codigo)
                .FirstOrDefault();

            if (qs == null)
            {
                await ctx.RespondAsync($"{ctx.User.Mention} :x: Questão com o código fornecido não existe.");
                return;
            }

            if(qs.Alternativas.Count == 0)
            {
                await ctx.RespondAsync($"{ctx.User.Mention} :x: Essa questão não possui alternativas para serem alteradas.");
                return;
            }

            var str = "";
            var off = 1;

            for (var i = 0; i < qs.Alternativas.Count; i++)
            {
                var alternativa = qs.Alternativas[i];

                str += "[`" + off++ + "`]: " + (alternativa.Valida ? "**@** " : "") + Formatter.Sanitize(alternativa.Texto);

                if (qs.Alternativas.IndexOf(alternativa) != qs.Alternativas.Count - 1)
                    str += "\n\u200b\n";
            }
            await ctx.RespondAsync(ctx.User.Mention + "\nQuestão:\n\u200b\n" + Formatter.Sanitize(qs.Texto), embed: new DiscordEmbedBuilder()
                .WithDescription("Qual o **número** da alternativa que você deseja modificar?\n\u200b\n" + str + "\n\u200b\n" + "**LEGENDA:**\n**@**: Questão Correta.\n[`N`]: Número da Alternativa")
                .WithColor(DiscordColor.Blurple)
            );

            var interactivity = ctx.Client.GetInteractivity();
            var pos = -1;
            var mctx = await interactivity.WaitForMessageAsync(x => x.Author == ctx.User);
            if (mctx == null)
            {
                await ctx.RespondAsync($"{ctx.User.Mention} :x: Tempo limite esgotado.");
                await mctx.Message.TryDeleteAsync();
                return;
            }

            await mctx.Message.TryDeleteAsync();

            var opos = await new Int32Converter()
                .ConvertAsync(mctx.Message.Content, ctx);

            if (!opos.HasValue)
            {
                await ctx.RespondAsync($"{ctx.User.Mention} :x: Valor digitado não é um número valido.");
                return;
            }

            pos = opos.Value;

            if (pos < 1 || pos > 9)
            {
                await ctx.RespondAsync($"{ctx.User.Mention} :x: Alternativa incorreta.");
                return;
            }

            var Num1 = DiscordEmoji.FromName(ctx.Client, ":one:");
            var Num2 = DiscordEmoji.FromName(ctx.Client, ":two:");
            var Num3 = DiscordEmoji.FromName(ctx.Client, ":three:");
            var NumX = DiscordEmoji.FromName(ctx.Client, ":x:");

            var alt = qs.Alternativas[pos];

            var msg = await ctx.RespondAsync(ctx.User.Mention, embed: new DiscordEmbedBuilder()
                .WithDescription("O que deseja fazer com essa alternativa?\n\u200b\n " + Formatter.Sanitize(alt.Texto) + "\n\u200b\n\n:one: **Alterar Texto**\n:two: **Definir Correta**\n:three: **Remover**\n:x: **Cancelar**")
            );

            await msg.CreateReactionAsync(Num1);
            await msg.CreateReactionAsync(Num2);
            await msg.CreateReactionAsync(Num3);
            await msg.CreateReactionAsync(NumX);

            var rctx = await interactivity.WaitForMessageReactionAsync(msg, ctx.User);
            if(rctx == null)
            {
                await ctx.RespondAsync($"{ctx.User.Mention} :x: Tempo limite esgotado.");
                await rctx.Message.TryDeleteAsync();
                return;
            }
        }
    }
}