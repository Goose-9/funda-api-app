using FundaStats.Core.Funda.Dtos;

namespace FundaStats.Core.Stats;

public interface IMakelaarStatsService
{
    IReadOnlyList<MakelaarStat> GetTopMakelaarByObjectCount(
        IEnumerable<FundaObjectDto> objects,
        int topN
    );
}
