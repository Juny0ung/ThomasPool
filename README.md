# ThomasPool

A full-stack profile registration and management system. Visitors submit a profile (name, contact info, photo, and a set of dynamic form fields) through a public web form, and admins review, filter, and approve submissions through a protected dashboard.

## Features

- **Public profile form** — dynamic, admin-configurable form fields (single-select, multi-select, boolean) in addition to core fields (name, gender, phone number, birth year, region, photo)
- **Admin dashboard** — search and filter submitted profiles by name, gender, birth year, and region
- **Admin accounts** — registration with an approval workflow (new admins start as `Pending` until approved by an existing admin)
- **JWT-based authentication** with role-based authorization
- **Photo upload & retrieval** backed by MongoDB (GridFS-style binary storage), processed with SkiaSharp

## Tech Stack

**Backend** (`ThomasPool-api`)
- ASP.NET Core 8 (Web API)
- MongoDB (`MongoDB.Driver`)
- JWT authentication (`Microsoft.AspNetCore.Authentication.JwtBearer`)
- BCrypt for password hashing
- SkiaSharp for image processing
- xUnit + Moq for testing (`ThomasPool-api.Tests`)

**Frontend** (`ThomasPool-web`)
- React 19 + TypeScript
- Vite
- React Router
- Tailwind CSS

## Project Structure

```
ThomasPool/
├── ThomasPool-api/           # ASP.NET Core Web API
│   ├── Api/                  # Controllers & DTOs
│   ├── Application/          # Services & application-level DTOs
│   ├── Domain/                # Entities, interfaces, domain services
│   └── Infra/                 # MongoDB persistence, JWT, image processing
├── ThomasPool-api.Tests/      # xUnit test suite
├── ThomasPool-web/            # React + TypeScript frontend
└── .github/workflows/         # CI/CD (test + deploy to Render)
```

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) (v18+)
- A MongoDB instance (local or [Atlas](https://www.mongodb.com/atlas))

### Backend Setup

1. Navigate to the API project:
   ```bash
   cd ThomasPool-api
   ```
2. Configure your local settings. Create an `appsettings.Development.json` (git-ignored) or use [.NET user-secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets):
   ```json
   {
     "MongoDB": {
       "ConnectionString": "<your-mongodb-connection-string>",
       "DatabaseName": "<your-database-name>"
     },
     "Jwt": {
       "Key": "<a-long-random-secret>",
       "Issuer": "ThomasPool",
       "Audience": "ThomasPool"
     },
     "Cors": {
       "AllowedOrigin": "http://localhost:5173"
     }
   }
   ```
3. Run the API:
   ```bash
   dotnet run
   ```
   The API will be available at `http://localhost:5050` (Swagger UI at `/swagger` in Development).

### Frontend Setup

1. Navigate to the web project:
   ```bash
   cd ThomasPool-web
   ```
2. Install dependencies:
   ```bash
   npm install
   ```
3. Create a `.env` file (git-ignored) with the API base URL:
   ```
   VITE_API_URL=http://localhost:5050
   ```
4. Run the dev server:
   ```bash
   npm run dev
   ```
   The app will be available at `http://localhost:5173`.

### Running Tests

```bash
dotnet test ThomasPool-api.Tests/ThomasPool-api.Tests.csproj
```

## Deployment

The backend is deployed via a GitHub Actions workflow ([`.github/workflows/backend-deploy.yml`](.github/workflows/backend-deploy.yml)) that runs the test suite and, on success, triggers a [Render](https://render.com) deploy hook. The deploy hook URL is stored as a GitHub Actions secret (`RENDER_DEPLOY_HOOK`) and is never committed to the repository.

## Environment Variables

No secrets are committed to this repository. All connection strings, JWT signing keys, and API URLs are supplied via configuration that is excluded from version control:

| Location | File | Notes |
|---|---|---|
| Backend | `appsettings.Development.json` | git-ignored; falls back to empty values in `appsettings.json` |
| Frontend | `.env` | git-ignored; read via `import.meta.env.VITE_API_URL` |
| CI/CD | GitHub Actions secrets | e.g. `RENDER_DEPLOY_HOOK` |
