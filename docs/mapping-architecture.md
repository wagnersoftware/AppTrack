# Mapping Architecture

## Overview

AppTrack uses **manual, static extension methods** for all object mapping between architectural layers.
There is no reflection-based mapping library (e.g. AutoMapper). Every mapping is a plain C# method,
visible in the call stack, navigable from the call site, and verified at compile time.

### Why manual mapping?

| Concern | AutoMapper | Manual mapping |
|---|---|---|
| Error visibility | Runtime (`InvalidOperationException`) | Compile time |
| Debuggability | Cannot step into | Full debugger support |
| Refactoring safety | Silently maps wrong property after rename | Build fails immediately |
| `ProjectTo<>` / EF query projection | Supported | **Not needed** — this project does not use `ProjectTo<>` |
| Dependencies | NuGet package + DI registration | None |

---

## Where mapper classes live

The placement rule follows the **Clean Architecture dependency direction**: a mapper class belongs to the
project that already depends on both the source type and the target type. No new project-level
dependencies are introduced.

```
AppTrack.Domain                        (entities)
    ↑
AppTrack.Application                   (DTOs, commands, queries)
    ← Mappings/ lives here             Domain ↔ DTO, Command → Domain
    ↑
AppTrack.Frontend.ApiService           (NSwag-generated DTOs/commands, services)
    ← Mappings/ lives here             ApiDto ↔ FrontendModel, FrontendModel → ApiCommand
```

```
AppTrack.Application/
  Mappings/
    <Concept>Mappings.cs   ← one file per domain concept

AppTrack.Frontend.ApiService/
  Mappings/
    <Concept>Mappings.cs   ← one file per domain concept
```

> **Rule:** If both types are in `AppTrack.Application` (or transitively available via its
> `AppTrack.Domain` reference), the mapper class belongs in `AppTrack.Application/Mappings/`.
> If either type is NSwag-generated (in `AppTrack.Frontend.ApiService.Base`), the mapper class
> belongs in `AppTrack.Frontend.ApiService/Mappings/`.

---

## Mapper class structure

Each mapper class is an `internal static` class containing only `internal static` extension methods.
It is not registered in DI and has no state.

```csharp
// AppTrack.Application/Mappings/FooMappings.cs
namespace AppTrack.Application.Mappings;

internal static class FooMappings
{
    internal static FooDto ToDto(this Foo entity) => ...;
    internal static Foo ToDomain(this CreateFooCommand command) => ...;
    internal static void ApplyTo(this UpdateFooCommand command, Foo entity) { ... }
}
```

```csharp
// AppTrack.Frontend.ApiService/Mappings/FooMappings.cs
namespace AppTrack.Frontend.ApiService.Mappings;

internal static class FooMappings
{
    internal static FooModel ToModel(this FooDto dto) => ...;
    internal static CreateFooCommand ToCreateCommand(this FooModel model) => ...;
    internal static UpdateFooCommand ToUpdateCommand(this FooModel model) => ...;
}
```

---

## Method naming conventions

| Direction | Method name | Notes |
|---|---|---|
| Entity → DTO | `entity.ToDto()` | returns a new DTO |
| Command → Entity (create) | `command.ToDomain()` | returns a new entity |
| Command → Entity (update) | `command.ApplyTo(entity)` | mutates the existing tracked entity; `Id` and `CreationDate` are never assigned |
| Query → Entity (seed) | `query.ToDomain()` | only used when an entity is auto-created on first access |
| ApiDto → FrontendModel | `dto.ToModel()` | returns a new model |
| FrontendModel → ApiCommand (create) | `model.ToCreateCommand()` | returns a new command |
| FrontendModel → ApiCommand (update) | `model.ToUpdateCommand()` | returns a new command |

### Why `ApplyTo` instead of a second `ToDomain`?

`ApplyTo` makes the intent explicit: it updates a **tracked EF entity** in place. Fields that must
not be overwritten (`Id`, `CreationDate`) are simply never assigned in the method body — there is no
`Ignore()` configuration to search for.

```csharp
// the absence of these lines IS the documentation
// entity.Id = ...          ← never written → Id is preserved
// entity.CreationDate = ... ← never written → audit date is preserved
internal static void ApplyTo(this UpdateFooCommand command, Foo entity)
{
    entity.Name = command.Name;
    entity.Description = command.Description;
}
```

---

## Type conversion conventions

Some properties require explicit conversion because the source and target types differ.

### `DateTime` ↔ `DateOnly` (frontend layer only)

The API layer uses `DateTime` (JSON wire format). Frontend models use `DateOnly` for date-only fields.
The conversion is written inline in the mapper method — no global converter registration needed.

```csharp
// ApiDto → FrontendModel: DateTime → DateOnly
StartDate = DateOnly.FromDateTime(dto.StartDate),

// FrontendModel → ApiCommand: DateOnly → DateTime
StartDate = model.StartDate.ToDateTime(TimeOnly.MinValue),
```

### Enum casts (frontend layer only)

NSwag generates numeric enum members (`_0 = 0, _1 = 1, ...`) because it cannot access the backend
source names at generation time. Frontend models declare named enums with the same ordinal values.
Cast via `(int)` to remain independent of both names.

```csharp
// ApiDto → FrontendModel
Status = (FooModel.FooStatus)(int)dto.Status,

// FrontendModel → ApiCommand
Status = (FooStatus)(int)model.Status,
```

### Child collections

Replace the entire collection. Do not attempt to reconcile individual items by ID — this keeps the
mapping stateless and avoids EF change-tracking complexity.

```csharp
// Backend ApplyTo: clear tracked collection, add new domain objects
entity.Items.Clear();
foreach (var dto in command.Items)
    entity.Items.Add(Item.Create(dto.Key, dto.Value));

// Frontend ToUpdateCommand: project to API DTO list
Items = model.Items.Select(i => i.ToDto()).ToList(),
```

---

## Usage in handlers and services

Mapper methods are called directly — no `IMapper` is injected.

```csharp
// Command handler — create
var entity = request.ToDomain();
await _repository.CreateAsync(entity);
return entity.ToDto();

// Command handler — update
var entity = await _repository.GetByIdAsync(request.Id);
request.ApplyTo(entity);
await _repository.UpdateAsync(entity);
return entity.ToDto();

// Query handler
var entities = await _repository.GetAllAsync();
return entities.Select(e => e.ToDto()).ToList();
```

```csharp
// Frontend service — read
var dto = await _client.FooGETAsync(id);
return dto.ToModel();

// Frontend service — create
var command = model.ToCreateCommand();
command.UserId = userId;  // caller sets fields not present on the model
var result = await _client.FooPOSTAsync(command);
return result.ToModel();
```

---

## Adding a new mapped concept

1. **Create `<Concept>Mappings.cs`** in `AppTrack.Application/Mappings/`.
   Implement `ToDto()`, `ToDomain()` (create), and `ApplyTo()` (update) as needed.

2. **Create `<Concept>Mappings.cs`** in `AppTrack.Frontend.ApiService/Mappings/`.
   Implement `ToModel()`, `ToCreateCommand()`, and `ToUpdateCommand()` as needed.

3. **Replace `IMapper` with direct calls** in the handler or service.
   Remove the `IMapper` constructor parameter.

4. If the concept has child collections, add sibling overloads for the child type
   in the **same** `<Concept>Mappings.cs` file. Keep all overloads of the same method name
   adjacent (SonarAnalyzer rule S4136).

No DI registration, no profile class, no `Assembly.GetExecutingAssembly()` scan.
