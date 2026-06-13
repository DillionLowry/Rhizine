using Xunit;
using NSubstitute;
using Rhizine.Core.Services;
using Rhizine.Core.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Text;

namespace Rhizine.Tests.UnitTests.Core.Services
{
    public class CachingServiceTests
    {
        private readonly ILoggingService _loggingService;
        private readonly IMemoryCache _memoryCache;
        private readonly IDistributedCache _distributedCache;
        private readonly CachingService _cachingService;

        public CachingServiceTests()
        {
            _loggingService = Substitute.For<ILoggingService>();
            _memoryCache = Substitute.For<IMemoryCache>();
            _distributedCache = Substitute.For<IDistributedCache>();
            _cachingService = new CachingService(_loggingService, _memoryCache, _distributedCache);
        }

        [Fact]
        public async void GetAsync_ItemExistsInMemoryCache_ShouldReturnItem()
        {
            // Arrange
            var key = "TestKey";
            var expectedValue = "TestValue";
            _memoryCache.TryGetValue(key, out Arg.Any<string>()).Returns(x =>
            {
                x[1] = expectedValue;
                return true;
            });

            // Act
            var value = await _cachingService.GetAsync<string>(key);

            // Assert
            Assert.Equal(expectedValue, value);
        }

        [Fact]
        public async void SetAsync_ShouldSetItemInMemoryCache()
        {
            // Arrange
            var key = "TestKey";
            var expectedValue = "TestValue";

            // Act
            await _cachingService.SetAsync(key, expectedValue);

            // Assert
            _memoryCache.Received().Set(key, expectedValue, Arg.Any<MemoryCacheEntryOptions>());
        }

        [Fact]
        public async void ExistsAsync_ItemExistsInMemoryCache_ShouldReturnTrue()
        {
            // Arrange
            var key = "TestKey";
            _memoryCache.TryGetValue(key, out Arg.Any<string>()).Returns(true);

            // Act
            var exists = await _cachingService.ExistsAsync(key);

            // Assert
            Assert.True(exists);
        }

        [Fact]
        public async void RemoveAsync_ItemExistsInMemoryCache_ShouldRemoveItem()
        {
            // Arrange
            var key = "TestKey";

            // Act
            await _cachingService.RemoveAsync(key);

            // Assert
            _memoryCache.Received().Remove(key);
        }
        [Fact]

        public async void GetAsync_ItemExistsInDistributedCache_ShouldReturnItem()
        {
            // Arrange
            var key = "TestKey";
            var expectedValue = "TestValue";
            _memoryCache.TryGetValue(key, out Arg.Any<string>()).Returns(false);
            _distributedCache.GetAsync(key, Arg.Any<CancellationToken>()).Returns(Task.FromResult(Encoding.UTF8.GetBytes(expectedValue)));

            // Act
            var value = await _cachingService.GetAsync<string>(key);

            // Assert
            Assert.Equal(expectedValue, value);
        }

        [Fact]
        public async void SetAsync_ShouldSetItemInDistributedCache()
        {
            // Arrange
            var key = "TestKey";
            var expectedValue = "TestValue";

            // Act
            await _cachingService.SetAsync(key, expectedValue);

            // Assert
            _distributedCache.Received().SetAsync(key, Arg.Any<byte[]>(), Arg.Any<DistributedCacheEntryOptions>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async void ExistsAsync_ItemExistsInDistributedCache_ShouldReturnTrue()
        {
            // Arrange
            var key = "TestKey";
            _memoryCache.TryGetValue(key, out Arg.Any<string>()).Returns(false);
            _distributedCache.GetAsync(key, Arg.Any<CancellationToken>()).Returns(Task.FromResult(Encoding.UTF8.GetBytes("TestValue")));

            // Act
            var exists = await _cachingService.ExistsAsync(key);

            // Assert
            Assert.True(exists);
        }

        [Fact]
        public async void RemoveAsync_ItemExistsInDistributedCache_ShouldRemoveItem()
        {
            // Arrange
            var key = "TestKey";

            // Act
            await _cachingService.RemoveAsync(key);

            // Assert
            _distributedCache.Received().RemoveAsync(key, Arg.Any<CancellationToken>());
        }
    }
}
