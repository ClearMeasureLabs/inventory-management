namespace Bootstrap;

public class RabbitMQConfig
{
    public string Host { get; set; } = string.Empty;

    public string Port { get; set; } = string.Empty;

    public string User { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string GetConnectionString()
    {
        return $"amqps://{User}:{Password}@{Host}:{Port}";
    }
}
