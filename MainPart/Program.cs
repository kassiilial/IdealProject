using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using HotChocolate;
using HotChocolate.AspNetCore;
using GraphQL;

var builder = WebApplication.CreateBuilder(args);

// Read toggles from configuration (appsettings.json)
// useSqlDatabase is true when you want to switch from the in-memory repo to a real SQL-backed repo
var useSqlDatabase = builder.Configuration.GetValue<bool>("UseSqlDatabase");
var useRedis = builder.Configuration.GetValue<bool>("UseRedisCache");

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Repository: in-memory by default, SQL when enabled
// Configure EF DbContext and repository
if (useSqlDatabase)
{
	var conn = builder.Configuration.GetConnectionString("DefaultConnection")
			   ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
	builder.Services.AddDbContext<Data.AppDbContext>(options =>
		options.UseSqlServer(conn));
	builder.Services.AddScoped<Services.IMyRepository, Services.EfRepository>();
}
else
{
	// Use EF InMemory provider but still use EfRepository so behavior is consistent with SQL path
	builder.Services.AddDbContext<Data.AppDbContext>(options =>
		options.UseInMemoryDatabase("InMemoryDb"));
	builder.Services.AddScoped<Services.IMyRepository, Services.EfRepository>();
}

// Distributed cache: in-memory by default, Redis when enabled
if (useRedis)
{
	var redisConn = builder.Configuration.GetConnectionString("RedisConnection")
					?? throw new InvalidOperationException("Connection string 'RedisConnection' is not configured but UseRedisCache is true.");
	builder.Services.AddStackExchangeRedisCache(options => options.Configuration = redisConn);
}
else
{
	builder.Services.AddDistributedMemoryCache();
}

// Cache settings
builder.Services.Configure<Config.CacheSettings>(builder.Configuration.GetSection("CacheSettings"));

// Cache wrapper: DistributedCacheService depends on IDistributedCache and IOptions<CacheSettings>
builder.Services.AddSingleton<Services.ICacheService, Services.DistributedCacheService>();

// GraphQL (HotChocolate)
builder.Services
	.AddGraphQLServer()
	.AddQueryType<Query>()
	.AddMutationType<Mutation>()
	.ModifyRequestOptions(opt => opt.IncludeExceptionDetails = builder.Environment.IsDevelopment());

var app = builder.Build();

// Only enable Swagger UI in Development to avoid middleware interaction during automated tests
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.MapGet("/", () => Results.Ok(new { message = "Hello from ASP.NET Core minimal API on .NET 10 (LTS)!" }));
app.MapGet("/health", () => Results.Ok("Healthy"));

// Demo endpoints for items
app.MapGet("/items", async (Services.IMyRepository repo) =>
{
	var items = await repo.GetAllAsync();
	return Results.Ok(items);
});

// Map GraphQL endpoint
app.MapGraphQL("/graphql");

app.MapGet("/items/{id}", async (int id, Services.IMyRepository repo, Services.ICacheService cache) =>
{
	var cacheKey = $"item:{id}";
	var cached = await cache.GetAsync<Models.Item>(cacheKey);
	if (cached is not null) return Results.Ok(cached);

	var item = await repo.GetByIdAsync(id);
	if (item is null) return Results.NotFound();
	await cache.SetAsync(cacheKey, item, TimeSpan.FromMinutes(5));
	return Results.Ok(item);
});

app.MapPost("/items", async (Models.Item item, Services.IMyRepository repo) =>
{
	var created = await repo.AddAsync(item);
	return Results.Created($"/items/{created.Id}", created);
});

app.MapPut("/items/{id}", async (int id, Models.Item item, Services.IMyRepository repo) =>
{
	if (id != item.Id) return Results.BadRequest("Id mismatch");
	var updated = await repo.UpdateAsync(item);
	return Results.Ok(updated);
});

app.MapDelete("/items/{id}", async (int id, Services.IMyRepository repo) =>
{
	var deleted = await repo.DeleteAsync(id);
	if (!deleted) return Results.NotFound();
	return Results.NoContent();
});

app.Run();
