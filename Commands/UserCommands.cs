using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Results;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrbitalDiscord.Commands
{
    public class UserCommands : CommandGroup
    {
        private readonly FeedbackService _feedbackService;
        private readonly ICommandContext _context;
        public UserCommands(FeedbackService feedbackService, ICommandContext context)
        {
            _feedbackService = feedbackService;
            _context = context;
        }

        [Command("avatar")]
        [Description("Get a users avatar")]
        public async Task<IResult> GetUserAvatar([Description("User to get the avatar from, leave empty for self")] IUser? user)
        {
            user ??= _context.User;

            var pfp = CDN.GetUserAvatarUrl(user);
            if (!pfp.IsSuccess)
            {
                return await _feedbackService.SendContextualErrorAsync("User doesn't have an avatar", ct: this.CancellationToken);
            }

            var embed = new Embed(Title: user.Username, Colour: _feedbackService.Theme.Primary, Image: new EmbedImage(pfp.Entity.AbsoluteUri));
            return await _feedbackService.SendContextualEmbedAsync(embed, ct: this.CancellationToken);
        }
    }
}
