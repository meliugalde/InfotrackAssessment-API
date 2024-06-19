
using Infotrack.Domain.Interfaces;
using Infotrack.Domain.Models.Entities;
using HtmlAgilityPack;

namespace Infotrack.Domain.Services;
public class SearchService : ISearchService
{
    private readonly IHttpClientFactory _clientFactory;
    private readonly ISearchRepository _searchesRepository;

    private const int SEARCH_NUM = 100;

    HttpClient _httpClient => _clientFactory.CreateClient("Client_A");

    public SearchService(IHttpClientFactory clientFactory, ISearchRepository searchesRepository)
    {
        _clientFactory = clientFactory;
        _searchesRepository = searchesRepository;
    }

    public async Task<List<string>> FindUrlPositionAsync(string keywords, string targetUrl)
    {
        var searchUrl = $"search?num={SEARCH_NUM}&q={keywords}";
        var response = await _httpClient.GetStringAsync(searchUrl);

        return GetUrlPosition(response, targetUrl);
    }

    private List<string> GetUrlPosition(string htmlContent, string targetUrl)
    {
            var doc = new HtmlDocument();
            doc.LoadHtml(htmlContent);

            var nodes = doc.DocumentNode.SelectNodes("//div[@class='egMi0 kCrYT']//a[@href]");

            if (nodes == null)
            {
                return new List<string>(); // No results found
            }

            var urls = nodes.Select(node => node.GetAttributeValue("href", string.Empty)).ToList();

            List<string> positions = urls
            .Select((url, index) => new { url, index })
            .Where(x => x.url.Contains(targetUrl, StringComparison.OrdinalIgnoreCase))
            .Select(x => (x.index + 1).ToString())
            .ToList();

            return positions;
    }

    public async Task SaveSearch(Search search)
    {
       await _searchesRepository.AddSearchHistoryAsync(search);
    }

    public async Task<IEnumerable<Search>> GetSearchHistoryAsync()
    {
        return await _searchesRepository.GetSearchHistoryAsync();
    }

    

    public void Dispose()
    {
        _searchesRepository.Dispose();
    }
}