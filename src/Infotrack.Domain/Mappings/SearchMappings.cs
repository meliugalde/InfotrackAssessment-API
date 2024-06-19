using Infotrack.Domain.Models.Dtos;
using Infotrack.Domain.Models.Entities;

namespace Infotrack.Domain.Models.Dtos;

public static class SearchMappings
{
    public static Search ToEntity(this SearchDto searchDto)
    {
        return new Search()
        {
            Id = searchDto.Id,
            Keywords = searchDto.Keywords,
            Url = searchDto.Url,
            Positions = searchDto.Positions,
            SearchDate = searchDto.SearchDate,
        };
    }

    public static SearchDto ToDto(this Search search)
    {
        return new SearchDto(search.Id, search.Url,search.Keywords,search.Positions,search.SearchDate);
    }
}
