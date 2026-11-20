using Npgsql;

namespace Friendsgiving.Api.Infrastructure;

public static class ConnectionStrings
{
    /// <summary>
    /// Reads <c>ConnectionStrings:DefaultConnection</c> (or <c>DATABASE_URL</c>). Railway hands out
    /// <c>postgres://user:pass@host:port/db</c> URLs, which Npgsql can't parse, so those are converted.
    /// </summary>
    public static string Resolve(IConfiguration config)
    {
        var raw = config.GetConnectionString("DefaultConnection")
            ?? config["DATABASE_URL"]
            ?? throw new InvalidOperationException(
                "No database configured. Set ConnectionStrings__DefaultConnection or DATABASE_URL.");

        return raw.StartsWith("postgres://") || raw.StartsWith("postgresql://") ? FromUrl(raw) : raw;
    }

    private static string FromUrl(string url)
    {
        var uri = new Uri(url);
        var credentials = uri.UserInfo.Split(':', 2);

        return new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.Port > 0 ? uri.Port : 5432,
            Database = uri.AbsolutePath.TrimStart('/'),
            Username = Uri.UnescapeDataString(credentials[0]),
            Password = credentials.Length > 1 ? Uri.UnescapeDataString(credentials[1]) : null,
        }.ConnectionString;
    }
}
