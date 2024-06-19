namespace Infotrack.Domain.Models.Entities;

public class Search
{
    public Guid Id { get; set; } 
    public string Keywords { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? Positions { get; set; }
    public DateTime SearchDate { get; set; }    
}