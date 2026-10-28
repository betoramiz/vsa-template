# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commits

- No incluyas `Co-Authored-By: Claude ...` ni la línea `Claude-Session: ...`
  en los mensajes de commit de este repo.

## Qué es este repo

Es una plantilla `dotnet new` (`shortName: vsa-api`, `sourceName: VsaTemplate`) de una API ASP.NET Core (net10.0, EF Core + SQLite) con Vertical Slice Architecture. Además de ser una app que compila y corre, es el *fuente* de la plantilla: lo que se edita acá se emite al generar proyectos nuevos. README.md y TUTORIAL.md (en español) explican las decisiones de diseño.

## Comandos

```bash
dotnet run --project src/VsaTemplate                  # API local, SQLite en vsatemplate.db
dotnet build
dotnet test                                           # unitarios + integración + arquitectura
dotnet test --filter "FullyQualifiedName~ArchitectureTests"   # una clase
dotnet test --filter "FullyQualifiedName~PlaceOrderTests.NombreDelTest"   # un test
dotnet ef migrations add <Nombre> --project src/VsaTemplate -o Common/Persistence/Migrations   # herramienta local en dotnet-tools.json (dotnet tool restore)
dotnet new install . && dotnet new vsa-api -n Acme.Orders -o Acme.Orders   # probar la plantilla
```

Las migraciones pendientes se aplican solas al arrancar, pero **solo en Development** (`ApplyPendingMigrations`). El repo no trae migraciones commiteadas: en una base nueva crea `Initial` primero o las consultas dan `no such table` (verificado). Skills del proyecto en `.claude/skills/` (en inglés): `add-use-case`, `add-feature-slice`, `add-ef-migration`, `add-domain-event`. Versiones de paquetes centralizadas en `Directory.Packages.props`; no hay linter configurado.

## Arquitectura

Vertical slices: `src/VsaTemplate/Features/<Feature>/` es el negocio; `Common/` es infraestructura compartida (solo lo que se repite en 3+ features); `Composition/` es wiring de toda la app. Casi todo se **descubre por convención/reflection**, así que un feature nuevo no toca `Program.cs`:

- **Casos de uso**: un archivo por caso (`CreateProduct.cs`) con una clase estática `CreateProduct` que anida `Request`, `Response` y `Validator` (FluentValidation), más una clase `CreateProductHandler` pública con `Handle(...)` que devuelve `TypedResults`/`Results<...>`. Los handlers se registran como scoped por Scrutor en `Composition/UseCaseRegistration.cs` (clase pública, no abstracta, nombre termina en `Handler`).
- **Endpoints**: cada feature tiene `Endpoints.cs` que implementa `IFeatureEndpoints` (`static Map`); `EndpointDiscovery.MapFeatureEndpoints` los encuentra por reflection. Solo enrutamiento; la validación se agrega con `.WithValidation<TRequest>()`.
- **Auth**: JWT; las policies viven en el slice (`Policies : ISlicePolicies`) y `Common/Auth` las descubre por reflection.
- **Persistencia**: un único `AppDbContext` compartido. Las `IEntityTypeConfiguration<>` viven junto a la entidad en `Features/X/Domain/` y se aplican con `ApplyConfigurationsFromAssembly`. Los `DbSet` están dentro de `#if (IncludeSampleFeature)`. Usar `db.TrySaveChangesAsync()` (devuelve `SaveResult`: `Success`/`ConcurrencyConflict`/`DuplicateKey`) cuando el handler debe reaccionar a esas fallas; cualquier otra excepción va a `GlobalExceptionHandler` (problem+json, RFC 9457). Entidades con `IHasRowVersion` obtienen token de concurrencia manual (específico de SQLite).
- **Eventos in-process**: `IEventDispatcher.Publish` llama en secuencia a todos los `IEventHandler<T>` (autodescubiertos, mismo scope del request; si un suscriptor falla, falla el request). Los eventos publicados viven en `Features/X/Contracts/`; los suscriptores en `Features/Notifications/` (u otro slice), nunca en `Common`.

### Reglas de dependencia (impuestas por `tests/.../Architecture/ArchitectureTests.cs`)

Lo **público** de un slice es su `Domain/` y sus `Contracts/`; sus casos de uso (tipos en la raíz del namespace del slice) son privados. Por lo tanto:
1. Un slice no usa casos de uso de otro slice (se comunican por eventos o consultando `Domain` vía el DbContext).
2. `Common` no depende de casos de uso de ningún slice.
3. Los tipos públicos anidados en un caso de uso se llaman exactamente `Request`, `Response` o `Validator` (también definen los nombres del cliente TypeScript generado).
4. Todo `*Endpoints` implementa `IFeatureEndpoints`; todo `*Handler` público queda registrado; los `IEventHandler<>` viven en `Features`.

Si un test de arquitectura falla, corrige el código, no el test.

## Particularidades de la plantilla

- El código usa directivas `#if (IncludeSampleFeature)` / `#if (IncludeComments)` (símbolos del Template Engine, no solo de C#). `Directory.Build.props` los define siempre para que este repo compile con todo incluido. Mantén esas directivas al editar archivos de `Common`/`Composition`/`Program.cs`, y los comentarios de diseño dentro de `#if (IncludeComments)`.
- `.template.config/template.json` define qué se excluye al generar (`Features/Products/**` y `Orders/**` sin `IncludeSampleFeature`, `TUTORIAL.md`, `.claude/`, `openapi/`, `Migrations/`, la base SQLite, etc.). Si agregas archivos que no deben viajar con la plantilla, añádelos ahí.
- `openapi/VsaTemplate.json` es el contrato para generar clientes TypeScript; `OpenApiGenerateDocumentsOnBuild` está en `false` actualmente en el `.csproj`.
- Los tests de integración usan `WebApplicationFactory<Program>` (`tests/.../Common/ApiFactory.cs`, `TestDb.cs`, `TestJwt.cs`); `Program` se expone con `public partial class Program;`.
