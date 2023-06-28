namespace Miha;

public class HealthOptions
{
    public const string Section = "Health";

    public int Port { get; set; } = 8000;
    public string Endpoint { get; set; } = "/healthz";
}
