namespace FundaStats.Core.Funda.Dtos;

public sealed class PagingInfo
{
    public int AantalPaginas { get; set; }
    public int HuidigePagina { get; set; }
    public string? VolgendeUrl { get; set; }
    public string? VorigeUrl { get; set; }
}
