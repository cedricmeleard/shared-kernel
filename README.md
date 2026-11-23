# SharedKernel

Reusable .NET 11 Shared Kernel building blocks.

## 📦 Published Packages

- **`SharedKernel.Results`**: Functional `Result` / `Result<TValue>` C# 15 `union` types + Railway-Oriented Programming (ROP) combinators (`Bind`, `Map`, `Tap`, `Ensure`) + exhaustive `switch` pattern matching.
- **`SharedKernel.Messaging`**: CQRS abstractions (`ICommand`, `IQuery`), Domain Events (`IDomainEvent`), MediatR `ValidationPipelineBehavior`, and Async Integration Message Bus (`IMessageBus`, `IIntegrationEvent`, `InMemoryMessageBus`).
- **`SharedKernel.Hateoas`**: Hypermedia models (`ResourceResponse<T>`, `CollectionResourceResponse<T>`, `Link`) and REST resource extensions.
- **`SharedKernel.Authentication`**: Vendor-agnostic `IUserContext` abstraction and JWT Bearer security integration (`AddJwtAuthentication`).

## 🛠️ Build & Pack

```bash
dotnet build SharedKernel.slnx
dotnet pack SharedKernel.slnx --configuration Release --output ./nupkgs
```

## 🏷️ Versioning & Release Workflow (SemVer & Pull Requests)

The `main` branch is protected and requires passing CI status checks (`build-and-test`) before merging.

### Workflow:
1. Create a feature branch and open a **Pull Request** targeting `main`.
2. CI builds, runs all unit tests, and validates package creation on the PR.
3. Include `semver:major`, `semver:minor`, or `semver:patch` in your **PR Title**, **PR Description**, or **Merge Commit Message**:
   - **`semver:major`**: Increments the **MAJOR** version (breaking changes, e.g. `1.1.0` ➔ `2.0.0`).
   - **`semver:minor`**: Increments the **MINOR** version (new features, e.g. `1.1.0` ➔ `1.2.0`).
   - **Default / `semver:patch`**: Increments the **PATCH** version (fixes, e.g. `1.1.0` ➔ `1.1.1`).
4. When the PR is merged into `main`, GitHub Actions automatically:
   - Publishes `.nupkg` packages to **GitHub Packages**.
   - Tags the merge commit (e.g. `v1.2.0`) and creates an official GitHub Release.
