using Infotrack.Domain.Models.Entities;

namespace Infotrack.Domain.Interfaces;
public interface ISearchService : IDisposable
{
    Task<List<string>> FindUrlPositionAsync(string keywords, string targetUrl);
    Task<IEnumerable<Search>> GetSearchHistoryAsync();
    Task SaveSearch(Search search);
}