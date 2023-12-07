using System.Reflection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Redis.OM;
using Redis.OM.Contracts;
using Redis.OM.Modeling;

namespace Miha.Redis.Services;

public partial class RedisIndexCreationService : IHostedService
{
    private readonly IRedisConnectionProvider _provider;
    private readonly ILogger<RedisIndexCreationService> _logger;

    public RedisIndexCreationService(
        IRedisConnectionProvider provider,
        ILogger<RedisIndexCreationService> logger)
    {
        _provider = provider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var indexableModels = Assembly.GetExecutingAssembly().GetTypes().Where(p => p.GetCustomAttribute(typeof(DocumentAttribute)) != null).ToList();

        LogInfoIndexableModels(indexableModels.Count);

        foreach (var model in indexableModels)
        {
            LogInfoIndexingType(model.Name);

            await _provider.Connection.CreateIndexAsync(model);
        }

        LogInfoIndexedModels();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    [LoggerMessage(EventId = 0, Level = LogLevel.Information, Message = "Indexing {indexableModelsCount} indexable-models in redis")]
    public partial void LogInfoIndexableModels(int indexableModelsCount);

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Indexing model {modelTypeName} in redis")]
    public partial void LogInfoIndexingType(string modelTypeName);

    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Finished indexing indexable-models for redis")]
    public partial void LogInfoIndexedModels();
}
