namespace Bootstrap;

public class SqlServerConfig
{
    public string Host { get; set; } = string.Empty;

    public string Port { get; set; } = string.Empty;

    public string User { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string Database { get; set; } = string.Empty;

    public string GetConnectionString()
    {
        return $"Server={Host},{Port};Database={Database};User Id={User};Password={Password};TrustServerCertificate=True;Encrypt=True;";
    }
}
