# VsaTemplate

Plantilla `dotnet new` para crear APIs .NET con **Vertical Slice Architecture**: 
la estructura del código refleja el negocio (features), no la
técnica (capas). La regla central es simple: **la complejidad se paga por
slice, no por proyecto** — un CRUD sencillo es un archivo; un caso de uso
complejo puede tener domain model, más dependencias y más tests, sin obligar
al resto del proyecto a esa misma ceremonia.

## ¿Para quién es esto?

Para equipos que sienten que Clean Architecture (capas, proyectos separados,
interfaces con una sola implementación) es demasiada ceremonia para casos de
uso simples, pero igual quieren orden, tests de arquitectura que impidan el
caos, y un camino claro de crecimiento cuando un feature sí se vuelve
complejo.

## Instalar y generar un proyecto

```bash
dotnet new install .                          # una vez, desde este repo
dotnet new vsa-api -n Acme.Orders -o Acme.Orders
```

Parámetros útiles al generar:

- `--IncludeSampleFeature` — incluye el feature de ejemplo (`Products`) para
  ver el patrón aplicado end-to-end.
- `--IncludeComments` — incluye comentarios explicativos de diseño en
  `Common`/`Composition`.

## Correr y testear

```bash
dotnet run --project src/VsaTemplate     # API en http://localhost:5xxx
dotnet test                              # unitarios + integración
```

Usa SQLite por defecto (`vsatemplate.db`). En Development, las migraciones
pendientes se aplican solas al arrancar. **El repo no trae ninguna migración
commiteada** (`Common/Persistence/Migrations/` se genera y la plantilla no la
incluye), así que en una base nueva hay que crear la inicial antes de la
primera corrida, o las consultas fallan con `no such table`:

```bash
dotnet tool restore                      # dotnet-ef es tool local (dotnet-tools.json)
dotnet ef migrations add Initial --project src/VsaTemplate -o Common/Persistence/Migrations
```

### Manejo de secretos

Los secretos (p. ej. la clave de firma JWT) no van en `appsettings.json`: en
desarrollo usa `dotnet user-secrets` (el proyecto ya tiene `UserSecretsId`) y en
producción variables de entorno. Para un secret store real (Azure Key Vault,
AWS Secrets Manager…) agrégalo como proveedor de configuración en `Program.cs`.

## Estructura a grandes rasgos

```
src/VsaTemplate/
  Program.cs           ← composición: builder.Services.Add...() + Map...()
  Common/               ← solo lo que se repite en 3+ features (validación,
                           errores, auth, paginación, eventos, OpenAPI...)
  Features/
    Products/
      Endpoints.cs        ← solo enrutamiento
      CreateProduct.cs    ← Request + Response + Validator + Handler, juntos
      Domain/             ← entidades del slice
tests/VsaTemplate.Tests/
  Architecture/         ← las reglas de la arquitectura, codificadas (rompen el build si se violan)
  Features/             ← tests unitarios por slice
  Integration/           ← tests HTTP end-to-end
```

Un caso de uso nuevo casi siempre son dos pasos: un archivo en
`Features/<Feature>/` con su `Handler`, y una línea de mapeo en el
`Endpoints.cs` del feature. No hay que tocar archivos de registro central —
todo se descubre por convención.

## Reglas de la arquitectura

1. **Un slice no usa los casos de uso de otro slice.** Lo público de un slice
   es su `Domain/` (consultas vía el DbContext compartido) y sus `Contracts/`
   (eventos publicados); todo lo demás es privado.
2. **`Common` nunca depende de casos de uso de los slices** (la flecha va en un
   solo sentido). Reaccionar a un evento es lógica de negocio: los
   `IEventHandler<T>` viven en `Features/`.
3. **Nada entra a `Common` hasta que se repite en 3+ features.**
4. **`Endpoints.cs` solo enruta**; la lógica vive en el handler del caso de uso.
5. **La complejidad es opt-in por slice** (domain model, validators, etc.).

Las reglas 1 y 2, más las convenciones de nombres y registro, están codificadas
en `tests/VsaTemplate.Tests/Architecture/ArchitectureTests.cs`.

## Qué trae de fábrica

- **Validación** (FluentValidation) vía endpoint filter, antes del handler.
- **Errores en formato estándar** (RFC 9457 `problem+json`) para toda la API.
- **Auth JWT** con policies declaradas por slice, no centralizadas.
- **Paginación** reusable para listados (`IPagedRequest` / `PagedResponse<T>`).
- **Eventos in-process** para comunicar slices sin acoplarlos.
- **Contrato OpenAPI → TypeScript**, para generar clientes Angular. El
  documento se sirve en `/openapi/v1.json` y la UI en `/scalar/v1` (solo
  Development/Staging). Para emitirlo a `src/VsaTemplate/openapi/` en cada build,
  pon `OpenApiGenerateDocumentsOnBuild` en `true` en el `.csproj` (hoy está en
  `false`); luego:
  `npx ng-openapi-gen --input src/VsaTemplate/openapi/VsaTemplate.json --output src/app/api`.
- **Health checks**, **rate limiting** y **logging estructurado** (Serilog)
  listos para producción.

## Para ir más a fondo

- [Wiki del repositorio](https://github.com/betoramiz/vsa-template/wiki) —
  referencia detallada de cada pieza (por qué está hecha así, decisiones de
  diseño, cómo extenderla).
