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

Usa SQLite por defecto (`vsatemplate.db`); las migraciones pendientes se
aplican solas al arrancar. Nada más que configurar para empezar.

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

## Qué trae de fábrica

- **Validación** (FluentValidation) vía endpoint filter, antes del handler.
- **Errores en formato estándar** (RFC 9457 `problem+json`) para toda la API.
- **Auth JWT** con policies declaradas por slice, no centralizadas.
- **Paginación** reusable para listados (`IPagedRequest` / `PagedResponse<T>`).
- **Eventos in-process** para comunicar slices sin acoplarlos.
- **Contrato OpenAPI → TypeScript**, para generar clientes Angular desde el
  JSON que emite cada build.
- **Health checks**, **rate limiting** y **logging estructurado** (Serilog)
  listos para producción.

## Para ir más a fondo

- [Wiki del repositorio](https://github.com/betoramiz/vsa-template/wiki) —
  referencia detallada de cada pieza (por qué está hecha así, decisiones de
  diseño, cómo extenderla).
