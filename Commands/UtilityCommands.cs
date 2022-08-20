using Polly;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Attributes;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Discord.Extensions.Embeds;
using Remora.Results;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace OrbitalDiscord.Commands
{
    public class UtilityCommands : CommandGroup
    {
        private readonly FeedbackService _feedbackService;
        private readonly ICommandContext _context;
        private readonly IDiscordRestGuildAPI _guildApi;
        public UtilityCommands(FeedbackService feedbackService, ICommandContext context, IDiscordRestGuildAPI guildApi)
        {
            _feedbackService = feedbackService;
            _context = context;
            _guildApi = guildApi;
        }

        [Command("ping")]
        [Description("Pong!")]
        public async Task<IResult> ShowPing()
        {
            return await _feedbackService.SendContextualSuccessAsync("Pong!", ct: this.CancellationToken);
        }

        [Command("serverinfo")]
        [Description("Displays some basic server info")]
        [DiscordDefaultDMPermission(false)]
        public async Task<IResult> ShowServerInfo()
        {
            var guild = (await _guildApi.GetGuildAsync(_context.GuildID.Value, ct: this.CancellationToken)).Entity;
            var embedBuilderResult = new EmbedBuilder()
                //.WithTitle($"{guild.Name}'s Info")
                .WithAuthor($"{guild.Name}'s Info", iconUrl: guild.Icon?.Value)
                .SetFields(
                    new EmbedField[]
                    {
                    new("ID", guild.ID.ToString(), true),
                    new("Members", guild.ApproximateMemberCount.HasValue ? guild.ApproximateMemberCount.Value.ToString() : "Unknown", true),
                    new("Has AFK Channel?", (guild.AFKChannelID is not null).ToString().Capitalize(), true),
                    new("Has Splash?", (guild.Splash is not null).ToString().Capitalize(), true),
                    new("Guild's MFA Level", guild.MFALevel.ToString().Capitalize(), true),
                    new("Verification Level", guild.VerificationLevel.ToString().Capitalize(), true),
                    new("NSFW Level", guild.NSFWLevel.ToString().Capitalize(), true)
                    }
                );

            if (!embedBuilderResult.IsSuccess)
            {
                return Result.FromError(embedBuilderResult);
            }

            var embedBuilder = embedBuilderResult.Entity;

            if (guild.Banner is not null)
            {
                embedBuilder.WithImageUrl(guild.Banner.Value);
            }

            return await _feedbackService.SendContextualEmbedAsync(embedBuilder.Build().Entity, ct: this.CancellationToken);
        }
    }
}
