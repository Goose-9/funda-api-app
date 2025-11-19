using DotNetEnv;
using FundaStats.Core.Funda;
using FundaStats.Core.Stats;

Env.Load("FundaStats.App/.env");

string? apiKey = Environment.GetEnvironmentVariable("FUNDA_API_KEY");

if (string.IsNullOrWhiteSpace(apiKey))
{
    Console.WriteLine("Error: FUNDA_API_KEY not found in .env file.");
    Console.WriteLine("Please add a .env file with the key in FundaStats.App directory");
    return;
}

Console.WriteLine("Loading Funda API client...");

using var httpClient = new HttpClient { BaseAddress = new Uri("http://partnerapi.funda.nl/") };

var fundaClient = new FundaClient(httpClient, apiKey);
var statsService = new MakelaarStatsService();

Console.WriteLine("Fetching Funda data... this may take a while.\n");

// Extra Parameters

string queryCity = "/amsterdam/";
string queryType = "koop";
int pageSize = 25;
int topN = 10;

try
{
    // 1) Amsterdam query
    var amsterdamQuery = new FundaSearchQuery(queryCity, queryType, pageSize);
    var amsterdamObjects = await fundaClient.GetAllObjectsAsync(amsterdamQuery);

    Console.WriteLine($"Amsterdam: fetched {amsterdamObjects.Count} objects.");

    var topAmsterdam = statsService.GetTopMakelaarByObjectCount(amsterdamObjects, topN);
    PrintTopMakelaarsTable("Top 10 makelaars in Amsterdam (koop)", topAmsterdam);

    // 2) Amsterdam tuin query
    var amsterdamTuinQuery = new FundaSearchQuery($"{queryCity}tuin/", queryType, pageSize);
    var amsterdamTuinObjects = await fundaClient.GetAllObjectsAsync(amsterdamTuinQuery);

    Console.WriteLine($"\nAmsterdam + tuin: fetched {amsterdamTuinObjects.Count} objects.");

    var topAmsterdamTuin = statsService.GetTopMakelaarByObjectCount(amsterdamTuinObjects, topN);
    PrintTopMakelaarsTable("Top 10 makelaars in Amsterdam (koop + tuin)", topAmsterdamTuin);
}
catch (Exception e)
{
    Console.WriteLine("\n❌ An error occurred:");
    Console.WriteLine(e.Message);
}

// Code Below is from ChatGPT, purely to make the table look pretty in the console.

static void PrintTopMakelaarsTable(string title, IReadOnlyList<MakelaarStat> stats)
{
    Console.WriteLine();
    Console.WriteLine(title);
    Console.WriteLine(new string('-', title.Length));

    // Header
    Console.WriteLine("{0,-3} {1,-10} {2,-40} {3,8}", "#", "ID", "Makelaar", "Objects");

    // Rows
    var rank = 1;
    foreach (var m in stats)
    {
        Console.WriteLine(
            "{0,-3} {1,-10} {2,-40} {3,8}",
            rank,
            m.MakelaarId,
            Truncate(m.MakelaarNaam, 40),
            m.ObjCount
        );

        rank++;
    }
}

static string Truncate(string? value, int maxLength)
{
    if (string.IsNullOrEmpty(value))
        return string.Empty;
    return value.Length <= maxLength ? value : value[..(maxLength - 3)] + "...";
}
