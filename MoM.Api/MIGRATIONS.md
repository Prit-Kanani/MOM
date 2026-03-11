# Backend Migration Steps

Run these commands from the `MoM.Api` folder:

```powershell
dotnet ef migrations add InitialCreate --project .\MoM.Api.csproj --startup-project .\MoM.Api.csproj
dotnet ef database update --project .\MoM.Api.csproj --startup-project .\MoM.Api.csproj
dotnet run
```

Notes:

- The API is already configured for SQL Server in `appsettings.json`.
- On startup, the app will apply migrations automatically if they exist.
- After migration, you can use the MVC frontend to create, edit, delete, and export MOM records.
- If you ran the API earlier and it created the database another way, delete the existing `MoM_DB` database first, then run the commands above.
	