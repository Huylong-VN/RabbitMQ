namespace RabbitMQ.Service;
public class AppSettings
{
    public MetaData MetaData { get; set; } = default!;

    public ConnectionStrings ConnectionStrings { get; set; } = default!;

    public RedisSettings? RedisTracking { get; set; }

    public RedisSettings? GlobalCache { get; set; }

    public RabbitMQSettings? RabbitMQ { get; set; } = default!;
    public List<ScheduledTask> ScheduledTasks { get; set; }

    public TokenOption? TokenOption { get; set; }

    public string? GoogleSheetCredentials { get; set; }

    public Configurations? Configurations { get; set; }

    public Hangfire? Hangfire { get; set; }

    public string? RootUrl { get; set; }

    public string? WebsiteUrl { get; set; }

    public GoogleAnalyticsConfig? GoogleAnalytics { get; set; }
}

public class ConnectionStrings
{
    public string DefaultConnection { get; set; } = default!;
}

public class MetaData
{
    public string Name { get; set; }

    public string Title { get; set; }

    public string Version { get; set; }
}

public class Hangfire
{
    public string UserName { get; set; }

    public string Password { get; set; }
}

public class Configurations
{
    public string GoogleSheetSourcePath { get; set; }
}

public class GoogleAnalyticsConfig
{
    public string Property { get; set; }
}
public class RedisSettings
{
    public string Url { get; set; }
}

public class TokenOption
{
    public string ServerSigningPassword { get; set; }

    public double AccessTokenDurationInMinutes { get; set; }

    public int RefreshTokenDurationInDays { get; set; }

    public string Issuer { get; set; }

    public List<string> AudienceList { get; set; }
}
public class ScheduledTask
{
    public string Id { get; set; }

    public string Title { get; set; }

    public string Detail { get; set; }

    public string CronExpression { get; set; }

    public DateTime? LastExecution { get; set; }

    public DateTime? NextExecution { get; set; }

    public string Message { get; set; }
}