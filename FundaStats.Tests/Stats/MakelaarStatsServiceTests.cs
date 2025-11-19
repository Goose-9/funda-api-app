using FundaStats.Core.Funda.Dtos;
using FundaStats.Core.Stats;

namespace FundaStats.Tests.Stats;

public class MakelaarStatsServiceTests
{
    private readonly MakelaarStatsService _service = new();

    [Fact]
    public void GroupsAndSortsMakelaarsByObjectCount()
    {
        var objects = new List<FundaObjectDto>
        {
            new() { MakelaarId = 1, MakelaarNaam = "A" },
            new() { MakelaarId = 1, MakelaarNaam = "A" },
            new() { MakelaarId = 2, MakelaarNaam = "B" },
            new() { MakelaarId = 3, MakelaarNaam = "C" },
            new() { MakelaarId = 3, MakelaarNaam = "C" },
            new() { MakelaarId = 3, MakelaarNaam = "C" },
        };

        var result = _service.GetTopMakelaarByObjectCount(objects, topN: 10);

        Assert.Equal(3, result.Count);

        var first = result[0];
        Assert.Equal(3, first.ObjCount);
        Assert.Equal(3, first.MakelaarId);

        var second = result[1];
        Assert.Equal(2, second.ObjCount);
        Assert.Equal(1, second.MakelaarId);

        var third = result[2];
        Assert.Equal(1, third.ObjCount);
        Assert.Equal(2, third.MakelaarId);
    }

    [Fact]
    public void ReturnsOnlyTopNResults()
    {
        var objects = new List<FundaObjectDto>
        {
            new() { MakelaarId = 1, MakelaarNaam = "A" },
            new() { MakelaarId = 1, MakelaarNaam = "A" },
            new() { MakelaarId = 2, MakelaarNaam = "B" },
            new() { MakelaarId = 2, MakelaarNaam = "B" },
            new() { MakelaarId = 3, MakelaarNaam = "C" },
            new() { MakelaarId = 3, MakelaarNaam = "C" },
        };

        var result = _service.GetTopMakelaarByObjectCount(objects, topN: 2);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void IgnoresObjectsWithMakelaarIdZero()
    {
        var objects = new List<FundaObjectDto>
        {
            new() { MakelaarId = 0, MakelaarNaam = "" },
            new() { MakelaarId = 1, MakelaarNaam = "Valid" },
            new() { MakelaarId = 1, MakelaarNaam = "Valid" },
        };

        var result = _service.GetTopMakelaarByObjectCount(objects, topN: 10);

        Assert.Single(result);
        Assert.Equal(1, result[0].MakelaarId);
        Assert.Equal(2, result[0].ObjCount);
    }

    [Fact]
    public void IgnoresObjectsWithEmptyMakelaarNaam()
    {
        var objects = new List<FundaObjectDto>
        {
            new() { MakelaarId = 1, MakelaarNaam = "Valid" },
            new() { MakelaarId = 1, MakelaarNaam = "Valid" },
            new() { MakelaarId = 2, MakelaarNaam = "" },
        };

        var result = _service.GetTopMakelaarByObjectCount(objects, topN: 10);

        Assert.Single(result);
        Assert.Equal(1, result[0].MakelaarId);
        Assert.Equal(2, result[0].ObjCount);
    }
}
