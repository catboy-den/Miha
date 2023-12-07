using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Miha.Redis.Documents;
using Miha.Redis.Repositories.Interfaces;

namespace Miha.Redis.Services;

public class RedisSeedService
{
    private readonly RedisSeedOptions _seedOptions;
    private readonly IGuildRepository _guildRepository;
    private readonly ILogger<RedisSeedService> _logger;

    public RedisSeedService(
        IOptions<RedisSeedOptions> seedOptions,
        IGuildRepository guildRepository,
        ILogger<RedisSeedService> logger)
    {
        _seedOptions = seedOptions.Value;
        _guildRepository = guildRepository;
        _logger = logger;
    }

    public async Task SeedGuildAsync(ulong? guildId)
    {
        if (_seedOptions.GuildOptions is null)
        {
            _logger.LogDebug("Seed options are null, skipping seeding");
            return;
        }
        
        if (guildId is null)
        {
            throw new ArgumentNullException(nameof(guildId), "A guildId is required for guild document seeding");
        }
        
        var existingGuildDoc = await _guildRepository.GetAsync(guildId.Value);

        if (existingGuildDoc is not null)
        {
            _logger.LogInformation("Guild document already exists, no guild document seeding is required");
            return;
        }

        _logger.LogInformation("Seeding guild document with configured seed guild options {@GuildOptions}", _seedOptions.GuildOptions);
        
        var guildDoc = new GuildDocument
        {
            Id = guildId.Value,
            AnnouncementChannel = _seedOptions.GuildOptions.AnnouncementChannel,
            AnnouncementRoleId = _seedOptions.GuildOptions.AnnouncementRoleId,
            BirthdayAnnouncementChannel = _seedOptions.GuildOptions.BirthdayAnnouncementChannel,
            WeeklyScheduleChannel = _seedOptions.GuildOptions.WeeklyScheduleChannel,
            LogChannel = _seedOptions.GuildOptions.LogChannel
        };

        var newGuildDoc = await _guildRepository.UpsertAsync(guildDoc);
        
        _logger.LogInformation("Guild document seeded {@GuildDocument}", newGuildDoc);
    }
}
