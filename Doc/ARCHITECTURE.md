# FinRiskLensAI — Architecture

## 1. Overview

**FinRiskLensAI** is an ASP.NET Core 8 (MVC) application organized as a layered
solution. The codebase is currently a scaffold for a **Credit Rule Engine**
(the runtime configuration, connection strings, and JWT audience all reference
`CreditRuleEngine`) built on a clean, dependency-inverted layering with
**Autofac** for composition, **Entity Framework Core** for persistence, and
**Serilog** for structured logging.

| Property            | Value                                             |
| ------------------- | ------------------------------------------------- |
| Target framework    | `net8.0`                                          |
| Language features   | Nullable reference types, implicit usings enabled |
| Web stack           | ASP.NET Core MVC (Controllers + Views)            |
| DI container        | Autofac (via `AutofacServiceProviderFactory`)     |
| ORM                 | EF Core 8 (SQL Server provider; PostgreSQL-ready) |
| Logging             | Serilog (Console + rolling File sinks)            |
| Auth                | JWT Bearer (configured, pipeline wired)           |

---

## 2. Solution Structure

The solution file `FinRiskLensAI.sln` sits at the repository root, with each
project in a sibling folder beneath it:

```
F:\FinRiskLensAI\
├── FinRiskLensAI.sln
│
├── FinRiskLensAI\                 → Web / Presentation layer (startup project)
│   ├── Controllers\HomeController.cs
│   ├── Models\ErrorViewModel.cs
│   ├── Views\                     (Razor views + shared layout)
│   ├── wwwroot\                   (static assets: bootstrap, jQuery, css, js)
│   ├── Program.cs                 (host bootstrap, DI, middleware pipeline)
│   ├── appsettings.json           (DB, JWT, Serilog config)
│   └── FinRiskLensAI.csproj
│
├── FinRiskLensAI.Services\        → Application / Business logic layer
│   ├── DI\ServicesModule.cs       (Autofac module — auto-registers *Service)
│   └── FinRiskLensAI.Services.csproj
│
├── FinRiskLensAI.Core\            → Domain layer (no infra dependencies)
│   ├── Interfaces\IRepository.cs  (repository contract)
│   ├── Models\Common\BaseEntity.cs
│   ├── Models\Common\AuditableEntity.cs
│   ├── DI\CoreModule.cs
│   └── FinRiskLensAI.Core.csproj
│
└── FinRiskLensAI.Data\           → Infrastructure / Persistence layer
    ├── DbContextEDMX\ApplicationDbContext.cs
    ├── Repositories\RepositoryBase.cs
    ├── DI\DataModule.cs           (Autofac module — auto-registers *Repository)
    └── FinRiskLensAI.Data.csproj
```

---

## 3. Layered Architecture

The solution follows a **Clean / Onion architecture**: dependencies point
*inward* toward the domain (`Core`). `Core` has no project dependencies of its
own; every other layer references it.

```
        ┌─────────────────────────────────────────────┐
        │            FinRiskLensAI (Web / MVC)          │
        │   Controllers · Views · Program.cs · config   │
        └───────────────┬───────────────┬───────────────┘
                        │ references     │ references
                        ▼                ▼
        ┌───────────────────────┐   ┌───────────────────────┐
        │  FinRiskLensAI.Services │   │   (Core, transitively) │
        │   business logic        │   └───────────────────────┘
        └───────────┬─────────────┘
                    │ references
        ┌───────────▼───────────┐        ┌───────────────────────┐
        │  FinRiskLensAI.Data     │───────▶│   FinRiskLensAI.Core    │
        │  EF Core · Repositories │ refs   │  entities · interfaces  │
        └─────────────────────────┘        └───────────────────────┘
                    ▲                                   ▲
                    └────────────── references ─────────┘
```

### Project reference graph

| Project        | References                     |
| -------------- | ------------------------------ |
| **Web**        | `Core`, `Services`             |
| **Services**   | `Core`, `Data`                 |
| **Data**       | `Core`                         |
| **Core**       | *(none — pure domain)*         |

### Layer responsibilities

- **Core (Domain)** — Framework-agnostic domain model and contracts. Holds
  base entity types (`BaseEntity`, `AuditableEntity`) and the persistence
  abstraction `IRepository<T>`. Depends on nothing but the BCL.
- **Data (Infrastructure)** — EF Core implementation of persistence:
  `ApplicationDbContext` and the generic `RepositoryBase<T>` that satisfies
  `IRepository<T>`. Knows about the database; the domain does not.
- **Services (Application)** — Home for business logic / use-case orchestration.
  Consumes repository abstractions from `Core` and persistence from `Data`.
  Currently a DI scaffold awaiting concrete `*Service` classes.
- **Web (Presentation)** — ASP.NET Core MVC host. Owns the composition root
  (`Program.cs`), the HTTP pipeline, controllers, Razor views, and configuration.

---

## 4. Dependency Injection (Autofac)

Composition is centralized in `Program.cs`, which swaps the default container
for Autofac and registers one module per layer:

```csharp
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
builder.Host.ConfigureContainer<ContainerBuilder>(container =>
{
    container.RegisterModule(new CoreModule());
    container.RegisterModule(new DataModule());
    container.RegisterModule(new ServicesModule());
});
```

Each layer owns its own registrations via an Autofac `Module`:

| Module           | Layer    | Registration strategy                                                                 |
| ---------------- | -------- | ------------------------------------------------------------------------------------- |
| `CoreModule`     | Core     | Placeholder for validators / mappers (currently empty).                               |
| `DataModule`     | Data     | Assembly scan — every type whose name ends in **`Repository`** → its interfaces, `InstancePerLifetimeScope`. |
| `ServicesModule` | Services | Assembly scan — every type whose name ends in **`Service`** → its interfaces, `InstancePerLifetimeScope`.    |

> **Convention:** New repositories/services are wired automatically as long as
> they follow the `*Repository` / `*Service` naming convention and implement an
> interface. No manual registration needed.

---

## 5. Domain Model

All entities derive from a common base that provides identity and soft-delete:

```csharp
public abstract class BaseEntity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public bool IsDeleted { get; protected set; }
    public void SoftDelete() => IsDeleted = true;
}

public abstract class AuditableEntity : BaseEntity
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
}
```

- **`BaseEntity`** — `Guid` primary key generated in-memory; soft-delete flag.
- **`AuditableEntity`** — adds created/updated timestamps and user stamps.

Audit timestamps are populated automatically on save (see §6).

---

## 6. Persistence

### ApplicationDbContext

`ApplicationDbContext` (in `Data/DbContextEDMX/`) centralizes EF Core config:

- **Decimal precision convention** — all `decimal` properties default to
  `precision(18, 4)`, chosen to be portable across SQL Server and PostgreSQL.
- **Configuration discovery** — `ApplyConfigurationsFromAssembly` auto-loads any
  `IEntityTypeConfiguration<T>` in the Data assembly, keeping mapping code
  modular.
- **Automatic auditing** — overrides `SaveChangesAsync` to stamp `CreatedAt` on
  add and `UpdatedAt` on add/modify for every tracked `AuditableEntity`.

### Generic repository

`RepositoryBase<T>` implements `IRepository<T>` over `DbSet<T>` with async CRUD,
predicate-based `FindAsync`, range insert, and `CountAsync`. All members are
`virtual` so specific repositories can override behavior. Note the repository
does **not** call `SaveChanges` — persistence is expected to be committed by a
higher layer (unit-of-work style), keeping write batching under caller control.

### Repository contract (`Core.Interfaces.IRepository<T>`)

```csharp
Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default);
Task<IReadOnlyList<T>> FindAsync(Expression<Func<T,bool>> predicate, CancellationToken ct = default);
Task<T>  AddAsync(T entity, CancellationToken ct = default);
Task     AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default);
void     Update(T entity);
void     Remove(T entity);
Task<int> CountAsync(CancellationToken ct = default);
```

---

## 7. Configuration

`appsettings.json` drives the runtime. Key sections:

- **`Database`** — a `DbType` selector (`SqlServer` by default) plus named
  connection strings for both **SqlServer** (LocalDB) and **PostgreSQL**. The
  design anticipates a provider switch based on `DbType`.
- **`Jwt`** — signing key, issuer (`CreditRuleEngine`), audience (`CREClients`),
  and token expiry. The dev signing key is a placeholder and **must be replaced**
  for any non-dev environment.
- **`Serilog`** — minimum levels with per-namespace overrides, Console + daily
  rolling File sink (`logs/cre-.log`), enriched with machine name and thread id.

> ⚠️ **Secrets note:** the JWT key and DB credentials are checked into
> `appsettings.json` for local convenience. Move these to user-secrets /
> environment variables / a secret store before deploying.

---

## 8. Request Pipeline & Hosting

`Program.cs` configures a standard MVC pipeline:

1. **Serilog** installed as the host logger (reads config from `appsettings`).
2. **Autofac** container factory + per-layer modules.
3. `AddControllersWithViews()`.
4. Middleware order: exception handler + HSTS (non-dev) → HTTPS redirect →
   static files → routing → **authentication** → **authorization** → endpoints.
5. Routing: attribute routes (`MapControllers`) plus the conventional
   `{controller=Home}/{action=Index}/{id?}` default route.

### Local endpoints (from `launchSettings.json`)

| Profile     | URL(s)                                          |
| ----------- | ----------------------------------------------- |
| http        | `http://localhost:5086`                         |
| https       | `https://localhost:7052`, `http://localhost:5086` |
| IIS Express | `http://localhost:2276` (SSL `44395`)           |

---

## 9. Key NuGet Dependencies

| Package                                            | Used in            | Purpose                         |
| -------------------------------------------------- | ------------------ | ------------------------------- |
| `Autofac`, `Autofac.Extensions.DependencyInjection`| all                | IoC container + host integration|
| `Microsoft.EntityFrameworkCore` (+ SqlServer, Design) | Data            | ORM, SQL Server provider, tooling |
| `Microsoft.AspNetCore.Authentication.JwtBearer`    | Web, Services      | JWT bearer authentication       |
| `Serilog` (+ AspNetCore, File, Enrichers.Thread)   | all                | Structured logging              |
| `Newtonsoft.Json`                                  | all                | JSON serialization              |
| `Microsoft.Bcl.AsyncInterfaces`                    | all                | async abstractions              |

---

## 10. Extending the Application

To add a new feature end-to-end, follow the layer conventions:

1. **Domain** — add an entity in `Core/Models` deriving from `BaseEntity` /
   `AuditableEntity`; declare any needed abstraction in `Core/Interfaces`.
2. **Persistence** — add an `IEntityTypeConfiguration<T>` and (if needed) a
   `FooRepository : RepositoryBase<Foo>` in `Data`. It auto-registers via
   `DataModule`. Expose a `DbSet<Foo>` on `ApplicationDbContext` (or rely on
   `Set<T>()`).
3. **Application** — add `IFooService` + `FooService` in `Services`. It
   auto-registers via `ServicesModule`.
4. **Presentation** — add a controller in the Web project that depends on the
   service interface; add Razor views as needed.

Because registration is convention-based, adhering to the `*Repository` /
`*Service` naming keeps DI wiring automatic.

---

## 11. Notes & Observations

- The `ApplicationDbContext` is defined but **not yet registered** with a
  provider in `Program.cs` (no `AddDbContext` call). Wire it using the
  `Database:DbType` + connection string before repositories can resolve.
- JWT authentication middleware is in the pipeline, but the JWT bearer
  **handler is not yet registered** (`AddAuthentication().AddJwtBearer(...)` is
  absent). Add it to activate the configured `Jwt` settings.
- `Services` and `Core` DI modules are scaffolds — no concrete services or
  domain services exist yet.
- The folder name `DbContextEDMX` is legacy-flavored; the context is a modern
  code-first EF Core `DbContext`, not an EDMX designer model.
