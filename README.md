# ClinicFlow

Appointment scheduling for a small clinic: an ASP.NET Core 10 Web API backed by
PostgreSQL, plus an Angular 19 front end.

```
ClinicFlow.Api/          .NET 10 Web API  → http://localhost:5057
└── clinic-flow-ui/      Angular 19 SPA   → http://localhost:4200
```

## Prerequisites

- .NET SDK 10.0
- Node.js 20+ and npm
- Docker (for PostgreSQL), or a PostgreSQL 14+ server you point the app at

## 1. Start PostgreSQL

```bash
docker compose up -d
```

The connection string lives in `appsettings.Development.json`. To point at your
own server without editing tracked files, override it from the environment —
ASP.NET Core maps `__` onto nested configuration keys:

```bash
export ConnectionStrings__DefaultConnection="Host=myhost;Database=clinicflow;Username=me;Password=..."
```

## 2. Run the API

```bash
dotnet run --launch-profile http
```

On startup the API applies EF Core migrations and, if the database is empty,
seeds three providers (weekday 09:00–17:00 availability), four patients and two
appointments.

- API base: `http://localhost:5057/api`
- Swagger UI: `http://localhost:5057/swagger`

## 3. Run the front end

```bash
cd clinic-flow-ui && npm install && npm start
```

Open `http://localhost:4200`. The SPA reads `API_BASE_URL` from
`src/app/core/api.config.ts`; it must match the API's origin and the
`AllowAngular` CORS policy in `Program.cs`.

## Tests

```bash
cd clinic-flow-ui && npx ng test --watch=false --browsers=ChromeHeadless
```

## Migrations

```bash
dotnet tool install --global dotnet-ef
dotnet ef migrations add <Name>
```

Migrations are applied automatically at startup by `Data/DatabaseInitialiser.cs`.
That is a development convenience — for production, run `dotnet ef database
update` as a deploy step instead.

## Security

**Credentials in this repository.** The only credential committed here is
`clinicflow/clinicflow`, a throwaway PostgreSQL account that exists solely
inside the local Docker container defined in `docker-compose.yml`, bound to
`127.0.0.1`. It is committed deliberately so the project runs immediately after
cloning. It grants access to nothing outside a developer's own machine.

Real credentials never belong in tracked files. In order of preference:

| Context | Mechanism |
| --- | --- |
| Local development | `dotnet user-secrets set "ConnectionStrings:DefaultConnection" "..."` — stored in `~/.microsoft/usersecrets/`, outside the repo |
| CI | GitHub Actions repository secrets, injected as environment variables |
| Production | The platform's secret store (Key Vault, Secrets Manager, container env) |

**Patient data is synthetic.** Every name, email, phone number and date of birth
in `Data/DatabaseInitialiser.cs` is invented, and all addresses use the reserved
`.test` TLD. This project handles no real personal or health information. Were
it to hold real patient records, the access controls, audit logging, encryption
at rest and transport hardening required by HIPAA and similar regimes would all
be prerequisites — none of which this reference implementation provides.

**If a secret is ever committed**, removing it in a later commit is not enough:
it remains in the history and must be treated as disclosed. Rotate the
credential first, then rewrite history (`git filter-repo`) if the repository is
public.

## Known limitations

- **Availability is evaluated in UTC.** `AvailabilitySlot` stores a bare
  `TimeOnly`, and `ProviderService.IsAvailableAsync` compares it against the UTC
  clock time of the requested appointment. A "09:00–17:00" slot therefore means
  09:00–17:00 UTC, not clinic-local time. Supporting clinics outside UTC needs a
  time zone on the provider or clinic, with the comparison done in that zone.
- **No authentication.** The JWT bearer package was referenced but never
  configured; `app.UseAuthentication()` has been removed because calling it
  without `AddAuthentication()` throws at startup. Add both together when you
  introduce auth.
- **Booking races.** Conflict checks and the insert are not in one transaction,
  so two simultaneous requests can both pass the check. A unique exclusion
  constraint on `(ProviderId, tstzrange(StartTime, EndTime))` would close this.
