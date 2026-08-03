# SharedKernel.Authentication

`SharedKernel.Authentication` provides a vendor-agnostic user context abstraction (`IUserContext`) and JWT Bearer security extension methods for ASP.NET Core applications.

---

## 🚀 Features

- **`IUserContext`**: Decoupled interface to access current authenticated user information (`UserId`, `IsAuthenticated`, claims).
- **`UserContext`**: ASP.NET Core `IHttpContextAccessor` implementation of `IUserContext`.
- **`AddJwtAuthentication()`**: Extension method for setting up JWT Bearer authentication with token validation options.

---

## 💻 Usage Examples

### 1. Registering JWT Authentication

```csharp
// appsettings.json
{
  "Jwt": {
    "Issuer": "https://auth.example.com",
    "Audience": "barbecueroad-api",
    "Secret": "super-secret-key-32-characters-minimum!"
  }
}

// Program.cs
builder.Services.AddJwtAuthentication(builder.Configuration);
```

### 2. Injecting IUserContext in Application Services

```csharp
public class OrderService(IUserContext userContext, IOrderRepository repository)
{
    public async Task<Result> PlaceOrderAsync(CancellationToken cancellationToken)
    {
        if (!userContext.IsAuthenticated)
        {
            return Result.Failure(new Error("Auth.Required", "User must be authenticated."));
        }

        var userId = userContext.UserId;
        // Proceed with order for userId...
        return Result.Success();
    }
}
```
