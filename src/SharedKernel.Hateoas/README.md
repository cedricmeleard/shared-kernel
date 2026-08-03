# SharedKernel.Hateoas

`SharedKernel.Hateoas` provides lightweight hypermedia containers (`ResourceResponse<T>`, `CollectionResourceResponse<T>`, `Link`) and extension methods to attach HATEOAS navigational links to REST API responses.

---

## 🚀 Features

- **`Link`**: Represents a hypermedia link (`Href`, `Rel`, `Method`).
- **`ResourceResponse<T>`**: Wraps a single DTO or entity in a hypermedia container with navigation links.
- **`CollectionResourceResponse<T>`**: Wraps a collection of items along with collection-level links.
- **`ToResource()` & `ToCollectionResource()`**: Extension methods for converting raw DTOs to hypermedia resources.

---

## 💻 Usage Examples

### 1. Wrapping Single Item in Resource

```csharp
using Hateoas.Extensions;
using Hateoas.Models;

public static class UserLinkFactory
{
    public static List<Link> CreateUserLinks(HttpContext context, LinkGenerator generator, Guid userId) =>
    [
        new(generator.GetUriByName(context, "GetUserById", new { id = userId })!, "self", "GET"),
        new(generator.GetUriByName(context, "UpdateUser", new { id = userId })!, "update-user", "PUT"),
        new(generator.GetUriByName(context, "DeleteUser", new { id = userId })!, "delete-user", "DELETE")
    ];
}

// In API Endpoint:
app.MapGet("api/users/{id}", (Guid id, HttpContext context, LinkGenerator generator) =>
{
    var user = new UserDto(id, "Alice");
    var links = UserLinkFactory.CreateUserLinks(context, generator, id);
    return TypedResults.Ok(user.ToResource(links));
});
```

### 2. Wrapping Collection Resource

```csharp
app.MapGet("api/users", (HttpContext context, LinkGenerator generator) =>
{
    var users = GetUsers();
    var collectionLinks = new List<Link> { new(context.Request.Path, "self", "GET") };

    var collectionResource = users.ToCollectionResource(
        user => UserLinkFactory.CreateUserLinks(context, generator, user.Id),
        collectionLinks);

    return TypedResults.Ok(collectionResource);
});
```
