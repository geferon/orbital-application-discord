using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Gateway.Events;
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
    public class MiscCommands : CommandGroup
    {
        private readonly FeedbackService _feedbackService;
        private readonly ICommandContext _context;
        public MiscCommands(FeedbackService feedbackService, ICommandContext context)
        {
            _feedbackService = feedbackService;
            _context = context;
        }

        [Command("coinflip")]
        [Description("Randomly flips a coin")]
        public async Task<IResult> CoinFlip()
        {
            var rnd = new Random();
            bool isHead = rnd.NextDouble() >= 0.5;

            Embed embed = new(
                Title: "Coin flip", Colour: _feedbackService.Theme.Primary,
                Image: new EmbedImage(isHead ? "https://www.random.org/coins/faces/60-eur/spain-1euro/obverse.jpg" : "https://www.random.org/coins/faces/60-eur/spain-1euro/reverse.jpg")
            );
            return await _feedbackService.SendContextualEmbedAsync(embed, ct: this.CancellationToken);
        }
    }
}
