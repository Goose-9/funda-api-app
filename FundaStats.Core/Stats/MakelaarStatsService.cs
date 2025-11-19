using FundaStats.Core.Funda.Dtos;

namespace FundaStats.Core.Stats;

public sealed class MakelaarStatsService : IMakelaarStatsService
{
    public IReadOnlyList<MakelaarStat> GetTopMakelaarByObjectCount(
        IEnumerable<FundaObjectDto> objects,
        int topN
    )
    {
        return objects
            .Where(obj => !string.IsNullOrWhiteSpace(obj.MakelaarNaam))
            .GroupBy(obj => obj.MakelaarId)
            .Select(group => new MakelaarStat
            {
                MakelaarId = group.Key,
                MakelaarNaam = group.First().MakelaarNaam ?? "(Uknown)",
                ObjCount = group.Count(),
            })
            .OrderByDescending(stat => stat.ObjCount)
            .ThenBy(stat => stat.MakelaarNaam) // alphabetical because why not :)
            .Take(topN)
            .ToList();
    }
}
