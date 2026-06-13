using Rhizine.Aspire.ApiService.Models;
using System.Linq;
using Xunit;

namespace Rhizine.Tests.UnitTests.Aspire
{
    public class PaginatedListTests
    {
        [Fact]
        public async Task CreateAsync_ShouldCreatePaginatedList()
        {
            // Arrange
            var data = Enumerable.Range(1, 100).ToAsyncEnumerable();
            var pageIndex = 2;
            var pageSize = 10;

            // Act
            var paginatedList = await PaginatedList<int>.CreateAsync(data, pageIndex, pageSize);

            // Assert
            Assert.Equal(pageSize, paginatedList.Items.Count);
            Assert.Equal(pageIndex, paginatedList.PageIndex);
            Assert.Equal(10, paginatedList.TotalPages);
            Assert.Equal(100, paginatedList.TotalCount);
            Assert.Equal(await data.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync(), paginatedList.Items);
        }

        [Fact]
        public async Task Add_ShouldAddItemToList()
        {
            // Arrange
            var data = Enumerable.Range(1, 10).ToList();
            var newItem = 11;
            data.Add(newItem);
            var dataQueryable = data.ToAsyncEnumerable();

            // Act
            var paginatedList = await PaginatedList<int>.CreateAsync(dataQueryable, 1, 11);

            // Assert
            Assert.Contains(newItem, paginatedList.Items);
        }

        [Fact]
        public async Task GetItemAt_ShouldReturnCorrectItem()
        {
            // Arrange
            var data = Enumerable.Range(1, 10).ToAsyncEnumerable();
            var pageIndex = 1;
            var pageSize = 10;
            var paginatedList = await PaginatedList<int>.CreateAsync(data, pageIndex, pageSize);

            // Act
            var item = paginatedList.GetItemAt(5);

            // Assert
            Assert.Equal(6, item);
        }

        [Fact]
        public async Task Contains_ShouldReturnTrueIfItemExists()
        {
            // Arrange
            var data = Enumerable.Range(1, 10).ToAsyncEnumerable();
            var pageIndex = 1;
            var pageSize = 10;
            var paginatedList = await PaginatedList<int>.CreateAsync(data, pageIndex, pageSize);

            // Act
            var contains = paginatedList.Contains(5);

            // Assert
            Assert.True(contains);
        }

        [Fact]
        public async Task IndexOf_ShouldReturnCorrectIndex()
        {
            // Arrange
            var data = Enumerable.Range(1, 10).ToAsyncEnumerable();
            var pageIndex = 1;
            var pageSize = 10;
            var paginatedList = await PaginatedList<int>.CreateAsync(data, pageIndex, pageSize);

            // Act
            var index = paginatedList.IndexOf(5);

            // Assert
            Assert.Equal(4, index);
        }
        [Fact]
        public async Task Sort_ShouldSortItems()
        {
            // Arrange
            var data = Enumerable.Range(1, 10).Reverse().ToAsyncEnumerable();
            var pageIndex = 1;
            var pageSize = 10;
            var paginatedList = await PaginatedList<int>.CreateAsync(data, pageIndex, pageSize);

            // Act
            paginatedList.Sort((a, b) => a.CompareTo(b));

            // Assert
            Assert.Equal(Enumerable.Range(1, 10).ToList(), paginatedList.Items);
        }

        [Fact]
        public async Task Filter_ShouldFilterItems()
        {
            // Arrange
            var data = Enumerable.Range(1, 10).ToAsyncEnumerable();
            var pageIndex = 1;
            var pageSize = 10;
            var paginatedList = await PaginatedList<int>.CreateAsync(data, pageIndex, pageSize);

            // Act
            var filteredItems = paginatedList.Filter(i => i % 2 == 0);

            // Assert
            Assert.Equal(new[] { 2, 4, 6, 8, 10 }, filteredItems);
        }

        [Fact]
        public async Task Clear_ShouldClearItems()
        {
            // Arrange
            var data = Enumerable.Range(1, 10).ToAsyncEnumerable();
            var pageIndex = 1;
            var pageSize = 10;
            var paginatedList = await PaginatedList<int>.CreateAsync(data, pageIndex, pageSize);

            // Act
            paginatedList.Clear();

            // Assert
            Assert.Empty(paginatedList.Items);
        }

        [Fact]
        public async Task RemoveAt_ShouldRemoveItemAtGivenIndex()
        {
            // Arrange
            var data = Enumerable.Range(1, 10).ToAsyncEnumerable();
            var pageIndex = 1;
            var pageSize = 10;
            var paginatedList = await PaginatedList<int>.CreateAsync(data, pageIndex, pageSize);

            // Act
            paginatedList.RemoveAt(0);

            // Assert
            Assert.Equal(Enumerable.Range(2, 9).ToList(), paginatedList.Items);
        }

        [Fact]
        public async Task Add_ShouldAddItem()
        {
            // Arrange
            var data = Enumerable.Range(1, 10).ToAsyncEnumerable();
            var pageIndex = 1;
            var pageSize = 10;
            var paginatedList = await PaginatedList<int>.CreateAsync(data, pageIndex, pageSize);

            // Act
            paginatedList.Add(11);

            // Assert
            Assert.Equal(Enumerable.Range(1, 11).ToList(), paginatedList.Items);
        }

        [Fact]
        public async Task Insert_ShouldInsertItemAtGivenIndex()
        {
            // Arrange
            var data = Enumerable.Range(1, 10).ToAsyncEnumerable();
            var pageIndex = 1;
            var pageSize = 10;
            var paginatedList = await PaginatedList<int>.CreateAsync(data, pageIndex, pageSize);

            // Act
            paginatedList.Insert(0, 0);

            // Assert
            Assert.Equal(new[] { 0 }.Concat(Enumerable.Range(1, 10)).ToList(), paginatedList.Items);
        }

        [Fact]
        public async Task Remove_ShouldRemoveItem()
        {
            // Arrange
            var data = Enumerable.Range(1, 10).ToAsyncEnumerable();
            var pageIndex = 1;
            var pageSize = 10;
            var paginatedList = await PaginatedList<int>.CreateAsync(data, pageIndex, pageSize);

            // Act
            var removed = paginatedList.Remove(1);

            // Assert
            Assert.True(removed);
            Assert.Equal(Enumerable.Range(2, 9).ToList(), paginatedList.Items);
        }

        [Fact]
        public async Task CopyTo_ShouldCopyItemsToArray()
        {
            // Arrange
            var data = Enumerable.Range(1, 10).ToAsyncEnumerable();
            var pageIndex = 1;
            var pageSize = 10;
            var paginatedList = await PaginatedList<int>.CreateAsync(data, pageIndex, pageSize);
            var array = new int[10];

            // Act
            paginatedList.CopyTo(array, 0);

            // Assert
            Assert.Equal(Enumerable.Range(1, 10).ToArray(), array);
        }

        [Fact]
        public async Task ToArray_ShouldReturnArray()
        {
            // Arrange
            var data = Enumerable.Range(1, 10).ToAsyncEnumerable();
            var pageIndex = 1;
            var pageSize = 10;
            var paginatedList = await PaginatedList<int>.CreateAsync(data, pageIndex, pageSize);

            // Act
            var array = paginatedList.ToArray();

            // Assert
            Assert.Equal(Enumerable.Range(1, 10).ToArray(), array);
        }

        [Fact]
        public async Task ToList_ShouldReturnList()
        {
            // Arrange
            var data = Enumerable.Range(1, 10).ToAsyncEnumerable();
            var pageIndex = 1;
            var pageSize = 10;
            var paginatedList = await PaginatedList<int>.CreateAsync(data, pageIndex, pageSize);

            // Act
            var list = paginatedList.ToList();

            // Assert
            Assert.Equal(Enumerable.Range(1, 10).ToList(), list);
        }
    }
}
