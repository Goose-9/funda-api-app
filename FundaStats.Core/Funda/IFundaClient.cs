using FundaStats.Core.Funda.Dtos;

namespace FundaStats.Core.Funda;

public interface IFundaClient
{
    Task<IReadOnlyList<FundaObjectDto>> GetAllObjectsAsync(
        FundaSearchQuery query,
        CancellationToken token = default
    );
}
