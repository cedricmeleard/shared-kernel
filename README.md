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

## 🏷️ Versioning & Release Workflow (SemVer)

NuGet package versions are automatically incremented on push to `main` using Semantic Versioning (`MAJOR.MINOR.PATCH`, e.g. `1.0.1` -> `1.1.0` -> `2.0.0`).

Include one of the following parameters in your commit message to control the version increment:

- **`semver:major`**: Increments the **MAJOR** version (breaking changes, e.g. `1.2.3` ➔ `2.0.0`).
  ```bash
  git commit -m "feat!: breaking change in Messaging abstractions semver:major"
  ```
- **`semver:minor`**: Increments the **MINOR** version (new backward-compatible features, e.g. `1.2.3` ➔ `1.3.0`).
  ```bash
  git commit -m "feat: add new ROP combinator to Results semver:minor"
  ```
- **Default / `semver:patch`**: Increments the **PATCH** version (bug fixes & minor tweaks, e.g. `1.2.3` ➔ `1.2.4`).
  ```bash
  git commit -m "fix: resolve null check in UserContext"
  ```

Upon push to `main`, GitHub Actions automatically:
1. Calculates the next SemVer version.
2. Builds, tests, and packs `.nupkg` packages.
3. Publishes `.nupkg` files to **GitHub Packages**.
4. Creates a Git tag (e.g. `v1.0.1`) and GitHub Release.
