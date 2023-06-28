namespace Miha.Redis;

public class RedisOptions
{
    public const string Section = "Redis";

    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 6379;
}
