using Booking.Application.Bookings.Commands.CancelBooking;
using Booking.Application.Bookings.Commands.ConfirmBooking;
using Booking.Application.Bookings.Commands.CreateBooking;
using Booking.Application.Bookings.Queries.GetAllBookings;
using Booking.Application.Bookings.Queries.GetBookingById;
using Booking.Application.Properties.Commands.ApproveProperty;
using Booking.Application.Properties.Commands.CreateProperty;
using Booking.Application.Properties.Commands.DeleteProperty;
using Booking.Application.Properties.Commands.UpdateProperty;
using Booking.Application.Properties.Queries.GetAllProperties;
using Booking.Application.Properties.Queries.GetPropertyById;
using Booking.Application.Properties.Queries.SearchProperties;
using Booking.Application.Reviews.Commands.CreateReview;
using Booking.Application.Reviews.Commands.DeleteReview;
using Booking.Application.Reviews.Commands.UpdateReview;
using Booking.Application.Reviews.Queries.GetReviewById;
using Booking.Application.Reviews.Queries.GetReviewsByProperty;
using Booking.Application.Users.Commands.ChangePassword;
using Booking.Application.Users.Commands.LoginUser;
using Booking.Application.Users.Commands.Logout;
using Booking.Application.Users.Commands.RefreshToken;
using Booking.Application.Users.Commands.UpdateUserProfile;
using Booking.Application.Users.Queries.GetUser;
using Booking.Infrastructure.Authentication;
using Booking.Infrastructure.Data;
using Booking.Infrastructure.Email;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Add DbContext
builder.Services.AddDbContext<BookingPlatformDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.AllowTrailingCommas = true;
    });

builder.Services.ConfigureHttpJsonOptions(opts =>
{
    opts.SerializerOptions.AllowTrailingCommas = true;
});

// Add OpenAPI/Swagger
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();

// Register services
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<JwtService>();

// Configure JWT Authentication
var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET") ?? builder.Configuration["Jwt:Secret"];
var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? builder.Configuration["Jwt:Issuer"];
var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? builder.Configuration["Jwt:Audience"];


builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        ,
        // Ensure the role claim from the token is recognized by ASP.NET authorization
        RoleClaimType = ClaimTypes.Role,
        NameClaimType = ClaimTypes.NameIdentifier
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(async context =>
    {
        var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
        var exception = exceptionHandlerPathFeature?.Error;

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";

        var errorResponse = new
        {
            Message = "An unexpected error occurred.",
            ExceptionMessage = exception?.Message,
            Detail = exception?.InnerException?.Message,
            StackTrace = exception?.StackTrace
        };

        await context.Response.WriteAsJsonAsync(errorResponse);
    });
});

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// GET /test/users
app.MapGet("/test/users", async (BookingPlatformDbContext db) =>
{
    var users = await db.Users.ToListAsync();
    return Results.Ok(new { count = users.Count, users });
})
.WithTags("Test");

// GET /test/users/{id}
app.MapGet("/test/users/{id}", async (
    Guid id,
    BookingPlatformDbContext db,
    HttpContext httpContext) =>
{
    // Extract current user ID from token
    var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
    if (userIdClaim == null)
    {
        return Results.Unauthorized();
    }

    var currentUserId = Guid.Parse(userIdClaim.Value);

    // Check if user is Admin
    var isAdmin = httpContext.User.IsInRole("Admin");

    // ✅ Authorization: Can view only SELF or if ADMIN
    if (currentUserId != id && !isAdmin)
    {
        return Results.Json(
            new { error = "Forbidden: You can only view your own profile" },
            statusCode: 403
        );
    }

    // Proceed with query
    var query = new GetUserQuery { UserId = id };
    var handler = new GetUserQueryHandler(db);
    var result = await handler.Handle(query);

    return result.IsSuccess
        ? Results.Ok(result.User)
        : Results.NotFound(new { error = result.Error });
})
.RequireAuthorization();

// POST /test/users

app.MapPost("/test/users", async ([FromBody] UserRegistration req, [FromServices] BookingPlatformDbContext db) =>
{
    // Validate roleType
    var validRoles = new[] { "Guest", "Host", "Administrator" };
    if (!validRoles.Contains(req.RoleType))
    {
        return Results.BadRequest(new { error = "Invalid role type. Must be: Guest, Host, or Administrator" });
    }

    // Check email uniqueness
    var emailExists = await db.Users.AnyAsync(u => u.Email == req.Email);
    if (emailExists)
    {
        return Results.BadRequest(new { error = "Email already exists" });
    }

    // Hash password
    var passwordHash = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(req.Password));

    // Create user
    var newUser = new Booking.Domain.Users.User(
        req.FirstName,
        req.LastName,
        req.Email,
        passwordHash
    );

    newUser.UpdatePhoneNumber(req.PhoneNumber);
    newUser.ProfileImageUrl = "default.png";
    newUser.isActive = true;
    newUser.CreatedAt = DateTime.UtcNow;

    db.Users.Add(newUser);
    await db.SaveChangesAsync();

    // ✅ MAP USER ROLES TO DB ROLES
    string dbRoleName = req.RoleType == "Host" ? "Owner" :
                        req.RoleType == "Administrator" ? "Admin" :
                        req.RoleType;

    var role = await db.Roles.FirstOrDefaultAsync(r => r.Name == dbRoleName);
    if (role != null)
    {
        var userRole = new Booking.Domain.UserRoles.UserRole(newUser.Id, role.Id);
        db.UserRoles.Add(userRole);
        await db.SaveChangesAsync();
    }

    return Results.Created($"/test/users/{newUser.Id}", new
    {
        userId = newUser.Id,
        email = newUser.Email,
        role = req.RoleType,
        message = $"User registered successfully as {req.RoleType}"
    });
});

// GET /test/roles
app.MapGet("/test/roles", async (BookingPlatformDbContext db) =>
{
    var roles = await db.Roles.ToListAsync();
    return Results.Ok(roles);
})
.WithTags("Test");

// GET /test/db-connection
app.MapGet("/test/db-connection", async (BookingPlatformDbContext db) =>
{
    try
    {
        var canConnect = await db.Database.CanConnectAsync();
        return Results.Ok(new { connected = canConnect, message = "Database connection successful!" });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Database connection failed: {ex.Message}");
    }
})
.WithTags("Test");

// POST /test/login
app.MapPost("/test/login", async (
    LoginUserCommand command,
    BookingPlatformDbContext db,
    JwtService jwtService) =>
{
    var handler = new LoginUserCommandHandler(db, jwtService);
    var result = await handler.Handle(command);

    if (!result.IsSuccess)
    {
        return Results.BadRequest(new { error = result.Error });
    }

    return Results.Ok(new
    {
        token = result.Token,
        refreshToken = result.RefreshToken,
        userId = result.UserId,
        email = result.Email,
        roles = result.Roles
    });
});

// POST /test/properties
app.MapPost("/test/properties", async (CreatePropertyCommand command, BookingPlatformDbContext db) =>
{
    var validator = new CreatePropertyValidator();
    var validationResult = await validator.ValidateAsync(command);

    if (!validationResult.IsValid)
    {
        return Results.BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
    }

    var handler = new CreatePropertyCommandHandler(db);
    var result = await handler.Handle(command);

    return result.IsSuccess
        ? Results.Created($"/test/properties/{result.PropertyId}", new
        {
            message = "Property created successfully!",
            propertyId = result.PropertyId
        })
        : Results.BadRequest(new { error = result.Error });
})
.RequireAuthorization();

// POST /test/addresses
app.MapPost("/test/addresses", async ([FromBody] AddressRequest req, BookingPlatformDbContext db) =>
{
    var newAddress = new Booking.Domain.Addresses.Address(
        req.Country,
        req.City,
        req.Street,
        req.PostalCode
    );

    db.Addresses.Add(newAddress);
    await db.SaveChangesAsync();

    return Results.Ok(new
    {
        Message = "Adresa u krijua me sukses!",
        AddressId = newAddress.Id,
        Details = newAddress
    });
});

// DELETE /test/properties/{id}
app.MapDelete("/test/properties/{id}", async (Guid id, BookingPlatformDbContext db) =>
{
    var command = new DeletePropertyCommand { PropertyId = id };
    var handler = new DeletePropertyCommandHandler(db);
    var result = await handler.Handle(command);

    return result.IsSuccess
        ? Results.Ok(new { message = "Property deleted successfully" })
        : Results.BadRequest(new { error = result.Error });
});


// PUT /test/properties/{id}
app.MapPut("/test/properties/{id}", async (Guid id, UpdatePropertyCommand command, BookingPlatformDbContext db) =>
{
    command.PropertyId = id;

    var validator = new UpdatePropertyValidator();
    var validationResult = await validator.ValidateAsync(command);

    if (!validationResult.IsValid)
    {
        return Results.BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
    }

    var handler = new UpdatePropertyCommandHandler(db);
    var result = await handler.Handle(command);

    return result.IsSuccess
        ? Results.Ok(new { message = "Property updated successfully" })
        : Results.BadRequest(new { error = result.Error });
})
.RequireAuthorization();

// GET /test/properties/{id}
app.MapGet("/test/properties/{id}", async (Guid id, BookingPlatformDbContext db) =>
{
    var query = new GetPropertyByIdQuery { PropertyId = id };
    var handler = new GetPropertyByIdQueryHandler(db);
    var result = await handler.Handle(query);

    return result.IsSuccess
        ? Results.Ok(result.Property)
        : Results.NotFound(new { error = result.Error });
})
.RequireAuthorization();

// GET /test/properties
app.MapGet("/test/properties", async (BookingPlatformDbContext db) =>
{
    var query = new GetAllPropertiesQuery();
    var handler = new GetAllPropertiesQueryHandler(db);
    var result = await handler.Handle(query);

    return Results.Ok(result);
});

// GET /test/properties/search
app.MapGet("/test/properties/search", async (
    string? city,
    string? propertyType,
    int? minGuests,
    int? maxGuests,
    bool? isApproved,
    int? page,              
    int? pageSize,        
    BookingPlatformDbContext db) =>
{
    var query = new SearchPropertiesQuery
    {
        City = city,
        PropertyType = propertyType,
        MinGuests = minGuests,
        MaxGuests = maxGuests,
        IsApproved = isApproved,
        Page = page ?? 1,          
        PageSize = pageSize ?? 10  
    };

    var handler = new SearchPropertiesQueryHandler(db);
    var result = await handler.Handle(query);

    return Results.Ok(result);
});

// PATCH /test/properties/{id}/approve
app.MapPatch("/test/properties/{id}/approve", async (Guid id, BookingPlatformDbContext db) =>
{
    var command = new ApprovePropertyCommand { PropertyId = id };
    var handler = new ApprovePropertyCommandHandler(db);
    var result = await handler.Handle(command);

    return result.IsSuccess
        ? Results.Ok(new { message = "Property approved successfully" })
        : Results.BadRequest(new { error = result.Error });
})
.RequireAuthorization(policy => policy.RequireRole("Admin", "Administrator"));


// POST /test/bookings
app.MapPost("/test/bookings", async (CreateBookingCommand command, BookingPlatformDbContext db, [FromServices] EmailService emailService) =>
{
    var validator = new CreateBookingValidator();
    var validationResult = await validator.ValidateAsync(command);

    if (!validationResult.IsValid)
    {
        return Results.BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
    }

    var handler = new CreateBookingCommandHandler(db, emailService);
    var result = await handler.Handle(command);

    return result.IsSuccess
        ? Results.Created($"/test/bookings/{result.BookingId}", new
        {
            message = "Booking created successfully",
            bookingId = result.BookingId
        })
        : Results.BadRequest(new { error = result.Error });
})
.RequireAuthorization();

// GET /test/bookings/{id}
app.MapGet("/test/bookings/{id}", async (Guid id, BookingPlatformDbContext db) =>
{
    var query = new GetBookingByIdQuery { BookingId = id };
    var handler = new GetBookingByIdQueryHandler(db);
    var result = await handler.Handle(query);

    return result.IsSuccess
        ? Results.Ok(result.Booking)
        : Results.NotFound(new { error = result.Error });
});

// GET /test/bookings
app.MapGet("/test/bookings", async (
    int? page,
    int? pageSize,
    BookingPlatformDbContext db) =>
{
    var query = new GetAllBookingsQuery
    {
        Page = page ?? 1,
        PageSize = pageSize ?? 10
    };

    var handler = new GetAllBookingsQueryHandler(db);
    var result = await handler.Handle(query);

    return Results.Ok(result);
});

// PATCH /test/bookings/{id}/confirm
app.MapPatch("/test/bookings/{id}/confirm", async (
    Guid id,
    BookingPlatformDbContext db,
    HttpContext httpContext) =>  
{
    
    var userIdClaim = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
    if (userIdClaim == null)
    {
        return Results.Unauthorized();
    }

    var userId = Guid.Parse(userIdClaim.Value);

    var command = new ConfirmBookingCommand
    {
        BookingId = id,
        RequestingUserId = userId 
    };

    var handler = new ConfirmBookingCommandHandler(db);
    var result = await handler.Handle(command);

    return result.IsSuccess
        ? Results.Ok(new { message = "Booking confirmed successfully" })
        : Results.BadRequest(new { error = result.Error });
})
.RequireAuthorization();

app.MapPatch("/test/bookings/{id}/cancel", async (
    Guid id,
    BookingPlatformDbContext db,
    HttpContext httpContext) =>
{
    // Extract UserId from JWT token
    var userIdClaim = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
    if (userIdClaim == null)
    {
        return Results.Unauthorized();
    }

    var userId = Guid.Parse(userIdClaim.Value);

    var command = new CancelBookingCommand
    {
        BookingId = id,
        RequestingUserId = userId
    };

    var handler = new CancelBookingCommandHandler(db);
    var result = await handler.Handle(command);

    return result.IsSuccess
        ? Results.Ok(new { message = "Booking cancelled successfully" })
        : Results.BadRequest(new { error = result.Error });
})
.RequireAuthorization();

// POST /test/reviews
app.MapPost("/test/reviews", async (CreateReviewCommand command, BookingPlatformDbContext db) =>
{
    var validator = new CreateReviewValidator();
    var validationResult = await validator.ValidateAsync(command);

    if (!validationResult.IsValid)
    {
        return Results.BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
    }

    var handler = new CreateReviewCommandHandler(db);
    var result = await handler.Handle(command);

    return result.IsSuccess
        ? Results.Created($"/test/reviews/{result.ReviewId}", new
        {
            message = "Review created successfully",
            reviewId = result.ReviewId
        })
        : Results.BadRequest(new { error = result.Error });
});

// PUT /test/reviews/{id}
app.MapPut("/test/reviews/{id}", async (Guid id, UpdateReviewCommand command, BookingPlatformDbContext db) =>
{
    command.ReviewId = id;

    var validator = new UpdateReviewValidator();
    var validationResult = await validator.ValidateAsync(command);

    if (!validationResult.IsValid)
    {
        return Results.BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
    }

    var handler = new UpdateReviewCommandHandler(db);
    var result = await handler.Handle(command);

    return result.IsSuccess
        ? Results.Ok(new { message = "Review updated successfully" })
        : Results.BadRequest(new { error = result.Error });
});

// DELETE /test/reviews/{id}
app.MapDelete("/test/reviews/{id}", async (Guid id, BookingPlatformDbContext db) =>
{
    var command = new DeleteReviewCommand { ReviewId = id };
    var handler = new DeleteReviewCommandHandler(db);
    var result = await handler.Handle(command);

    return result.IsSuccess
        ? Results.Ok(new { message = "Review deleted successfully" })
        : Results.BadRequest(new { error = result.Error });
});

// GET /test/reviews/{id}
app.MapGet("/test/reviews/{id}", async (Guid id, BookingPlatformDbContext db) =>
{
    var query = new GetReviewByIdQuery { ReviewId = id };
    var handler = new GetReviewByIdQueryHandler(db);
    var result = await handler.Handle(query);

    return result.IsSuccess
        ? Results.Ok(result.Review)
        : Results.NotFound(new { error = result.Error });
});

// GET /test/properties/{propertyId}/reviews
app.MapGet("/test/properties/{propertyId}/reviews", async (Guid propertyId, BookingPlatformDbContext db) =>
{
    var query = new GetReviewsByPropertyQuery { PropertyId = propertyId };
    var handler = new GetReviewsByPropertyQueryHandler(db);
    var result = await handler.Handle(query);

    return Results.Ok(new
    {
        propertyId = propertyId,
        count = result.Count,
        averageRating = result.AverageRating,
        reviews = result.Reviews
    });
});

// DEBUG: return current user's claims and roles
app.MapGet("/test/me", (ClaimsPrincipal user) =>
{
    var roles = user.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();
    var claims = user.Claims.Select(c => new { c.Type, c.Value }).ToList();

    return Results.Ok(new
    {
        IsAuthenticated = user.Identity?.IsAuthenticated ?? false,
        AuthenticationType = user.Identity?.AuthenticationType,
        Name = user.Identity?.Name,
        NameIdentifier = user.FindFirst(ClaimTypes.NameIdentifier)?.Value,
        Email = user.FindFirst(ClaimTypes.Email)?.Value,
        Roles = roles,
        Claims = claims
    });
}).RequireAuthorization();

// POST /test/refresh-token
app.MapPost("/test/refresh-token", async (
    [FromBody] RefreshTokenCommand command,
    BookingPlatformDbContext db,
    JwtService jwtService) =>
{
    var handler = new RefreshTokenCommandHandler(db, jwtService);
    var result = await handler.Handle(command);

    if (!result.IsSuccess)
    {
        return Results.BadRequest(new { error = result.Error });
    }

    return Results.Ok(new
    {
        accessToken = result.AccessToken,
        refreshToken = result.RefreshToken
    });
});

// POST /test/logout
app.MapPost("/test/logout", async (
    [FromBody] LogoutCommand command,
    BookingPlatformDbContext db) =>
{
    var handler = new LogoutCommandHandler(db);
    var result = await handler.Handle(command);

    if (!result.IsSuccess)
    {
        return Results.BadRequest(new { error = result.Error });
    }

    return Results.Ok(new { message = "Logged out successfully" });
})
.RequireAuthorization();

// PUT /test/users/profile
app.MapPut("/test/users/profile", async (
    [FromBody] UpdateUserProfileCommand command,
    BookingPlatformDbContext db,
    HttpContext httpContext) =>
{
    // Extract userId from token
    var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
    if (userIdClaim == null)
    {
        return Results.Unauthorized();
    }

    var currentUserId = Guid.Parse(userIdClaim.Value);

    // User can only update their own profile
    command.UserId = currentUserId;

    var validator = new UpdateUserProfileValidator();
    var validationResult = await validator.ValidateAsync(command);

    if (!validationResult.IsValid)
    {
        return Results.BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
    }

    var handler = new UpdateUserProfileCommandHandler(db);
    var result = await handler.Handle(command);

    return result.IsSuccess
        ? Results.Ok(new { message = "Profile updated successfully" })
        : Results.BadRequest(new { error = result.Error });
})
.RequireAuthorization();

// POST /test/users/change-password
app.MapPost("/test/users/change-password", async (
    [FromBody] ChangePasswordCommand command,
    BookingPlatformDbContext db,
    HttpContext httpContext) =>
{
    // Extract userId from token
    var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
    if (userIdClaim == null)
    {
        return Results.Unauthorized();
    }

    var currentUserId = Guid.Parse(userIdClaim.Value);

    // User can only change their own password
    command.UserId = currentUserId;

    var validator = new ChangePasswordValidator();
    var validationResult = await validator.ValidateAsync(command);

    if (!validationResult.IsValid)
    {
        return Results.BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
    }

    var handler = new ChangePasswordCommandHandler(db);
    var result = await handler.Handle(command);

    return result.IsSuccess
        ? Results.Ok(new { message = "Password changed successfully" })
        : Results.BadRequest(new { error = result.Error });
})
.RequireAuthorization();

app.Run();

public record UserRegistration(
    [property: JsonPropertyName("firstName")] string FirstName,
    [property: JsonPropertyName("lastName")] string LastName,
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("password")] string Password,
    [property: JsonPropertyName("phoneNumber")] string PhoneNumber,
    [property: JsonPropertyName("roleType")] string RoleType
);

public record AddressRequest(string Country, string City, string Street, string PostalCode);