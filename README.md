# MOM Suite

MOM Suite is a two-project ASP.NET Core solution for creating, managing, previewing, and exporting Minutes of Meeting documents.

It includes:
- `MoM.Web`: ASP.NET Core MVC frontend
- `MoM.Api`: ASP.NET Core Web API backend

The system supports a structured MOM workflow with reusable users and venues, attendance tracking, analytics, and PDF export using QuestPDF.

## Highlights

- Branded landing page with guided entry into the application
- Dashboard focused on analytics and meeting insights
- Dedicated `Meeting Vault` page for saved MOM records
- Full MOM CRUD flow: create, edit, preview, delete, export
- QuestPDF-based PDF generation
- Reusable master data for users and venues
- Custom themed dropdowns with select-or-create behavior
- Attendance tracking with present/absent status
- Analytics for total meetings, attendance ratio, user attendance, and venue usage
- Scalar API reference UI for backend exploration

## Solution Structure

```text
MOM/
|-- MoM.Api/          Backend API, EF Core, SQL Server, PDF generation
|-- MoM.Web/          Frontend MVC application
|-- MoM.sln           Full solution
|-- README.md
|-- .gitignore
```

## Technology Stack

- .NET 8
- ASP.NET Core MVC
- ASP.NET Core Web API
- Entity Framework Core
- SQL Server / LocalDB
- QuestPDF
- Scalar.AspNetCore

## Current Functional Scope

### Frontend

- Welcome page at `/`
- Analytics dashboard at `/Home/Dashboard`
- Meeting archive at `/Home/MeetingVault`
- Create/edit MOM page at `/Home/Editor`
- Preview page at `/Home/Preview/{id}`

### Backend

- Meeting CRUD endpoints
- Lookup endpoints for users and venues
- Statistics endpoint for dashboard analytics
- PDF export endpoint
- Scalar API documentation

## Data Model Overview

The application now uses normalized lookup and mapping tables instead of storing important people and venue details as plain strings.

### Core tables

- `Meetings`
- `Users`
- `Venues`

### Mapping tables

- `MeetingUserMaps`
  - maps users to a meeting
  - used for `Facilitator`, `Chairperson`, `Secretary`, and `Attendee`
  - stores attendee presence with `IsPresent`

- `MeetingVenueMaps`
  - maps the selected venue to a meeting

### Child tables

- `AgendaItems`
  - each agenda item can map its `Owner` to a user

- `ActionItems`
  - each action item can map its `Responsibility` to a user

## Key Workflow

1. Open the application at `http://localhost:5156`
2. Start from the welcome page
3. Open the dashboard for analytics
4. Create a new MOM
5. Fill meeting details section-wise
6. Select an existing user/venue or type a new one
7. Save the MOM
8. Open `Meeting Vault` to preview, edit, export, or delete saved records

## Local URLs

### Frontend

- `http://localhost:5156`

### Backend API

- `http://localhost:5157`

### Scalar API Reference

- `http://localhost:5157/scalar`

## Setup

### 1. Restore packages

From the solution root:

```powershell
dotnet restore
```

### 2. Apply database migrations

From the API project:

```powershell
cd .\MoM.Api
dotnet ef database update --project .\MoM.Api.csproj --startup-project .\MoM.Api.csproj
```

If you changed the schema and need a new migration:

```powershell
dotnet ef migrations add YourMigrationName --project .\MoM.Api.csproj --startup-project .\MoM.Api.csproj
dotnet ef database update --project .\MoM.Api.csproj --startup-project .\MoM.Api.csproj
```

### 3. Run the backend

```powershell
cd .\MoM.Api
dotnet run
```

### 4. Run the frontend

Open a second terminal:

```powershell
cd .\MoM.Web
dotnet run
```

## Important Notes

### MVC proxy

The frontend does not call the backend directly from browser-side hardcoded URLs. It uses an MVC proxy controller under `/mom-api`, which forwards requests to the API.

### Select-or-create inputs

For the following fields, users can either select an existing record or type a new value:

- Venue
- Facilitator
- Chairperson
- Secretary
- Attendees
- Agenda owner
- Action item owner

If a new value is typed, the backend creates the corresponding `User` or `Venue` record and maps it automatically.

### Attendance logic

Attendance totals are derived from attendee mappings, not from manual count entry.

### Analytics

The dashboard currently includes:

- Total meetings
- Total present attendees
- Total absent attendees
- Meetings taken chart
- Present vs absent ratio chart
- User attendance count
- Venue-wise meeting count

Pagination is applied to the user attendance and venue activity sections on the dashboard.

### PDF generation

PDF export is handled by QuestPDF in the backend. Export includes:

- meeting details
- agenda
- actionable items
- attendee status list

## API Surface

### Meetings

- `GET /api/meetings`
- `GET /api/meetings/{id}`
- `POST /api/meetings`
- `PUT /api/meetings/{id}`
- `DELETE /api/meetings/{id}`
- `GET /api/meetings/stats`

### Lookups

- `GET /api/lookups/users`
- `GET /api/lookups/venues`

### Export

- `GET /api/export/{id}`

## Project Notes

- The backend applies pending EF Core migrations automatically at startup if migrations exist.
- Scalar is enabled in development and opens from the API launch profile.
- The frontend and backend use fixed local development ports for easier setup.

## Troubleshooting

### Migrations fail

Check:

- SQL Server / LocalDB availability
- connection string in `MoM.Api/appsettings.json`
- whether your existing local database is from an older incompatible schema

### Package restore fails

Common causes:

- blocked access to `nuget.org`
- SSL or certificate issues
- invalid private NuGet source configuration

### Build fails because files are locked

If you see `MSB3021` or `MSB3027`, stop any running `MoM.Api` or `MoM.Web` process and build again.

## Repository

GitHub repository:

`https://github.com/Prit-Kanani/MOM`
