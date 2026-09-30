# Project Context

This file records the project-specific context established in the current workspace. It is a snapshot, not a complete source-code audit.

## Saved Memory

- No user, session, or repository memory notes were present when this file was created.
- Details below come from the visible workspace structure and the files inspected for this snapshot.
- The workspace listing may be incomplete; implementation details not inspected are intentionally not inferred.

## Workspace

- Workspace: `VehicleRecallWorkspace`
- Environment: Windows
- Solution is organized into three projects:
  - `RecallOperations.Api`: ASP.NET Core Web API.
  - `VehicleRecall.Shared`: shared .NET models and project references.
  - `VehicleRecallApp`: ASP.NET Core web application; its UI implementation was not inspected for this snapshot.
- All three project files target .NET 10 (`net10.0`) and enable nullable reference types and implicit usings.

## API Notes

Verified in `RecallOperations.Api/Program.cs`:

- Controllers are registered, and Swagger is enabled at `/swagger`.
- Swagger documents bearer-token authentication. The API uses JWT bearer authentication and a `ManagerOnly` authorization policy.
- JWT configuration reads `Jwt:Issuer`, `Jwt:Audience`, and `Jwt:SigningKey`. The signing key is required and must be at least 32 UTF-8 bytes; the exception message directs developers to .NET User Secrets.
- SQL Server is configured using the `DefaultConnection` connection string.
- CORS allows `http://localhost:5048` with any header and method.
- An exception handler returns a JSON 500 response for unexpected server errors.
- Registered services include `UserStore`, `JwtTokenService`, and an ASP.NET Core password hasher for the shared `UserAccount` model.

## Visible API Structure

- Controllers: `CustomersController.cs`, `Systemcontroller.cs`, `UsersController.cs`, and `VehiclesController.cs`.
- Data: `UserDbContext.cs`, `users.json`, and `Registration-invitations.json`.
- Services: `JwtTokenService.cs` and `UserStore.cs`.
- EF Core migrations include user/authentication setup, customer and vehicle tables, seed data, stored procedures, and customer contact-address persistence.

## Project Dependencies

- `RecallOperations.Api` references `VehicleRecall.Shared` and uses ASP.NET Core, JWT bearer authentication, EF Core SQL Server, and Swashbuckle.
- `VehicleRecallApp` references `VehicleRecall.Shared` and Swashbuckle.
- `VehicleRecall.Shared` is a .NET class library.

## Known Limits

- Controller behavior, data models beyond their visible filenames, frontend pages, tests, and runtime configuration values were not reviewed when creating this snapshot.
- Do not add secrets or populated credentials to this document.