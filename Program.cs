using OrbitalDiscord;
using Remora.Commands.Extensions;
using Remora.Commands.Groups;
using Remora.Discord.API;
using Remora.Discord.Commands.Extensions;
using Remora.Discord.Commands.Services;
using Remora.Discord.Hosting.Extensions;
using Remora.Rest.Core;

IHost host = Host.CreateDefaultBuilder(args)
    .AddDiscordService(services =>
    {
        var cfg = services.GetRequiredService<IConfiguration>();
        return cfg.GetValue<string?>("REMORA_BOT_TOKEN") ??
            throw new InvalidOperationException("No bot token provided, please set REMORA_BOT_TOKEN to a valid token.");
    })
    .ConfigureServices(services =>
    {
        var cmdBuilder = services.AddDiscordCommands(enableSlash: true)
        .AddCommandTree();

        // Automatically add all command groups in our Assembly
        var commandGroupTypes = typeof(Program).Assembly
            .GetExportedTypes()
            .Where(t =>
                t.IsAssignableTo(typeof(CommandGroup))
            );

        foreach (var group in commandGroupTypes)
        {
            cmdBuilder.WithCommandGroup(group);
        }
    })
    .Build();

var services = host.Services;
var log = services.GetRequiredService<ILogger<Program>>();
var configuration = services.GetRequiredService<IConfiguration>();

Snowflake? debugServer = null;

#if DEBUG
var debugServerString = configuration.GetValue<string?>("REMORA_DEBUG_SERVER");
if (debugServerString is not null)
{
    if (!DiscordSnowflake.TryParse(debugServerString, out debugServer))
    {
        log.LogWarning("Failed to parse debug server from environment");
    }
}
#endif

var slashService = services.GetRequiredService<SlashService>();

var checkSlashSupport = slashService.SupportsSlashCommands();
if (!checkSlashSupport.IsSuccess)
{
    log.LogWarning
    (
        "The registered commands of the bot don't support slash commands: {Reason}",
        checkSlashSupport.Error?.Message
    );
}
else
{
    var updateSlash = await slashService.UpdateSlashCommandsAsync(debugServer);
    if (!updateSlash.IsSuccess)
    {
        log.LogWarning("Failed to update slash commands: {Reason}", updateSlash.Error?.Message);
    }
}

await host.RunAsync();
