using Infotrack.API.Controllers;
using Infotrack.Domain.Interfaces;
using Infotrack.Domain.Models.Dtos;
using Infotrack.Domain.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace Infotrack.Tests;

    [TestFixture]
    public class SearchControllerTests
    {
        private SearchController _controller;
        private Mock<ISearchService> _mockSearchService;
        private Mock<ILogger<SearchController>> _mockLogger;

        [SetUp]
        public void SetUp()
        {
            _mockSearchService = new Mock<ISearchService>();
            _mockLogger = new Mock<ILogger<SearchController>>();
            _controller = new SearchController(_mockSearchService.Object, _mockLogger.Object);
        }

        [Test]
        public async Task FindUrlPosition_ReturnsBadRequest_WhenKeywordsOrTargetUrlIsNullOrEmpty()
        {
            // Arrange
            string keywords = null;
            string targetUrl = null;
            var messageExpected = "Keywords and target URL must be provided.";

            // Act
            var result = await _controller.FindUrlPosition(keywords, targetUrl);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;
            Assert.That(badRequestResult?.Value, Is.EqualTo(messageExpected));
             _mockLogger.Verify(
                 x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Error),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == messageExpected),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));
        }

        [Test]
        public async Task FindUrlPosition_ReturnsNotFound_WhenPositionsListIsEmpty()
        {
            // Arrange
            string keywords = "test";
            string targetUrl = "http://example.com";
            _mockSearchService.Setup(s => s.FindUrlPositionAsync(keywords, targetUrl))
                .ReturnsAsync(new List<string>());

            // Act
            var result = await _controller.FindUrlPosition(keywords, targetUrl);

            // Assert
            Assert.IsInstanceOf<NotFoundObjectResult>(result);
            var notFoundResult = result as NotFoundObjectResult;
            Assert.That(notFoundResult.Value, Is.EqualTo("The URL was not found in the search results."));
           
           _mockLogger.Verify(
                 x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()));
        }

        [Test]
        public async Task FindUrlPosition_ReturnsOk_WhenPositionsListIsNotEmpty()
        {
            // Arrange
            string keywords = "test";
            string targetUrl = "http://example.com";
            var positions = new List<string> { "1", "2" };
            _mockSearchService.Setup(s => s.FindUrlPositionAsync(keywords, targetUrl))
                .ReturnsAsync(positions);
            var messageExpected = $"Successfully found {targetUrl} with keywords {keywords} .";

            // Act
            var result = await _controller.FindUrlPosition(keywords, targetUrl);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            var searchDto = okResult.Value.GetType().GetProperty("searchDto").GetValue(okResult.Value, null);
            Assert.IsNotNull(searchDto);
           _mockLogger.Verify(
                 x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == messageExpected),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));
        
        }

        [Test]
        public async Task FindUrlPosition_ReturnsInternalServerError_OnException()
        {
            // Arrange
            string keywords = "test";
            string targetUrl = "http://example.com";
            _mockSearchService.Setup(s => s.FindUrlPositionAsync(keywords, targetUrl))
                .ThrowsAsync(new Exception("Internal error"));

            // Act
            var result = await _controller.FindUrlPosition(keywords, targetUrl);

            // Assert
            Assert.IsInstanceOf<ObjectResult>(result);
            var objectResult = result as ObjectResult;
            Assert.That(objectResult.StatusCode, Is.EqualTo(500));
            Assert.That(objectResult.Value, Is.EqualTo("Internal server error."));
           _mockLogger.Verify(
                 x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Error),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        [Test]
        public async Task GetSearchHistory_ReturnsOkResult_WithSearchHistory()
        {
            // Arrange
            var searchHistory = new List<Search>
            {
                new Search { Id = Guid.NewGuid(), Url = "http://example.com", Keywords = "example", Positions = "1,2", SearchDate = DateTime.Now },
                new Search { Id = Guid.NewGuid(), Url = "http://another.com", Keywords = "another", Positions = "3,4", SearchDate = DateTime.Now }
            };

            _mockSearchService.Setup(service => service.GetSearchHistoryAsync()).ReturnsAsync(searchHistory);

            // Act
            var result = await _controller.GetSearchHistory();

            // Assert
            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult.StatusCode, Is.EqualTo(200));

            var historyDtos = okResult.Value as IEnumerable<SearchDto>;
            Assert.That(historyDtos, Is.Not.Null);
            Assert.That(historyDtos.Count(), Is.EqualTo(2));

             _mockLogger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Fetching url searches.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),Times.Once);
        }

        [Test]
        public async Task GetSearchHistory_ReturnsInternalServerError_OnException()
        {
            // Arrange
            _mockSearchService.Setup(s => s.GetSearchHistoryAsync())
                .ThrowsAsync(new Exception("Internal error"));

            // Act
            var result = await _controller.GetSearchHistory();

            // Assert
            Assert.IsInstanceOf<ObjectResult>(result);
            var objectResult = result as ObjectResult;
            Assert.That(objectResult.StatusCode, Is.EqualTo(500));
            Assert.That(objectResult.Value, Is.EqualTo("Internal server error."));

             _mockLogger.Verify(
                 x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Error),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        
        }
    }
