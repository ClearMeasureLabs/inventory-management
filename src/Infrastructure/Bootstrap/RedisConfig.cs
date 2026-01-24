namespace Bootstrap;

public class RedisConfig
{
    public string Host { get; set; } = string.Empty;

    public string Port { get; set; } = string.Empty;

    public string User { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string GetConnectionString()
    {
        var connectionString = $"{Host}:{Port}";
        
        if (!string.IsNullOrWhiteSpace(User))
        {
            connectionString += $",user={User}";
        }
        
        if (!string.IsNullOrWhiteSpace(Password))
        {
            connectionString += $",password={Password}";
        }
        
        connectionString += ",abortConnect=false";
        
        return connectionString;
    }
}
