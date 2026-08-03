using FluentAssertions;
using Hateoas.Extensions;
using Hateoas.Models;
using Xunit;

namespace SharedKernel.UnitTests.Hateoas;

public record SampleDataDto(Guid Id, string Name);

public class HateoasTests
{
    [Fact]
    public void ToResource_Should_WrapData_And_AttachLinks()
    {
        // Arrange
        var data = new SampleDataDto(Guid.NewGuid(), "Sample Item");
        var links = new List<Link>
        {
            new("https://localhost/api/items/1", "self", "GET"),
            new("https://localhost/api/items/1", "delete", "DELETE")
        };

        // Act
        var resource = data.ToResource(links);

        // Assert
        resource.Should().NotBeNull();
        resource.Data.Name.Should().Be("Sample Item");
        resource.Links.Should().HaveCount(2);
        resource.Links.Should().Contain(l => l.Rel == "self" && l.Method == "GET");
    }

    [Fact]
    public void ToCollectionResource_Should_WrapItems_And_AttachCollectionLinks()
    {
        // Arrange
        var items = new List<SampleDataDto>
        {
            new(Guid.NewGuid(), "Item 1"),
            new(Guid.NewGuid(), "Item 2")
        };

        var collectionLinks = new List<Link>
        {
            new("https://localhost/api/items", "self", "GET")
        };

        // Act
        var collection = items.ToCollectionResource(
            item => [new Link($"https://localhost/api/items/{item.Id}", "self", "GET")],
            collectionLinks);

        // Assert
        collection.Should().NotBeNull();
        collection.Items.Should().HaveCount(2);
        collection.Links.Should().HaveCount(1);
    }
}
