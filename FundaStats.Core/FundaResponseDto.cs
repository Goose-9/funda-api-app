namespace FundaStats.Core;

public sealed class FundaResponseDto
{
    public int TotaalAantalObjecteren { get; set; }
    public required PagingInfo Paging { get; set; }
}
