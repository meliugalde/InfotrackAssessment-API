using Infotrack.Domain.Interfaces;
using Infotrack.Domain.Models.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Infotrack.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SearchController : ControllerBase
    {
        private readonly ISearchService _searchService;
        private readonly ILogger<SearchController> _logger;

        public SearchController(ISearchService searchService, ILogger<SearchController> logger)
        {
            _searchService = searchService;
            _logger = logger;
        }

       [HttpPost("find-url-position")]
        public async Task<IActionResult> FindUrlPosition([FromQuery] string keywords, [FromQuery] string targetUrl)
        {
            _logger.LogInformation("Searching keywords and url.");

            if (string.IsNullOrEmpty(keywords) || string.IsNullOrEmpty(targetUrl))
            {
                 _logger.LogError("Keywords and target URL must be provided.");
        
                return BadRequest("Keywords and target URL must be provided.");
            }

            try
            {
                var positionsList = await _searchService.FindUrlPositionAsync(keywords, targetUrl);
                var positionsString =  string.Join( ",", positionsList);

                SearchDto searchDto = new SearchDto(Guid.NewGuid(), targetUrl, keywords, positionsString, DateTime.Now);

                await _searchService.SaveSearch(searchDto.ToEntity());

                if (positionsList.Count == 0)
                {
                     _logger.LogInformation("The URL {url} was not found in the search results with keywords {keywords} .", targetUrl, keywords);
        
                    return NotFound("The URL was not found in the search results.");
                }

                _logger.LogInformation("Successfully found {url} with keywords {keywords} .", targetUrl, keywords);
                return Ok(new { searchDto });
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "An error occurred while searching keyword and url.");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetSearchHistory()
        {
            _logger.LogInformation("Fetching url searches.");

            try
            {
                var history = await _searchService.GetSearchHistoryAsync();
                IEnumerable<SearchDto> historyDtos = history.Select(x => x.ToDto());

                return Ok(historyDtos);
            }
            catch(Exception ex)
            {
                 _logger.LogError(ex, "An error occurred while fetching search hitory.");
                return StatusCode(500, "Internal server error.");
            }
            
        }
    }
}
