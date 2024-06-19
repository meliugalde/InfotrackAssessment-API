namespace Infotrack.Domain.Models.Dtos;

public record class SearchDto(Guid Id, string Url, string Keywords, string Positions, DateTime SearchDate);
