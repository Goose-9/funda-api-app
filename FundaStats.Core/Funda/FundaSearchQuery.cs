namespace FundaStats.Core.Funda;

public sealed class FundaSearchQuery
{
    public string Path { get; }
    public string Type { get; }
    public int PageSize { get; }

    public FundaSearchQuery(string path, string type = "koop", int pageSize = 25)
    {
        Path = path ?? throw new ArgumentNullException(nameof(path));
        Type = type ?? throw new ArgumentNullException(nameof(type));
        PageSize = pageSize;
    }
}
