# Development Guidelines

## 1. Build & Configuration

### Environment Requirements
- **Runtime**: .NET 10.0 SDK
- **Database**: SQL Server
- **IDE**: JetBrains Rider (recommended) or Visual Studio

### Project Setup
1. **Restore Dependencies**: Run `dotnet restore` to install all necessary NuGet packages.
2. **Configuration**: 
   - Ensure `appsettings.json` (or `appsettings.Development.json`) has a valid `AppSetting:DbConnectionString`.
   - JWT settings should be configured in the `AppSetting:Jwt` section.
   - Other settings like `AzureStorage`, `BaseUrl`, and `SmtpSettings` are also configured within the `AppSetting` block.
3. **Database Initialization**: 
   - The application is configured to run `context.Database.Migrate()` and `DbSeeder.SeedAsync()` on startup.
   - Simply running the project will apply migrations and seed initial data.

### Build & Run
- Use `dotnet build` to compile the project.
- Use `dotnet run` to start the API.
- The API will be available at the URL configured in `Properties/launchSettings.json`.

---

## 2. Architecture & Development Information

### Patterns & Libraries
- **CQRS**: Implemented using **MediatR**. Commands and Queries are separated from their handlers.
- **Validation**: Uses **FluentValidation**. Validation is integrated into the MediatR pipeline via `ValidationPipelineBehavior`.
- **Object Mapping**: Uses **AutoMapper** for mapping between Entities and DTOs.
- **Data Access**: Uses **Entity Framework Core** for ORM and **Dapper** for specific high-performance queries if needed.
- **Authentication**: **JWT Bearer** authentication is used.
- **Global Exception Handling**: Handled via `ExceptionMiddleware`.

### API Responses
- All API responses should follow the standardized `ApiResponse` format.
- Use `ApiHelperResponse.Success(...)` to create successful responses.
- Throw `ApiException` for business rule violations or expected errors to be caught by the middleware.

### Debugging
- Swagger is enabled in the Development environment for easy API testing.
- JWT Authentication is supported in Swagger.
- `ExceptionMiddleware` provides consistent error logging and responses.

---

## 3. Coding Conventions

### Language Features
- **Primary Constructors**: Use C# 12 primary constructors for dependency injection in classes and handlers.
- **File-Scoped Namespaces**: Prefer file-scoped namespaces (`namespace HRManagement.Api.Something;`) for cleaner code.
- **Nullable Reference Types**: Enabled by default; use `?` for optional properties/parameters.

### MediatR Pattern
- **Nested Handlers**: It is a convention in this project to nest the `Handler` class inside the `Command` or `Query` class it handles.
  ```csharp
  public class MyCommand : IRequest<ApiResponse<string>>
  {
      public class Handler(...) : IRequestHandler<MyCommand, ApiResponse<string>>
      {
          public async Task<ApiResponse<string>> Handle(...) { ... }
      }
  }
  ```

### Naming Conventions
- **Classes/Interfaces/Methods/Properties**: `PascalCase`.
- **Local Variables/Parameters**: `camelCase`.
- **Private Fields**: `_camelCase` (e.g., `private readonly IMyService _myService;`).
- **Interfaces**: Prefix with `I` (e.g., `IEmployeeRepository`).

### Repository Pattern
- Repositories should be registered as `Scoped` in `DependencyContainer`.
- Use the `IUnitOfWork` if multiple repository operations need to be atomic, although direct repository usage is common in handlers.

### Project Structure
- **Application**: Contains Commands, Queries, DTOs, Mappings, and Validators.
- **Domain**: Contains Entities (Models/Tables), Enums, and Core Response types.
- **Repositories**: Contains DbContext, Repository implementations, and Seeder.
- **Extensions**: Contains Dependency Injection setup and Middlewares.
- **Controllers**: Thin controllers that delegate work to MediatR.
