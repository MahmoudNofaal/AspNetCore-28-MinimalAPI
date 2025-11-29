# Minimal APIs in ASP.NET Core

## Table of Contents
1. [Introduction](#introduction)
2. [Getting Started](#getting-started)
3. [Route Parameters](#route-parameters)
4. [Organizing Routes with MapGroups](#organizing-routes-with-mapgroups)
5. [HTTP Response Types (IResult)](#http-response-types-iresult)
6. [Endpoint Filters](#endpoint-filters)
7. [Best Practices](#best-practices)
8. [Quick Reference](#quick-reference)

---

## Introduction

**Minimal APIs** in ASP.NET Core provide a streamlined approach to building HTTP APIs with minimal code and configuration. They eliminate the need for controllers and reduce boilerplate code, making them ideal for:

- Small APIs and microservices
- Prototyping and rapid development
- Lightweight applications
- Lambda-style HTTP endpoints

### Key Benefits

✅ **Less Boilerplate** - No need for controllers or action methods  
✅ **Simplified Configuration** - Define routes directly in `Program.cs`  
✅ **Performance** - Reduced overhead compared to controller-based APIs  
✅ **Modern Syntax** - Leverages C# features like lambda expressions  
✅ **Flexibility** - Mix with traditional controllers when needed

---

## Getting Started

### Basic Setup

Create a minimal API in `Program.cs`:

```csharp
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/hello", () => "Hello, world!");

app.Run();
```

**Explanation:**
- `WebApplication.CreateBuilder(args)` - Creates the application builder
- `app.MapGet()` - Maps an HTTP GET request to a handler
- `app.Run()` - Starts the application and listens for requests

### HTTP Verb Methods

```csharp
// GET request
app.MapGet("/api/products", () => "Get all products");

// POST request
app.MapPost("/api/products", (Product product) => "Create product");

// PUT request
app.MapPut("/api/products/{id}", (int id, Product product) => "Update product");

// DELETE request
app.MapDelete("/api/products/{id}", (int id) => "Delete product");

// PATCH request
app.MapPatch("/api/products/{id}", (int id, JsonPatchDocument patch) => "Partial update");
```

### Multiple Endpoints Example

```csharp
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Welcome to the API");
app.MapGet("/about", () => "About this API");
app.MapGet("/health", () => Results.Ok(new { Status = "Healthy" }));

app.Run();
```

---

## Route Parameters

Route parameters allow you to capture values from the URL and use them in your handlers.

### Basic Route Parameter

```csharp
app.MapGet("/greet/{name}", (string name) => 
    $"Hello, {name}!");
```

**Request:** `GET /greet/Alice`  
**Response:** `"Hello, Alice!"`

### Multiple Route Parameters

```csharp
app.MapGet("/users/{userId}/posts/{postId}", (int userId, int postId) => 
    $"User {userId}, Post {postId}");
```

**Request:** `GET /users/123/posts/456`  
**Response:** `"User 123, Post 456"`

### Optional Route Parameters

```csharp
app.MapGet("/search/{query?}", (string? query) => 
    query is null ? "No search query provided" : $"Searching for: {query}");
```

**Request:** `GET /search`  
**Response:** `"No search query provided"`

**Request:** `GET /search/books`  
**Response:** `"Searching for: books"`

### Route Constraints

```csharp
// Only accept integers
app.MapGet("/products/{id:int}", (int id) => 
    $"Product ID: {id}");

// Only accept strings with minimum length
app.MapGet("/category/{name:minlength(3)}", (string name) => 
    $"Category: {name}");

// Multiple constraints
app.MapGet("/items/{id:int:min(1)}", (int id) => 
    $"Item ID: {id}");
```

**Common Constraints:**
- `:int` - Integer values only
- `:bool` - Boolean values
- `:datetime` - DateTime values
- `:decimal` - Decimal values
- `:guid` - GUID values
- `:length(6)` - Exact length
- `:minlength(3)` - Minimum length
- `:maxlength(10)` - Maximum length
- `:min(1)` - Minimum value
- `:max(100)` - Maximum value
- `:range(1,100)` - Value range
- `:alpha` - Alphabetic characters only
- `:regex(pattern)` - Regular expression match

### Query String Parameters

```csharp
app.MapGet("/search", (string? query, int page = 1, int pageSize = 10) => 
    $"Query: {query}, Page: {page}, Size: {pageSize}");
```

**Request:** `GET /search?query=books&page=2&pageSize=20`  
**Response:** `"Query: books, Page: 2, Size: 20"`

---

## Organizing Routes with MapGroups

`MapGroups` organizes related routes under a common prefix, improving code organization and maintainability.

### Basic Group

```csharp
var apiGroup = app.MapGroup("/api");

apiGroup.MapGet("/products", () => new[] { "Product1", "Product2" });
apiGroup.MapGet("/orders", () => new[] { "Order1", "Order2" });
```

**Endpoints Created:**
- `GET /api/products`
- `GET /api/orders`

### Nested Groups

```csharp
var api = app.MapGroup("/api");
var v1 = api.MapGroup("/v1");

v1.MapGet("/users", () => "Get users v1");
v1.MapGet("/products", () => "Get products v1");
```

**Endpoints Created:**
- `GET /api/v1/users`
- `GET /api/v1/products`

### Groups with Common Configuration

```csharp
var secureGroup = app.MapGroup("/secure")
    .RequireAuthorization()
    .AddEndpointFilter<LoggingFilter>();

secureGroup.MapGet("/data", () => "Secure data");
secureGroup.MapGet("/settings", () => "Secure settings");
```

Both endpoints now require authorization and use the logging filter.

### Complete Example: API Versioning

```csharp
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// API v1
var v1 = app.MapGroup("/api/v1");
v1.MapGet("/products", () => new[] { "Product1", "Product2" });
v1.MapPost("/products", (Product product) => Results.Created($"/api/v1/products/{product.Id}", product));

// API v2
var v2 = app.MapGroup("/api/v2");
v2.MapGet("/products", () => new[] { "ProductA", "ProductB", "ProductC" });
v2.MapPost("/products", (Product product) => Results.Created($"/api/v2/products/{product.Id}", product));

app.Run();
```

### Practical Example: Resource-Based Groups

```csharp
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Products group
var products = app.MapGroup("/api/products");
products.MapGet("/", GetAllProducts);
products.MapGet("/{id:int}", GetProductById);
products.MapPost("/", CreateProduct);
products.MapPut("/{id:int}", UpdateProduct);
products.MapDelete("/{id:int}", DeleteProduct);

// Orders group
var orders = app.MapGroup("/api/orders");
orders.MapGet("/", GetAllOrders);
orders.MapGet("/{id:int}", GetOrderById);
orders.MapPost("/", CreateOrder);

app.Run();

// Handler methods
static IResult GetAllProducts() => Results.Ok(new[] { "Product1", "Product2" });
static IResult GetProductById(int id) => Results.Ok(new { Id = id, Name = "Product" });
static IResult CreateProduct(Product product) => Results.Created($"/api/products/{product.Id}", product);
static IResult UpdateProduct(int id, Product product) => Results.NoContent();
static IResult DeleteProduct(int id) => Results.NoContent();
static IResult GetAllOrders() => Results.Ok(new[] { "Order1", "Order2" });
static IResult GetOrderById(int id) => Results.Ok(new { Id = id, Status = "Pending" });
static IResult CreateOrder(Order order) => Results.Created($"/api/orders/{order.Id}", order);
```

---

## HTTP Response Types (IResult)

`IResult` represents the result of an HTTP request and provides a consistent way to return various HTTP responses.

### Success Responses

```csharp
// 200 OK
app.MapGet("/status", () => 
    Results.Ok(new { Status = "Running", Timestamp = DateTime.UtcNow }));

// 201 Created (with location header)
app.MapPost("/products", (Product product) => 
    Results.Created($"/products/{product.Id}", product));

// 202 Accepted
app.MapPost("/process", () => 
    Results.Accepted("/status/123", new { JobId = 123 }));

// 204 No Content
app.MapDelete("/products/{id}", (int id) => 
    Results.NoContent());
```

### Client Error Responses

```csharp
// 400 Bad Request
app.MapPost("/validate", (User user) => 
    string.IsNullOrEmpty(user.Email) 
        ? Results.BadRequest("Email is required") 
        : Results.Ok(user));

// 401 Unauthorized
app.MapGet("/secure", () => 
    Results.Unauthorized());

// 403 Forbidden
app.MapGet("/admin", () => 
    Results.Forbid());

// 404 Not Found
app.MapGet("/users/{id}", (int id) => 
    id > 0 
        ? Results.Ok(new { Id = id }) 
        : Results.NotFound($"User {id} not found"));

// 409 Conflict
app.MapPost("/users", (User user) => 
    Results.Conflict("User already exists"));

// 422 Unprocessable Entity
app.MapPost("/validate", (User user) => 
    Results.UnprocessableEntity(new { Errors = new[] { "Invalid data" } }));
```

### Server Error Responses

```csharp
// 500 Internal Server Error
app.MapGet("/error", () => 
    Results.Problem("An error occurred"));

// 503 Service Unavailable
app.MapGet("/maintenance", () => 
    Results.StatusCode(503));
```

### Redirect Responses

```csharp
// 301 Moved Permanently
app.MapGet("/old-path", () => 
    Results.RedirectPermanent("/new-path"));

// 302 Found (temporary redirect)
app.MapGet("/redirect", () => 
    Results.Redirect("/target"));

// 307 Temporary Redirect (preserves method)
app.MapGet("/temp-redirect", () => 
    Results.RedirectToRoute("routeName"));
```

### File Responses

```csharp
// Return file
app.MapGet("/download", () => 
    Results.File("path/to/file.pdf", "application/pdf", "document.pdf"));

// Return stream
app.MapGet("/stream", () => 
    Results.Stream(stream, "application/octet-stream"));

// Return bytes
app.MapGet("/image", () => 
    Results.Bytes(imageBytes, "image/png"));
```

### Custom Status Codes

```csharp
// Custom status code
app.MapGet("/custom", () => 
    Results.StatusCode(418)); // I'm a teapot

// JSON response with custom status
app.MapGet("/custom-json", () => 
    Results.Json(new { Message = "Custom" }, statusCode: 299));
```

### Validation Problem Details

```csharp
app.MapPost("/users", (User user) =>
{
    var errors = new Dictionary<string, string[]>
    {
        { "Email", new[] { "Email is required", "Email format is invalid" } },
        { "Name", new[] { "Name is too short" } }
    };
    
    return Results.ValidationProblem(errors);
});
```

### Complete Response Example

```csharp
app.MapGet("/products/{id:int}", (int id, IProductService service) =>
{
    try
    {
        var product = service.GetById(id);
        
        if (product is null)
            return Results.NotFound(new { Message = $"Product {id} not found" });
        
        return Results.Ok(product);
    }
    catch (Exception ex)
    {
        return Results.Problem(
            detail: ex.Message,
            statusCode: 500,
            title: "An error occurred");
    }
});
```

---

## Endpoint Filters

Endpoint filters allow you to execute custom logic before or after request handlers, similar to middleware but scoped to specific endpoints.

### Use Cases for Filters

- Request/response logging
- Validation
- Authentication/authorization checks
- Performance monitoring
- Request transformation
- Error handling

### IEndpointFilter Interface

```csharp
public interface IEndpointFilter
{
    Task<object?> InvokeAsync(
        EndpointFilterInvocationContext context, 
        EndpointFilterInvocationDelegate next);
}
```

### Basic Filter Implementation

```csharp
public class LoggingFilter : IEndpointFilter
{
    public async Task<object?> InvokeAsync(
        EndpointFilterInvocationContext context, 
        EndpointFilterInvocationDelegate next)
    {
        Console.WriteLine($"Request: {context.HttpContext.Request.Method} {context.HttpContext.Request.Path}");
        
        var result = await next(context);
        
        Console.WriteLine($"Response: {context.HttpContext.Response.StatusCode}");
        
        return result;
    }
}
```

### Applying Filters to Endpoints

```csharp
// Single endpoint
app.MapGet("/data", () => "Some data")
   .AddEndpointFilter<LoggingFilter>();

// Multiple filters (executed in order)
app.MapGet("/secure-data", () => "Secure data")
   .AddEndpointFilter<AuthenticationFilter>()
   .AddEndpointFilter<LoggingFilter>()
   .AddEndpointFilter<ValidationFilter>();

// Apply to group
var api = app.MapGroup("/api")
    .AddEndpointFilter<LoggingFilter>();

api.MapGet("/products", GetProducts);
api.MapGet("/orders", GetOrders);
```

### Validation Filter Example

```csharp
public class ValidationFilter : IEndpointFilter
{
    public async Task<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterInvocationDelegate next)
    {
        // Get the first argument (assuming it's the model to validate)
        var model = context.Arguments.FirstOrDefault();
        
        if (model is null)
        {
            return Results.BadRequest("Model is required");
        }
        
        // Perform validation (example using System.ComponentModel.DataAnnotations)
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(model);
        
        if (!Validator.TryValidateObject(model, validationContext, validationResults, true))
        {
            var errors = validationResults
                .Select(v => v.ErrorMessage)
                .ToArray();
            
            return Results.ValidationProblem(
                errors.ToDictionary(e => "model", e => new[] { e }));
        }
        
        return await next(context);
    }
}

// Usage
app.MapPost("/users", (User user) => Results.Created($"/users/{user.Id}", user))
   .AddEndpointFilter<ValidationFilter>();
```

### Performance Monitoring Filter

```csharp
public class PerformanceFilter : IEndpointFilter
{
    private readonly ILogger<PerformanceFilter> _logger;
    
    public PerformanceFilter(ILogger<PerformanceFilter> logger)
    {
        _logger = logger;
    }
    
    public async Task<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterInvocationDelegate next)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            return await next(context);
        }
        finally
        {
            stopwatch.Stop();
            _logger.LogInformation(
                "Request {Method} {Path} completed in {ElapsedMs}ms",
                context.HttpContext.Request.Method,
                context.HttpContext.Request.Path,
                stopwatch.ElapsedMilliseconds);
        }
    }
}
```

### Authentication Filter Example

```csharp
public class ApiKeyAuthFilter : IEndpointFilter
{
    private const string ApiKeyHeaderName = "X-API-Key";
    private readonly string _validApiKey;
    
    public ApiKeyAuthFilter(IConfiguration configuration)
    {
        _validApiKey = configuration["ApiKey"] ?? throw new InvalidOperationException("API Key not configured");
    }
    
    public async Task<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterInvocationDelegate next)
    {
        if (!context.HttpContext.Request.Headers.TryGetValue(ApiKeyHeaderName, out var apiKey))
        {
            return Results.Unauthorized();
        }
        
        if (apiKey != _validApiKey)
        {
            return Results.Unauthorized();
        }
        
        return await next(context);
    }
}

// Usage
app.MapGet("/admin/data", () => "Admin data")
   .AddEndpointFilter<ApiKeyAuthFilter>();
```

### Short-Circuit Filter (Early Exit)

```csharp
public class CacheFilter : IEndpointFilter
{
    private readonly IMemoryCache _cache;
    
    public CacheFilter(IMemoryCache cache)
    {
        _cache = cache;
    }
    
    public async Task<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterInvocationDelegate next)
    {
        var cacheKey = context.HttpContext.Request.Path.ToString();
        
        // Check cache first
        if (_cache.TryGetValue(cacheKey, out var cachedResult))
        {
            // Short-circuit: return cached result without calling next
            return cachedResult;
        }
        
        // Not in cache, call next filter/endpoint
        var result = await next(context);
        
        // Cache the result
        _cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));
        
        return result;
    }
}
```

### Inline Filter (Without Separate Class)

```csharp
app.MapGet("/products", () => "Products")
   .AddEndpointFilter(async (context, next) =>
   {
       Console.WriteLine("Before handler");
       var result = await next(context);
       Console.WriteLine("After handler");
       return result;
   });
```

### Filter with Dependency Injection

```csharp
public class ServiceFilter : IEndpointFilter
{
    private readonly IMyService _service;
    private readonly ILogger<ServiceFilter> _logger;
    
    public ServiceFilter(IMyService service, ILogger<ServiceFilter> logger)
    {
        _service = service;
        _logger = logger;
    }
    
    public async Task<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterInvocationDelegate next)
    {
        // Use injected services
        await _service.DoSomethingAsync();
        _logger.LogInformation("Filter executed");
        
        return await next(context);
    }
}

// Register filter dependencies
builder.Services.AddScoped<IMyService, MyService>();
```

### Complete Example: Multiple Filters

```csharp
var builder = WebApplication.CreateBuilder(args);

// Register services for filters
builder.Services.AddMemoryCache();
builder.Services.AddScoped<IProductService, ProductService>();

var app = builder.Build();

// Apply multiple filters to a group
var api = app.MapGroup("/api")
    .AddEndpointFilter<LoggingFilter>()
    .AddEndpointFilter<PerformanceFilter>();

// Secure endpoints with additional authentication filter
var secureApi = api.MapGroup("/secure")
    .AddEndpointFilter<ApiKeyAuthFilter>();

secureApi.MapGet("/products", GetProducts)
    .AddEndpointFilter<CacheFilter>(); // Endpoint-specific cache filter

secureApi.MapPost("/products", CreateProduct)
    .AddEndpointFilter<ValidationFilter>(); // Endpoint-specific validation

app.Run();
```

---

## Best Practices

### 1. Organization

✅ **Use MapGroups for Related Endpoints**
```csharp
var products = app.MapGroup("/api/products");
products.MapGet("/", GetAllProducts);
products.MapPost("/", CreateProduct);
```

✅ **Separate Handler Logic**
```csharp
// Instead of inline handlers
app.MapGet("/products", () => /* complex logic */);

// Use separate methods
app.MapGet("/products", GetProducts);

static IResult GetProducts() 
{
    // Handler logic
    return Results.Ok(products);
}
```

✅ **Use Descriptive Route Names**
```csharp
app.MapGet("/products/{id}", GetProductById)
   .WithName("GetProduct");

app.MapPost("/products", (Product product) => 
    Results.CreatedAtRoute("GetProduct", new { id = product.Id }, product));
```

### 2. Security

✅ **Use HTTPS**
```csharp
if (app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}
```

✅ **Implement Authentication/Authorization**
```csharp
app.MapGet("/secure", () => "Secure data")
   .RequireAuthorization();

app.MapGet("/admin", () => "Admin data")
   .RequireAuthorization("AdminPolicy");
```

✅ **Validate Input**
```csharp
app.MapPost("/products", (Product product) =>
{
    if (string.IsNullOrEmpty(product.Name))
        return Results.BadRequest("Name is required");
    
    return Results.Created($"/products/{product.Id}", product);
});
```

### 3. Error Handling

✅ **Use Try-Catch for Exceptions**
```csharp
app.MapGet("/products/{id}", (int id) =>
{
    try
    {
        var product = GetProduct(id);
        return Results.Ok(product);
    }
    catch (NotFoundException)
    {
        return Results.NotFound();
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});
```

✅ **Use Global Exception Handler**
```csharp
app.UseExceptionHandler("/error");

app.MapGet("/error", () => 
    Results.Problem("An error occurred"));
```

### 4. Documentation

✅ **Add OpenAPI/Swagger**
```csharp
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
```

✅ **Use WithDescription and WithTags**
```csharp
app.MapGet("/products", GetProducts)
   .WithName("GetProducts")
   .WithDescription("Retrieves all products")
   .WithTags("Products");
```

✅ **Document Response Types**
```csharp
app.MapGet("/products/{id}", GetProductById)
   .Produces<Product>(200)
   .Produces(404);
```

### 5. Performance

✅ **Use Async Handlers**
```csharp
app.MapGet("/products", async (IProductService service) => 
    await service.GetAllAsync());
```

✅ **Implement Caching**
```csharp
app.MapGet("/products", async (IProductService service) =>
{
    var products = await service.GetAllAsync();
    return Results.Ok(products);
})
.CacheOutput(builder => builder.Expire(TimeSpan.FromMinutes(5)));
```

✅ **Use Response Compression**
```csharp
builder.Services.AddResponseCompression();
app.UseResponseCompression();
```

### 6. Testing

✅ **Make Handlers Testable**
```csharp
// Testable handler
public static class ProductHandlers
{
    public static async Task<IResult> GetAll(IProductService service)
    {
        var products = await service.GetAllAsync();
        return Results.Ok(products);
    }
}

// In Program.cs
app.MapGet("/products", ProductHandlers.GetAll);
```

---

## Quick Reference

### HTTP Verb Mappings
```csharp
app.MapGet("/path", handler);       // GET
app.MapPost("/path", handler);      // POST
app.MapPut("/path", handler);       // PUT
app.MapDelete("/path", handler);    // DELETE
app.MapPatch("/path", handler);     // PATCH
app.MapMethods("/path", ["GET", "POST"], handler); // Multiple
```

### Common IResult Methods
```csharp
Results.Ok(data)                    // 200
Results.Created(location, data)     // 201
Results.Accepted(location, data)    // 202
Results.NoContent()                 // 204
Results.BadRequest(error)           // 400
Results.Unauthorized()              // 401
Results.Forbid()                    // 403
Results.NotFound(message)           // 404
Results.Conflict(message)           // 409
Results.UnprocessableEntity(data)   // 422
Results.Problem(detail)             // 500
Results.StatusCode(code)            // Custom
```

### Route Constraints
```csharp
{id:int}                // Integer
{id:guid}               // GUID
{name:alpha}            // Letters only
{name:minlength(3)}     // Min length
{id:range(1,100)}       // Value range
{name:regex(^[a-z]+$)}  // Regex pattern
```

### Filter Template
```csharp
public class MyFilter : IEndpointFilter
{
    public async Task<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterInvocationDelegate next)
    {
        // Before handler logic
        
        var result = await next(context);
        
        // After handler logic
        
        return result;
    }
}
```

### Configuration Checklist
- [ ] Add endpoints with appropriate HTTP verbs
- [ ] Use route constraints for parameters
- [ ] Group related routes with MapGroups
- [ ] Return appropriate IResult types
- [ ] Add endpoint filters for cross-cutting concerns
- [ ] Implement authentication/authorization
- [ ] Add error handling
- [ ] Configure OpenAPI/Swagger
- [ ] Add response compression
- [ ] Implement caching where appropriate
- [ ] Write tests for handlers

---

## Complete Example Application

```csharp
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("Products"));
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAuthentication().AddJwtBearer();
builder.Services.AddAuthorization();

var app = builder.Build();

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Routes
var api = app.MapGroup("/api");

// Products endpoints
var products = api.MapGroup("/products")
    .WithTags("Products")
    .AddEndpointFilter<LoggingFilter>();

products.MapGet("/", async (IProductService service) =>
    Results.Ok(await service.GetAllAsync()))
    .WithName("GetProducts")
    .Produces<List<Product>>(200);

products.MapGet("/{id:int}", async (int id, IProductService service) =>
{
    var product = await service.GetByIdAsync(id);
    return product is not null ? Results.Ok(product) : Results.NotFound();
})
    .WithName("GetProduct")
    .Produces<Product>(200)
    .Produces(404);

products.MapPost("/", async (Product product, IProductService service) =>
{
    await service.CreateAsync(product);
    return Results.CreatedAtRoute("GetProduct", new { id = product.Id }, product);
})
    .RequireAuthorization()
    .AddEndpointFilter<ValidationFilter>()
    .Produces<Product>(201)
    .Produces(401);

products.MapPut("/{id:int}", async (int id, Product product, IProductService service) =>
{
    await service.UpdateAsync(id, product);
    return Results.NoContent();
})
    .RequireAuthorization()
    .Produces(204)
    .Produces(401)
    .Produces(404);

products.MapDelete("/{id:int}", async (int id, IProductService service) =>
{
    await service.DeleteAsync(id);
    return Results.NoContent();
})
    .RequireAuthorization("AdminPolicy")
    .Produces(204)
    .Produces(401)
    .Produces(403);

app.Run();

// Models
public record Product(int Id, string Name, decimal Price);

// Services
public interface IProductService
{
    Task<List<Product>> GetAllAsync();
    Task<Product?> GetByIdAsync(int id);
    Task CreateAsync(Product product);
    Task UpdateAsync(int id, Product product);
    Task DeleteAsync(int id);
}

// Filter
public class LoggingFilter : IEndpointFilter
{
    private readonly ILogger<LoggingFilter> _logger;
    
    public LoggingFilter(ILogger<LoggingFilter> logger)
    {
        _logger = logger;
    }
    
    public async Task<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterInvocationDelegate next)
    {
        _logger.LogInformation(
            "Request: {Method} {Path}",
            context.HttpContext.Request.Method,
            context.HttpContext.Request.Path);
        
        return await next(context);
    }
}
```

---

## Additional Resources

- [Official Microsoft Docs - Minimal APIs](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis)
- [Minimal API Tutorial](https://learn.microsoft.com/en-us/aspnet/core/tutorials/min-web-api)
- [Route Constraints Reference](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/routing#route-constraints)
- [OpenAPI Support for Minimal APIs](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/openapi)
