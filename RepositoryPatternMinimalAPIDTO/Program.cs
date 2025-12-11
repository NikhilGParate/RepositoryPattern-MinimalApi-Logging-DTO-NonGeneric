using FluentValidation;
using Microsoft.EntityFrameworkCore;
using RepositoryPatternMinimalAPIDTO.Data;
using RepositoryPatternMinimalAPIDTO.Mapping;
using RepositoryPatternMinimalAPIDTO.Repositories;
using RepositoryPatternMinimalAPIDTO.Validation;
using AutoMapper;
using Microsoft.OpenApi.Models;
using RepositoryPatternMinimalAPIDTO.Models;
using RepositoryPatternMinimalAPIDTO.Middlewares;
using RepositoryPatternMinimalAPIDTO.Dtos;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddLogging();
// EF Core - choose provider. For production use SqlServer, for demo use InMemory
var useInMemory = true;
if (useInMemory)
{
    builder.Services.AddDbContext<AppDbContext>(opt => opt.UseInMemoryDatabase("ProductDb"));
}
else
{
    builder.Services.AddDbContext<AppDbContext>(opt => opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
}
// Repositories & AutoMapper
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddAutoMapper(typeof(MappingProfile));

// FluentValidation registration
builder.Services.AddValidatorsFromAssemblyContaining<ProductCreateValidator>();
//builder.Services.AddFluentValidationAutoValidation();


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Product API", Version = "v1" });
    c.SwaggerDoc("v2", new OpenApiInfo { Title = "Product API", Version = "v2" });
});

var app = builder.Build();
// Seed sample data for demo (in-memory)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (useInMemory)
    {
        if (!db.Products.Any())
        {
            db.Products.AddRange(new[]
            {
                new Product { Name = "Apple", Price = 10 },
                new Product { Name = "Banana", Price = 5 },
                new Product { Name = "Laptop", Price = 600 },
                new Product { Name = "Phone", Price = 400 },
            });
            db.SaveChanges();
        }
    }
}
// Middlewares
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Product API v1");
    c.SwaggerEndpoint("/swagger/v2/swagger.json", "Product API v2");
});

// Map grouped endpoints for versioning via route prefix
var v1 = app.MapGroup("/api/v1/products");
var v2 = app.MapGroup("/api/v2/products");

// resolve dependencies inside endpoints using DI
// Helper for validation: resolve IValidator<T> and run it manually
async Task<IResult> ValidateAndReturnBadRequestAsync<TDto>(TDto dto, IValidator<TDto>? validator)
{
    if (validator == null) return Results.Ok(); // no validator => ok
    var result = await validator.ValidateAsync(dto);
    if (result.IsValid) return Results.Ok();
    var errors = result.Errors.Select(e => new { e.PropertyName, e.ErrorMessage });
    return Results.BadRequest(new { errors });
}


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
/* -------------------------
   V1 Endpoints
   ------------------------- */

// GET /api/v1/products?name=app&minPrice=0&maxPrice=100&page=1&pageSize=10
v1.MapGet("/", async (IProductRepository repo, IMapper mapper, string? name, decimal? minPrice, decimal? maxPrice, int page = 1, int pageSize = 10) =>
{
    page = Math.Max(1, page);
    pageSize = Math.Clamp(pageSize, 1, 100);

    var total = await repo.CountAsync(name, minPrice, maxPrice);
    var items = await repo.GetFilteredAsync(name, minPrice, maxPrice, page, pageSize);

    var dto = items.Select(p => mapper.Map<ProductReadDto>(p));

    return Results.Ok(new
    {
        page,
        pageSize,
        total,
        data = dto
    });
}).WithName("GetProductsV1");

// GET /api/v1/products/{id}
v1.MapGet("/{id:int}", async (int id, IProductRepository repo, IMapper mapper) =>
{
    var p = await repo.GetByIdAsync(id);
    if (p == null) return Results.NotFound();
    return Results.Ok(mapper.Map<ProductReadDto>(p));
}).WithName("GetProductByIdV1");

// POST /api/v1/products
v1.MapPost("/", async (ProductCreateDto createDto, IProductRepository repo, IMapper mapper, IValidator<ProductCreateDto> validator) =>
{
    var validationResult = await validator.ValidateAsync(createDto);
    if (!validationResult.IsValid)
        return Results.BadRequest(validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));

    var entity = mapper.Map<Product>(createDto);
    var created = await repo.AddAsync(entity);
    return Results.Created($"/api/v1/products/{created.Id}", mapper.Map<ProductReadDto>(created));
});

// PUT /api/v1/products/{id}
v1.MapPut("/{id:int}", async (int id, ProductUpdateDto updateDto, IProductRepository repo, IMapper mapper, IValidator<ProductUpdateDto> validator) =>
{
    if (id != updateDto.Id) return Results.BadRequest(new { message = "Id mismatch" });

    var validation = await validator.ValidateAsync(updateDto);
    if (!validation.IsValid)
        return Results.BadRequest(validation.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));

    var existing = await repo.GetByIdAsync(id);
    if (existing == null) return Results.NotFound();

    // map updateDto to existing entity
    mapper.Map(updateDto, existing);
    await repo.UpdateAsync(existing);
    return Results.Ok(mapper.Map<ProductReadDto>(existing));
}).WithName("UpdateProductV1");

// DELETE /api/v1/products/{id}
v1.MapDelete("/{id:int}", async (int id, IProductRepository repo) =>
{
    var existing = await repo.GetByIdAsync(id);
    if (existing == null) return Results.NotFound();
    await repo.DeleteAsync(id);
    return Results.NoContent();
}).WithName("DeleteProductV1");


/* -------------------------
   V2 Endpoints (example of version differences)
   ------------------------- */

// v2 returns a slightly different shape (e.g., includes a currency field)
v2.MapGet("/", async (IProductRepository repo, IMapper mapper, string? name, decimal? minPrice, decimal? maxPrice, int page = 1, int pageSize = 10) =>
{
    page = Math.Max(1, page);
    pageSize = Math.Clamp(pageSize, 1, 100);

    var total = await repo.CountAsync(name, minPrice, maxPrice);
    var items = await repo.GetFilteredAsync(name, minPrice, maxPrice, page, pageSize);

    var dto = items.Select(p => {
        var r = mapper.Map<ProductReadDto>(p);
        return new { r.Id, r.Name, r.Price, Currency = "USD", r.CreatedAt };
    });

    return Results.Ok(new
    {
        page,
        pageSize,
        total,
        data = dto
    });
}).WithName("GetProductsV2");
app.Run();
