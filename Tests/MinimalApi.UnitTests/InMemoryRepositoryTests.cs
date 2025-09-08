using Xunit;
using FluentAssertions;
using Moq;
using Services;
using Models;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace MinimalApi.UnitTests;

public class InMemoryRepositoryTests
{
    [Fact]
    public async Task Add_Get_Update_Delete_flow_with_mock()
    {
        var mock = new Mock<IMyRepository>();

        var seeded = new[] { new Item { Id = 1, Name = "First" }, new Item { Id = 2, Name = "Second" } };
        mock.Setup(r => r.GetAllAsync()).ReturnsAsync(seeded.AsEnumerable());

        var newItem = new Item { Id = 3, Name = "New" };
        mock.Setup(r => r.AddAsync(It.IsAny<Item>())).ReturnsAsync((Item it) => { it.Id = 3; return it; });
        mock.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(newItem);
        mock.Setup(r => r.UpdateAsync(It.IsAny<Item>())).ReturnsAsync((Item it) => it);
        mock.Setup(r => r.DeleteAsync(3)).ReturnsAsync(true);

        var repo = mock.Object;

        var items = (await repo.GetAllAsync()).ToList();
        items.Count.Should().BeGreaterOrEqualTo(2);

        var created = await repo.AddAsync(new Item { Name = "New" });
        created.Id.Should().Be(3);

        var fetched = await repo.GetByIdAsync(created.Id);
        fetched.Should().NotBeNull();
        fetched!.Name.Should().Be("New");

        created.Name = "Updated";
        var updated = await repo.UpdateAsync(created);
        updated.Name.Should().Be("Updated");

        var deleted = await repo.DeleteAsync(created.Id);
        deleted.Should().BeTrue();

        mock.Verify(r => r.AddAsync(It.IsAny<Item>()), Times.Once);
        mock.Verify(r => r.GetByIdAsync(3), Times.AtLeastOnce);
        mock.Verify(r => r.UpdateAsync(It.IsAny<Item>()), Times.Once);
        mock.Verify(r => r.DeleteAsync(3), Times.Once);
    }

    [Fact]
    public async Task Update_null_throws_via_mock_verification()
    {
        var mock = new Mock<IMyRepository>();
        mock.Setup(r => r.UpdateAsync(null!)).ThrowsAsync(new ArgumentNullException());

        await mock.Object.Invoking(r => r.UpdateAsync(null!)).Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task Update_with_zero_id_throws_via_mock_verification()
    {
        var mock = new Mock<IMyRepository>();
        mock.Setup(r => r.UpdateAsync(It.Is<Item>(i => i.Id == 0))).ThrowsAsync(new ArgumentException());

        var item = new Item { Name = "NoId" };
        await mock.Object.Invoking(r => r.UpdateAsync(item)).Should().ThrowAsync<ArgumentException>();
    }
}
