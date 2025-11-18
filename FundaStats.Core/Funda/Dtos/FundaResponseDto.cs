namespace FundaStats.Core.Funda.Dtos;

public sealed class FundaResponseDto
{
    public int TotaalAantalObjecteren { get; set; }
    public required PagingInfo Paging { get; set; }
}
