using Infotrack.Domain.Interfaces;
using Infotrack.Domain.Models.Entities;
using Infotrack.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infotrack.Infrastructure.Repositories;

public class SearchRepository : ISearchRepository
{
    private readonly InfotrackDBContext _context;
    public SearchRepository(InfotrackDBContext context)
    {
        this._context = context;
    }

    public async Task<int> AddSearchHistoryAsync(Search search)
    {
        _context.Searches.Add(search);
        return await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Search>> GetSearchHistoryAsync()
    {
        return await _context.Searches.ToListAsync();
    }
    
    public void Dispose()
    {
        _context.Dispose();
    }
}
