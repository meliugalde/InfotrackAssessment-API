using Infotrack.Domain.Interfaces;
using Infotrack.Domain.Models.Entities;
using Infotrack.Domain.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;

namespace Infotrack.Tests;

    [TestFixture]
    public class SearchServiceTests : IDisposable
    {
        private SearchService _searchService;
        private Mock<ISearchRepository> _mockSearchRepository;
        private Mock<HttpMessageHandler> _httpMessageHandlerMock;

        [SetUp]
        public void SetUp()
        {
            _mockSearchRepository = new Mock<ISearchRepository>();
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);

            var httpClient = new HttpClient(_httpMessageHandlerMock.Object) {
                    BaseAddress = new Uri("https://www.testing.dev/")
                };

            var mockHttpClientFactory = new Mock<IHttpClientFactory>();

            mockHttpClientFactory.Setup(_ => _.CreateClient("Client_A")).Returns(httpClient);

            _searchService = new SearchService(mockHttpClientFactory.Object,_mockSearchRepository.Object);
        }

        [TearDown]
        public void Dispose(){
            _searchService.Dispose();
        }

        [Test]
        public async Task FindUrlPositionAsync_ReturnsCorrectPositions()
        {
            // Arrange
            var htmlContent = "<div class='egMi0 kCrYT'><a href='http://targeturl.com'></a></div>";
            var keywords = "test keywords";
            var targetUrl = "http://targeturl.com";

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent(htmlContent)
                });

            // Act
            var result = await _searchService.FindUrlPositionAsync(keywords, targetUrl);

            // Assert
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result.First(), Is.EqualTo("1"));
        }

        [Test]
        public async Task GetUrlPosition_ReturnsEmptyList_WhenNoUrlFound()
        {
            // Arrange
            var htmlContent = "<div class='no-results'></div>";
            var keywords = "test keywords";
            var targetUrl = "http://targeturl.com";

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent(htmlContent)
                });


            // Act
            var result = await _searchService.FindUrlPositionAsync(keywords, targetUrl);

            // Assert
            Assert.That(result, !Is.Null);
            Assert.That(result.Count, Is.EqualTo(0));
            
        }

        [Test]
        public async Task GetUrlPosition_ReturnsEmptyList_WhenUrlExistButDoNotMatch()
        {
            // Arrange
            var htmlContent = "<div class='egMi0 kCrYT'><a href='http://otherurl.com'></a></div>";
            var keywords = "test keywords";
            var targetUrl = "http://targeturl.com";

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent(htmlContent)
                });


            // Act
            var result = await _searchService.FindUrlPositionAsync(keywords, targetUrl);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task SaveSearch_CallsAddSearchHistoryAsync()
        {
            // Arrange
            var search = new Search { Id = Guid.NewGuid(), Url = "http://example.com", Keywords = "example", Positions = "1,2,3", SearchDate = DateTime.Now };

            // Act
            await _searchService.SaveSearch(search);

            // Assert
            _mockSearchRepository.Verify(repo => repo.AddSearchHistoryAsync(search), Times.Once);
        }

         [Test]
        public async Task GetSearchHistoryAsync_CallsGetSearchHistoryAsyncAndReturnsResults()
        {
            // Arrange
            var searchHistory = new List<Search>
            {
                new Search { Id = Guid.NewGuid(), Url = "http://example.com", Keywords = "example", Positions = "1,2,3", SearchDate = DateTime.Now }
            };

            _mockSearchRepository.Setup(repo => repo.GetSearchHistoryAsync()).ReturnsAsync(searchHistory);

            // Act
            var result = await _searchService.GetSearchHistoryAsync();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(1));
            Assert.That(result, Is.EqualTo(searchHistory));
            _mockSearchRepository.Verify(repo => repo.GetSearchHistoryAsync(), Times.Once);
        }
    }