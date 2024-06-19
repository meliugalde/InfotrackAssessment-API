using Infotrack.Domain.Models.Entities;

namespace Infotrack.Domain.Interfaces;

public interface ISearchRepository : IDisposable
{
    Task<IEnumerable<Search>> GetSearchHistoryAsync();
    Task<int> AddSearchHistoryAsync(Search search);
}
