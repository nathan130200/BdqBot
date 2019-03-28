using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.CommandsNext.Entities;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;

namespace Bdq.Core
{
    public sealed class BdqHelpFormatter : BaseHelpFormatter
    {
        public DiscordEmbedBuilder Embed
        {
            get;
            private set;
        }

        public Command Command {
            get;
            private set;
        }

        public BdqHelpFormatter(CommandContext ctx) : base(ctx)
        {
            this.Embed = new DiscordEmbedBuilder()
                .WithFooter($"Solicitado por {ctx.User.Username}#{ctx.User.Discriminator}", ctx.User.AvatarUrl)
                .WithAuthor("BDQ: Ajuda", string.Empty, ctx.Client.CurrentUser.GetAvatarUrl(ImageFormat.Png))
                .WithThumbnailUrl("https://i.imgur.com/pGZrf9D.png");

            this.Command = null;
        }

        public override CommandHelpMessage Build()
        {
            return new CommandHelpMessage(embed: this.Embed);
        }

        /// <summary>
        /// Embed Description Newline.
        /// </summary>
        public const string N = "\n\u200b\n";

        public override BaseHelpFormatter WithCommand(Command command)
        {
            this.Command = command;
            {
                this.Embed.Author.Name = "BDQ: Ajuda | Comando: " + this.Command.Name;
                this.Embed.Description += ":information_source: **Informação:**" + N + (this.Command.Description ?? "_Não Fornecido_") + N;

                if (this.Command.Aliases?.Any() == true)
                    this.Embed.Description += ":twisted_rightwards_arrows: **Alternativas:**" + N + string.Join("\n", this.Command.Aliases.Select(Formatter.InlineCode)) + N;

                if(this.Command.Overloads.Any() == true)
                {
                    this.Embed.Description += ":zap: **Sobrecargas:**" + N;

                    foreach(var overload in this.Command.Overloads.OrderByDescending(x => x.Priority))
                    {
                        var sb = new StringBuilder();
                        {
                            sb.Append("`")
                                .Append(command.QualifiedName)
                                .Append("`: ");

                            foreach(var arg in overload.Arguments)
                            {
                                sb.Append("`");

                                if (arg.IsOptional || arg.IsCatchAll)
                                    sb.Append("[");
                                else
                                    sb.Append("<");

                                {
                                    sb.Append(arg.Name);

                                    if (arg.IsCatchAll)
                                        sb.Append("...");
                                }

                                if (arg.IsOptional || arg.IsCatchAll)
                                    sb.Append("]");
                                else
                                    sb.Append(">");

                                sb.Append("`");

                                {
                                    var part = "";

                                    switch (Type.GetTypeCode(arg.Type))
                                    {
                                        case TypeCode.Boolean: part = "condição"; break;
                                        case TypeCode.Byte: part = "byte"; break;
                                        case TypeCode.Char: part = "caractere"; break;
                                        case TypeCode.DateTime: part = "data/hora"; break;
                                        case TypeCode.Decimal: part = "número decimal"; break;
                                        case TypeCode.Double: part = "número flutuante pequeno"; break;
                                        case TypeCode.Int16: part = "número pequeno"; break;
                                        case TypeCode.Int32: part = "número"; break;
                                        case TypeCode.Int64: part = "número longo"; break;
                                        case TypeCode.Object: part = "objeto"; break;
                                        case TypeCode.SByte: part = "byte"; break;
                                        case TypeCode.Single: part = "número flutuante"; break;
                                        case TypeCode.String: part = "texto" + (arg.IsCatchAll ? " longo" : ""); break;
                                        case TypeCode.UInt16: part = "número pequeno positivo"; break;
                                        case TypeCode.UInt32: part = "número positivo"; break;
                                        case TypeCode.UInt64: part = "número longo positivo"; break;
                                        default:
                                        {
                                            if (arg.Type.IsAssignableFrom(typeof(DiscordUser)))
                                                part = "usuário";
                                            else if (arg.Type.IsAssignableFrom(typeof(DiscordMember)))
                                                part = "membro";
                                            else if (arg.Type.IsAssignableFrom(typeof(DiscordChannel)))
                                                part = "canal";
                                            else if (arg.Type.IsAssignableFrom(typeof(DiscordEmoji)))
                                                part = "emoji";
                                            else
                                                part = arg.Type.FullName;
                                        }
                                        break;
                                    }

                                    part = "(" + part + ")";
                                    sb.Append(" " + part + "\n");
                                }

                            }
                        }
                        this.Embed.Description += sb.ToString();
                    }
                }
            }
            return this;
        }

        public override BaseHelpFormatter WithSubcommands(IEnumerable<Command> subcommands)
        {
            var p = "";

            if (this.Command == null)
                p = "_Lista de Comandos_:" + N;

            this.Embed.Description += p + "\n:popcorn: **" + (this.Command == null ? "Comandos" : "Sub Comandos") 
                + " **:\n" + string.Join(", ", subcommands.Select(x => Formatter.InlineCode(x.Name)));

            return this;
        }
    }
}
