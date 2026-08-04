using SharedKernel.Hateoas.Models;

namespace SharedKernel.Hateoas.Extensions;

public static class ResourceExtensions
{
    public static ResourceResponse<T> ToResource<T>(
        this T data,
        IEnumerable<Link> links) =>
        new(data, links.ToList());

    public static CollectionResourceResponse<T> ToCollectionResource<T>(
        this IEnumerable<T> items,
        Func<T, IEnumerable<Link>> itemLinkFactory,
        IEnumerable<Link> collectionLinks)
    {
        var resourceItems = items
            .Select(item => new ResourceResponse<T>(item, itemLinkFactory(item).ToList()))
            .ToList();

        return new CollectionResourceResponse<T>(resourceItems, collectionLinks.ToList());
    }
}