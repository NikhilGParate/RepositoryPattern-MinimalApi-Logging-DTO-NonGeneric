# RepositoryPattern-MinimalApi-Logging-DTO-NonGeneric
You now have a modern, clean, production-ready repository pattern setup with:

Minimal API + versioning

Async CRUD

Logging

Validation

DTOs + AutoMapper

Filtering + Pagination

Exception handling

This is a full foundation for any enterprise-ready ASP.NET Core API project.

Table of Contents

Overview

Features

Getting Started

Project Structure

Technologies Used

Setup & Run

API Endpoints

Logging

Validation

AutoMapper

Pagination & Filtering

Unit Testing

Future Improvements

License

Overview

This project demonstrates a production-ready ASP.NET Core 8 Minimal API implementation of a non-generic repository pattern with the following features:

Async CRUD operations for Product entity

DTOs (Data Transfer Objects) and AutoMapper

FluentValidation for request validation

Logging with built-in ASP.NET Core ILogger

Exception handling middleware

API versioning support

Pagination and filtering support

Unit testing with EF Core InMemory provider

This architecture is ideal for small to medium enterprise applications and provides a clean separation of concerns.

Features

✅ Non-Generic Repository Pattern for Product

✅ Minimal API endpoints

✅ Async CRUD operations

✅ DTOs & AutoMapper

✅ Validation using FluentValidation

✅ Logging at repository and endpoint levels

✅ Global exception handling

✅ API versioning (v1, v2 example)

✅ Pagination & filtering in GET /products

✅ Unit test coverage using EF Core InMemory

Project Structure
ProductApi/
├─ Data/           → DbContext
├─ Models/         → Entity models (Product)
├─ Dtos/           → Request/Response DTOs
├─ Repositories/   → Repository interface + implementation
├─ Validation/     → FluentValidation validators
├─ Mapping/        → AutoMapper profiles
├─ Middlewares/    → Global exception handling
├─ Program.cs      → Minimal API endpoints + DI + versioning
├─ appsettings.json → Logging & DB config
├─ Tests/          → Unit tests for repository

Technologies Used

.NET 8 Minimal API

Entity Framework Core (InMemory / SQL Server)

FluentValidation

AutoMapper

ILogger / Microsoft.Extensions.Logging

xUnit (for unit testing)

Setup & Run

Clone the repository:

git clone https://github.com/<username>/MinimalApi-RepositoryPattern.git
cd MinimalApi-RepositoryPattern


Install NuGet packages:

dotnet restore


Run the application:

dotnet run


Access API at:

https://localhost:5001/api/v1/products

API Endpoints
v1: /api/v1/products
Method	Endpoint	Description
GET	/	List all products (with pagination & filtering)
GET	/{id}	Get product by ID
POST	/	Create a new product
PUT	/{id}	Update existing product
DELETE	/{id}	Delete product by ID

Query parameters for GET /products:

name → filter by product name

minPrice → minimum price filter

maxPrice → maximum price filter

page → page number (default 1)

pageSize → page size (default 10)

Logging

Logs are written using ASP.NET Core ILogger.

Current logs go to:

Console (terminal or VS Output window)

Debug output (Visual Studio)

Repository example:

_logger.LogInformation("Product added with id {Id} and name {Name}", product.Id, product.Name);


Global exception logging is done via ExceptionHandlingMiddleware:

_logger.LogError(ex, "Unhandled exception occurred");


For production, you can extend logging to files or Seq/ELK using Serilog.

Validation

Request validation is done using FluentValidation:

public class ProductCreateValidator : AbstractValidator<ProductCreateDto>
{
    public ProductCreateValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
    }
}


Validates DTOs before repository operations.

Returns structured validation errors in API response.

AutoMapper

DTOs are mapped to entities using AutoMapper:

CreateMap<ProductCreateDto, Product>();
CreateMap<ProductUpdateDto, Product>();
CreateMap<Product, ProductReadDto>();


Simplifies mapping in endpoints:

var entity = mapper.Map<Product>(createDto);

Pagination & Filtering

Implemented in repository:

public async Task<IEnumerable<Product>> GetFilteredAsync(string? name, decimal? minPrice, decimal? maxPrice, int pageNumber, int pageSize)
{
    var query = _db.Products.AsQueryable();
    if (!string.IsNullOrEmpty(name)) query = query.Where(p => p.Name.Contains(name));
    if (minPrice.HasValue) query = query.Where(p => p.Price >= minPrice.Value);
    if (maxPrice.HasValue) query = query.Where(p => p.Price <= maxPrice.Value);

    return await query
        .Skip((pageNumber - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();
}


Supports large datasets efficiently.

Unit Testing

Repository tested using xUnit + EF Core InMemory provider.

Example:

[Fact]
public async Task AddAsync_ShouldAddProduct()
{
    var options = new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase("TestDb").Options;
    var context = new AppDbContext(options);
    var logger = new LoggerFactory().CreateLogger<ProductRepository>();
    var repo = new ProductRepository(context, logger);

    var product = new Product { Name = "Test", Price = 100 };
    var result = await repo.AddAsync(product);

    Assert.NotNull(result);
    Assert.Equal("Test", result.Name);
}

Future Improvements

Add JWT authentication & authorization

Use Serilog to log to files / database / cloud

Implement soft delete

Add sorting / advanced filtering

Add Swagger API documentation with examples

Add integration tests
