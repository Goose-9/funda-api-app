using System.Net;
using System.Net.Http.Json;
using FundaStats.Core.Funda.Dtos;

namespace FundaStats.Core.Funda;

public sealed class FundaClient : IFundaClient
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public FundaClient(HttpClient httpClient, string apiKey)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
    }

    public async Task<IReadOnlyList<FundaObjectDto>> GetAllObjectsAsync(
        FundaSearchQuery query,
        CancellationToken token = default
    )
    {
        if (query == null)
            throw new ArgumentNullException(nameof(query));

        var allObjects = new List<FundaObjectDto>();

        int currentPage = 1;
        int totalPages = 1;

        do
        {
            var url = BuildUrl(query, currentPage);

            using var request = new HttpRequestMessage(HttpMethod.Get, url);

            using var response = await _httpClient.GetAsync(url, token);

            // I thought it would return 429 but it appears to be 401 in testing
            if (response.StatusCode == (HttpStatusCode)401)
            {
                // request cap reached
                throw new HttpRequestException(
                    "Received 401 Unauthorized from Funda API. "
                        + "The usage limit has been exceeded. "
                        + "Please try again later."
                );
            }

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"Funda API request failed: {(int)response.StatusCode} {response.ReasonPhrase}"
                );
            }

            var dto =
                await response.Content.ReadFromJsonAsync<FundaResponseDto>(token)
                ?? throw new InvalidOperationException("Failed to deserialize Funda response.");

            if (dto.Objects is not null)
            {
                allObjects.AddRange(dto.Objects);
            }

            totalPages = dto.Paging.AantalPaginas;
            currentPage++;

            // await Task.Delay(600, token);
        } while (currentPage <= totalPages);

        return allObjects;
    }

    private string BuildUrl(FundaSearchQuery query, int page)
    {
        // Example:
        // http://partnerapi.funda.nl/feeds/Aanbod.svc/json/{key}/?type=koop&zo=/amsterdam/&page=1&pagesize=25
        var uri =
            $"feeds/Aanbod.svc/json/{_apiKey}/"
            // + $"?type={Uri.EscapeDataString(query.Type)}"
            // + $"&zo={Uri.EscapeDataString(query.Path)}"
            + $"?type={query.Type}"
            + $"&zo={query.Path}"
            + $"&page={page}"
            + $"&pagesize={query.PageSize}";

        return uri;
    }
}
