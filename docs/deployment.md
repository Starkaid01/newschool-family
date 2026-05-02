# Deployment notes

## Runtime requirements

- .NET 8 runtime
- SQL Server
- writable application storage for local email pickup and temporary generated content

## Required production configuration

At minimum, set:

- `ConnectionStrings__StarkaidSchoolConnection`
- `Stripe__PublishableKey`
- `Stripe__SecretKey`
- `Stripe__PriceId20`
- `Stripe__PriceId80`
- `Stripe__PriceId120`
- `Stripe__PriceIdExtra100`
- `Stripe__WebhookSecretSnapshot`
- `Stripe__WebhookSecretMin`
- `Email__PublicBaseUrl`

Optional:

- `Email__ResendApiKey`
- `OpenRouter__PrimaryApiKey`
- `FamilyLibrary__Enabled`
- `FamilyLibrary__SourceCatalogConnection`
- `FamilyLibrary__SourceAssetsRootPath`

## Production safety checks

The application refuses to start in non-development environments when Stripe production keys are not configured correctly.

## CI

GitHub Actions builds the solution on every push to `main` and on pull requests.
