namespace SharedKernel.Hateoas.Models;

public record ResourceResponse<T>(T Data, IReadOnlyList<Link> Links);

public record CollectionResourceResponse<T>(
    IReadOnlyList<ResourceResponse<T>> Items,
    IReadOnlyList<Link> Links);