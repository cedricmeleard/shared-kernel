# CLAUDE.md

This file provides guidance to AI coding agents and developers working within the **shared-kernel** repository.

## Commands

```bash
# Build solution
dotnet build SharedKernel.slnx

# Run all unit tests
dotnet test SharedKernel.slnx

# Pack NuGet packages locally
dotnet pack SharedKernel.slnx --configuration Release --output ./nupkgs
```

## Architecture & Project Structure

This repository provides reusable, zero-dependency or lightweight .NET 11 building blocks shared across .NET domain applications.

```text
shared-kernel/
├── src/
│   ├── SharedKernel.Results/         # Functional Result pattern & Railway-Oriented Programming (ROP)
│   ├── SharedKernel.Messaging/       # CQRS, Domain Events, Validation Behavior, and Async Message Bus
│   ├── SharedKernel.Hateoas/          # Hypermedia models (ResourceResponse, Link) & REST extensions
│   └── SharedKernel.Authentication/  # IUserContext abstraction & JWT Bearer integration
├── test/
│   └── SharedKernel.UnitTests/       # xUnit + FluentAssertions unit tests for all shared libraries
├── .github/workflows/
│   └── publish.yml                   # CI/CD workflow publishing NuGet packages to GitHub Packages
└── SharedKernel.slnx                 # .NET 11 Solution file
```

## Package Descriptions & Design Guidelines

1. **`SharedKernel.Results`**:
   - Pure C# 15 `union` `Result` and `Result<TValue>` pattern (`Success`, `Success<TValue>`, `Error`) implementing `IResult<TSelf>`.
   - Railway-Oriented Programming (ROP) combinators: `Bind`, `Map`, `Tap`, `Ensure`, terminated by exhaustive `switch` pattern matching (`Success` / `Success<T>` / `Error`).
   - **No `Async` suffixes on method names**: Overload synchronous and asynchronous delegates (`.Bind(...)`, `.Map(...)`, `.Tap(...)`, `.Ensure(...)`).

2. **`SharedKernel.Messaging`**:
   - Decoupled CQRS abstractions: `ICommand`, `IQuery`, `ICommandHandler`, `IQueryHandler`, `ISender`.
   - Domain Event abstractions: `IDomainEvent`, `IDomainEventHandler`, `IEventPublisher`.
   - MediatR locked to `12.5.0` (last Apache 2.0 version series). Internalized inside this library.
   - Vendor-agnostic Async Integration Message Bus: `IMessageBus`, `IIntegrationEvent`, `IIntegrationEventHandler<T>`, and `InMemoryMessageBus` (powered by `System.Threading.Channels`).

3. **`SharedKernel.Hateoas`**:
   - REST Hypermedia models (`ResourceResponse<T>`, `CollectionResourceResponse<T>`, `Link`).
   - `.ToResource(...)` and `.ToCollectionResource(...)` extension methods.

4. **`SharedKernel.Authentication`**:
   - Vendor-agnostic identity abstractions (`IUserContext`, `UserContext`).
   - `AddJwtAuthentication` extension method for ASP.NET Core `IServiceCollection`.

## Coding Rules & Conventions

- All projects target `net11.0` with `<ImplicitUsings>enable</ImplicitUsings>` and `<Nullable>enable</Nullable>`.
- Versioning is automated via GitHub Actions using SemVer. Include `semver:major`, `semver:minor`, or `semver:patch` in commit messages when pushing to `main` to control release version bumps.
- Always run `dotnet test SharedKernel.slnx` before committing changes.
