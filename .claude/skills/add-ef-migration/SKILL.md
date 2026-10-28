---
name: add-ef-migration
description: Create or apply an EF Core migration for the shared AppDbContext in this VSA template (SQLite). Use when an entity or its configuration changed, when asked to "add a migration", or when a fresh database fails with "no such table".
---

# EF Core migrations

There is a single `AppDbContext` and a single linear `ModelSnapshot`, so migrations are **global and live in
`src/VsaTemplate/Common/Persistence/Migrations/`**, not per slice. Entity configurations, in contrast,
live next to their entity (`Features/<F>/Domain/`).

## Create a migration

`dotnet-ef` is a local tool pinned in `dotnet-tools.json`; no global install needed.

```bash
dotnet tool restore
dotnet ef migrations add AddInvoices --project src/VsaTemplate -o Common/Persistence/Migrations
```

Run from the repo root. Use a descriptive PascalCase name (`Initial`, `AddInvoices`, `AddProductSku`).
Review the generated `Up()`/`Down()` and the updated `AppDbContextModelSnapshot.cs` before committing.

## Apply

- **Development**: automatic. `ApplyPendingMigrations()` in `Program.cs` calls `Database.Migrate()` at startup,
  but only when `app.Environment.IsDevelopment()` and there are pending migrations.
- **Production**: not applied automatically; run them in the deploy pipeline.
  Manual: `dotnet ef database update --project src/VsaTemplate`.

## Gotchas

- **The template ships with no migrations.** A fresh clone has none, so the first run in Development returns
  `500 ... SQLite Error 1: 'no such table: Products'`. Create `Initial` first (command above).
- Unit tests use `TestDb` (`EnsureCreated`, schema straight from the model); integration tests boot `Program`
  and so exercise the real migration chain. `EnsureCreated` and migrations are mutually exclusive on the same database.
- Concurrency (SQLite): entities implementing `IHasRowVersion` use a manually bumped token. Set it up in the
  entity configuration and save through `db.TrySaveChangesAsync(ct)`:

```csharp
// Features/Products/Domain/ProductConfiguration.cs
builder.Property(p => p.Name).HasMaxLength(200);
builder.Property(p => p.Price).HasPrecision(18, 2);
builder.Property(p => p.RowVersion).IsConcurrencyToken();
```

- Do not hand-edit an already-applied migration; add a new one.
- Local dev database is `src/VsaTemplate/vsatemplate.db` (excluded from generated projects by `template.json`). Delete it to start clean.
