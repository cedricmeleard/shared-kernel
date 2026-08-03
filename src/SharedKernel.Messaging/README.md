# SharedKernel.Messaging

`SharedKernel.Messaging` provides a clean, decoupled abstraction layer for CQRS commands and queries, Domain Events, FluentValidation pipeline behaviors, and a vendor-agnostic Async Integration Message Bus.

---

## 🚀 Features

1. **CQRS Abstractions**:
   - `ICommand`, `ICommand<TResponse>`, `ICommandHandler<TCommand, TResponse>`
   - `IQuery<TResponse>`, `IQueryHandler<TQuery, TResponse>`
   - `ISender` (Internalizes MediatR 12.5.0 so domain and API layers remain MediatR-free).

2. **Domain Event Handling**:
   - `IDomainEvent`, `IDomainEventHandler<TDomainEvent>`, `IEventPublisher`.

3. **Automatic Validation Pipeline**:
   - `ValidationPipelineBehavior<TRequest, TResponse>`: Intercepts MediatR command pipelines and automatically runs registered FluentValidation validators, returning structured `Result.Failure` on validation errors.

4. **Vendor-Agnostic Async Integration Message Bus**:
   - `IIntegrationEvent`, `IIntegrationEventHandler<TIntegrationEvent>`, `IMessageBus` (`PublishAsync`).
   - `InMemoryMessageBus`: Channel-based (`System.Threading.Channels`) background queue worker (`ChannelMessageBusHostedService`) for fire-and-forget async integration event handling without external broker dependencies.

---

## 💻 Usage Examples

### 1. Registering Messaging in Dependency Injection

```csharp
// Program.cs / DependencyInjection.cs
services.AddMessaging(typeof(ApplicationAssembly).Assembly);
```

### 2. Defining a Command & Handler

```csharp
public record CreateUserCommand(string Email, string Name) : ICommand<UserResponse>;

public class CreateUserCommandHandler(IUserRepository repository) 
    : ICommandHandler<CreateUserCommand, UserResponse>
{
    public async Task<Result<UserResponse>> Handle(CreateUserCommand command, CancellationToken cancellationToken)
    {
        var user = new User(Guid.NewGuid(), command.Email, command.Name);
        await repository.AddAsync(user, cancellationToken);
        return Result.Success(new UserResponse(user.Id, user.Email));
    }
}
```

### 3. Publishing Integration Events on MessageBus

```csharp
public class UserRegisteredDomainEventHandler(IMessageBus messageBus) 
    : IDomainEventHandler<UserRegisteredDomainEvent>
{
    public Task Handle(UserRegisteredDomainEvent notification, CancellationToken cancellationToken)
    {
        var integrationEvent = new UserRegisteredIntegrationEvent(notification.UserId, notification.Email);
        return messageBus.PublishAsync(integrationEvent, cancellationToken);
    }
}
```
