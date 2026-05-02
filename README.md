# NewSchool Family

NewSchool Family is a mobile-first platform for **ensino domiciliar**. It helps families organize the daily routine, follow an annual curriculum, guide lessons step by step, and preserve evidence of learning in a separate media archive.

The current repository contains the web application used to run the parent experience, daily lessons, curriculum sequencing, reading flow, printable recommendations, billing, and evidence management.

## Product highlights

- Guided daily lessons with a clear `faça agora` flow
- Annual curriculum organized by age range and subject
- Weekly and monthly progression without forcing families to build plans manually
- Reading routine tied to curriculum stages
- Separate evidence center for photos, videos, and documents
- Storage plans with Stripe-based billing
- Public pages ready for SEO and future advertising slots

## Technology stack

- .NET 8
- ASP.NET Core MVC
- Entity Framework Core 8
- SQL Server
- Stripe.NET
- Resend

## Solution layout

```text
NewSchool.sln
└── NewSchool.Web
    ├── Controllers
    ├── Data
    ├── Domain
    ├── Models
    ├── Services
    ├── ViewComponents
    ├── Views
    └── wwwroot
```

This repository is intentionally organized as a **modular monolith**. The application keeps domain-oriented services, persistence, web controllers, and views inside a single deployable project so the product can evolve fast without losing code boundaries.

Additional technical notes:

- [Architecture](docs/architecture.md)
- [Deployment notes](docs/deployment.md)
- [Security policy](SECURITY.md)

## Getting started

### Prerequisites

- .NET SDK 8.0.x
- SQL Server or LocalDB

### 1. Restore dependencies

```bash
dotnet restore NewSchool.sln
```

### 2. Configure local settings

Copy the example file and adjust only what you need:

```bash
cp NewSchool.Web/appsettings.Local.example.json NewSchool.Web/appsettings.Local.json
```

You can also use **User Secrets** during development:

```bash
dotnet user-secrets --project NewSchool.Web set "ConnectionStrings:StarkaidSchoolConnection" "Server=(localdb)\\MSSQLLocalDB;Database=NewSchoolDev;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True;"
```

Recommended local keys:

- `ConnectionStrings:StarkaidSchoolConnection`
- `Stripe:PublishableKey`
- `Stripe:SecretKey`
- `Stripe:PriceId20`
- `Stripe:PriceId80`
- `Stripe:PriceId120`
- `Stripe:PriceIdExtra100`
- `Stripe:WebhookSecretSnapshot`
- `Stripe:WebhookSecretMin`
- `Email:ResendApiKey`
- `OpenRouter:PrimaryApiKey`

### 3. Run the application

```bash
dotnet run --project NewSchool.Web
```

### 4. Build in Release

```bash
dotnet build NewSchool.sln -c Release
```

## Configuration model

Versioned configuration files in this repository are **sanitized**. Real secrets must be provided through:

- `appsettings.Local.json`
- `appsettings.{Environment}.Local.json`
- User Secrets
- environment variables in deployment

## Billing and media storage

The product supports a free tier with a limited number of stored evidence files and paid tiers for larger media archives. Billing integration is implemented through Stripe and depends on environment-specific keys and webhook secrets.

## Quality and maintenance

- Release builds are validated through CI
- Local override files are ignored
- Runtime logs, generated email files, local SQLite artifacts, and publish scratch files are ignored

## Security

Do not commit secrets, production webhooks, live payment keys, or private customer data. See [SECURITY.md](SECURITY.md) for disclosure and handling guidelines.

## Contact

For product or security contact:

- `starkaid24@gmail.com`
