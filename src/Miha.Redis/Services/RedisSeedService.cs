using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Miha.Redis.Documents;
using Miha.Redis.Repositories.Interfaces;

namespace Miha.Redis.Services;

public class RedisSeedService(
    IOptions<RedisOptions> redisOptions,
    IGuildRepository guildRepository,
    ILogger<RedisSeedService> logger)
{
    private readonly RedisSeedOptions _seedOptions = redisOptions.Value.SeedOptions;

    public async Task SeedGuildAsync(ulong? guildId)
    {
        if (_seedOptions.GuildOptions is null)
        {
            logger.LogDebug("Seed options are null, skipping seeding");
            return;
        }
        
        if (guildId is null)
        {
            throw new ArgumentNullException(nameof(guildId), "A guildId is required for guild document seeding");
        }
        
        var existingGuildDoc = await guildRepository.GetAsync(guildId.Value);

        if (existingGuildDoc is not null)
        {
            logger.LogInformation("Guild document already exists, no guild document seeding is required");
            return;
        }

        logger.LogInformation("Seeding guild document with configured seed guild options {@GuildOptions}", _seedOptions.GuildOptions);
        
        var guildDoc = new GuildDocument
        {
            Id = guildId.Value,
            AnnouncementChannel = _seedOptions.GuildOptions.AnnouncementChannel,
            AnnouncementRoleId = _seedOptions.GuildOptions.AnnouncementRoleId,
            BirthdayAnnouncementChannel = _seedOptions.GuildOptions.BirthdayAnnouncementChannel,
            WeeklyScheduleChannel = _seedOptions.GuildOptions.WeeklyScheduleChannel,
            LogChannel = _seedOptions.GuildOptions.LogChannel
        };

        var newGuildDoc = await guildRepository.UpsertAsync(guildDoc);
        
        logger.LogInformation("Guild document seeded {@GuildDocument}", newGuildDoc);
    }
}
