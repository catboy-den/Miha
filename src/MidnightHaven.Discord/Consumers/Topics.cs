namespace MidnightHaven.Discord.Consumers;

public static class Topics
{
    public static class GuildEvent
    {
        public const string Created = "guildevent-created";
        public const string Started = "guildevent-started";
        public const string Cancelled = "guildevent-cancelled";
        public const string Updated = "guildevent-updated";
    }
}
