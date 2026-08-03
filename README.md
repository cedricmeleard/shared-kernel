# SharedKernel

Reusable .NET 10 Shared Kernel building blocks for **BarbecueRoad** and **Patientelle**.

## 📦 Published Packages

- **`SharedKernel.Results`**: Functional `Result` / `Result<TValue>` pattern + Railway-Oriented Programming (ROP) combinators (`Match`, `Bind`, `Map`, `Tap`, `Ensure`).
- **`SharedKernel.Messaging`**: CQRS abstractions (`ICommand`, `IQuery`), Domain Events (`IDomainEvent`), MediatR `ValidationPipelineBehavior`, and Async Integration Message Bus (`IMessageBus`, `IIntegrationEvent`, `InMemoryMessageBus`).
- **`SharedKernel.Hateoas`**: Hypermedia models (`ResourceResponse<T>`, `CollectionResourceResponse<T>`, `Link`) and REST resource extensions.
- **`SharedKernel.Authentication`**: Vendor-agnostic `IUserContext` abstraction and JWT Bearer security integration (`AddJwtAuthentication`).

## 🛠️ Build & Pack

```bash
dotnet build SharedKernel.slnx
dotnet pack SharedKernel.slnx --configuration Release --output ./nupkgs
```
