using Booking.Application.Bookings.Commands.ConfirmBooking;
using Booking.Application.Bookings.Commands.CreateBooking;
using Booking.Infrastructure.Email;
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
using Booking.Application.Users.Commands.LoginUser;
using Booking.Infrastructure.Data;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add DbContext
builder.Services.AddDbContext<BookingPlatformDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        // Allow trailing commas in incoming JSON bodies (lenient parsing)
        opts.JsonSerializerOptions.AllowTrailingCommas = true;
    });

// Also configure minimal API (RequestDelegateFactory) JSON options so body binding
// for MapPost/MapGet uses the same lenient settings.
builder.Services.ConfigureHttpJsonOptions(opts =>
{
    opts.SerializerOptions.AllowTrailingCommas = true;
});

// Add OpenAPI/Swagger
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddScoped<EmailService>();


var app = builder.Build();

app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(async context =>
    {
        var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
        var exception = exceptionHandlerPathFeature?.Error;

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";

        // Krijojmë një objekt me detajet e error-it
        var errorResponse = new
        {
            Message = "An unexpected error occurred.",
            ExceptionMessage = exception?.Message, // Këtu do shohësh "Cannot insert NULL..."
            Detail = exception?.InnerException?.Message, // Këtu sheh detajet e SQL
            StackTrace = exception?.StackTrace // Rruga ku ndodhi gabimi
        };

        await context.Response.WriteAsJsonAsync(errorResponse);
    });
});

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();


// GET /test/users
app.MapGet("/test/users", async (BookingPlatformDbContext db) =>
{
    var users = await db.Users.ToListAsync();
    return Results.Ok(new { count = users.Count, users });
})
.WithTags("Test");


// Endpoint-i i ri
app.MapPost("/test/users", async ([FromBody] UserRegistration req, [FromServices] BookingPlatformDbContext db) =>
{
    // Hash the provided password to match the application's password storage format
    var passwordHash = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(req.Password));

    var newUser = new Booking.Domain.Users.User(
        req.FirstName,
        req.LastName,
        req.Email,
        passwordHash
    );

    // KJO ËSHTË ZGJIDHJA: Vendoset numri që vjen nga Postman
    newUser.UpdatePhoneNumber(req.PhoneNumber);
    newUser.ProfileImageUrl = "default.png"; 
    newUser.isActive = true;
    newUser.CreatedAt = DateTime.UtcNow;

    db.Users.Add(newUser);
    await db.SaveChangesAsync();

    return Results.Created($"/test/users/{newUser.Id}", newUser);
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

app.MapPost("/test/login", async ([FromBody] LoginUserCommand command, [FromServices] BookingPlatformDbContext db) =>
{
    var handler = new LoginUserCommandHandler(db);
    var result = await handler.Handle(command);

    if (!result.IsSuccess)
    {
        return Results.BadRequest(result.Error);
    }

    return Results.Ok(result);
});

app.MapPost("/test/properties", async (CreatePropertyCommand command, BookingPlatformDbContext db) =>
{
    // 1. Validimi
    var validator = new CreatePropertyValidator();
    var validationResult = await validator.ValidateAsync(command);

    if (!validationResult.IsValid)
    {
        return Results.BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
    }

    // 2. Handler (now handles address logic internally)
    var handler = new CreatePropertyCommandHandler(db);
    var result = await handler.Handle(command);

    return result.IsSuccess
        ? Results.Created($"/test/properties/{result.PropertyId}", new
        {
            message = "Property created successfully!",
            propertyId = result.PropertyId
        })
        : Results.BadRequest(new { error = result.Error });
});

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

    // Tani kthejmë të gjithë objektin që sapo u krijua, 
    // EF do ta ketë mbushur fushën .Id automatikisht pas SaveChanges()
    return Results.Ok(new
    {
        Message = "Adresa u krijua me sukses!",
        AddressId = newAddress.Id, // Kjo është ajo që na duhet!
        Details = newAddress
    });
});

app.MapDelete("/test/properties/{id}", async (int id, BookingPlatformDbContext db) =>
{
    var command = new DeletePropertyCommand { PropertyId = id };
    var handler = new DeletePropertyCommandHandler(db);
    var result = await handler.Handle(command);

    return result.IsSuccess
        ? Results.Ok(new { message = "Property deleted successfully" })
        : Results.BadRequest(new { error = result.Error });
});

app.MapPut("/test/properties/{id}", async (int id, UpdatePropertyCommand command, BookingPlatformDbContext db) =>
{
    command.PropertyId = id; // Set from route

    // Validate
    var validator = new UpdatePropertyValidator();
    var validationResult = await validator.ValidateAsync(command);

    if (!validationResult.IsValid)
    {
        return Results.BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
    }

    // Handle
    var handler = new UpdatePropertyCommandHandler(db);
    var result = await handler.Handle(command);

    return result.IsSuccess
        ? Results.Ok(new { message = "Property updated successfully" })
        : Results.BadRequest(new { error = result.Error });
});

// GET /test/properties/{id}
app.MapGet("/test/properties/{id}", async (int id, BookingPlatformDbContext db) =>
{
    var query = new GetPropertyByIdQuery { PropertyId = id };
    var handler = new GetPropertyByIdQueryHandler(db);
    var result = await handler.Handle(query);

    return result.IsSuccess
        ? Results.Ok(result.Property)
        : Results.NotFound(new { error = result.Error });
});

// GET /test/properties
app.MapGet("/test/properties", async (BookingPlatformDbContext db) =>
{
    var query = new GetAllPropertiesQuery();
    var handler = new GetAllPropertiesQueryHandler(db);
    var result = await handler.Handle(query);

    return Results.Ok(new
    {
        count = result.Count,
        properties = result.Properties
    });
});

// GET /test/properties/search
app.MapGet("/test/properties/search", async (
    string? city,
    string? propertyType,
    int? minGuests,
    int? maxGuests,
    bool? isApproved,
    BookingPlatformDbContext db) =>
{
    var query = new SearchPropertiesQuery
    {
        City = city,
        PropertyType = propertyType,
        MinGuests = minGuests,
        MaxGuests = maxGuests,
        IsApproved = isApproved
    };

    var handler = new SearchPropertiesQueryHandler(db);
    var result = await handler.Handle(query);

    return Results.Ok(new
    {
        count = result.Count,
        properties = result.Properties
    });
});

app.MapPost("/test/bookings", async (CreateBookingCommand command, BookingPlatformDbContext db, [FromServices] EmailService emailService) =>
{
    // Validate
    var validator = new CreateBookingValidator();
    var validationResult = await validator.ValidateAsync(command);

    if (!validationResult.IsValid)
    {
        return Results.BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
    }

    // Handle
    var handler = new CreateBookingCommandHandler(db, emailService);
    var result = await handler.Handle(command);

    return result.IsSuccess
        ? Results.Created($"/test/bookings/{result.BookingId}", new
        {
            message = "Booking created successfully",
            bookingId = result.BookingId
        })
        : Results.BadRequest(new { error = result.Error });
});

app.MapPatch("/test/properties/{id}/approve", async (int id, BookingPlatformDbContext db) =>
{
    var command = new ApprovePropertyCommand { PropertyId = id };
    var handler = new ApprovePropertyCommandHandler(db);
    var result = await handler.Handle(command);

    return result.IsSuccess
        ? Results.Ok(new { message = "Property approved successfully" })
        : Results.BadRequest(new { error = result.Error });
});

app.MapPatch("/test/bookings/{id}/confirm", async (int id, BookingPlatformDbContext db) =>
{
    var command = new ConfirmBookingCommand { BookingId = id };
    var handler = new ConfirmBookingCommandHandler(db);
    var result = await handler.Handle(command);

    return result.IsSuccess
        ? Results.Ok(new { message = "Booking confirmed successfully" })
        : Results.BadRequest(new { error = result.Error });
});

app.MapGet("/test/bookings/{id}", async (int id, BookingPlatformDbContext db) =>
{
    var query = new GetBookingByIdQuery { BookingId = id };
    var handler = new GetBookingByIdQueryHandler(db);
    var result = await handler.Handle(query);

    return result.IsSuccess
        ? Results.Ok(result.Booking)
        : Results.NotFound(new { error = result.Error });
});


app.MapGet("/test/bookings", async (BookingPlatformDbContext db) =>
{
    var query = new GetAllBookingsQuery();
    var handler = new GetAllBookingsQueryHandler(db);
    var result = await handler.Handle(query);

    return Results.Ok(new
    {
        count = result.Count,
        bookings = result.Bookings
    });
});

// CREATE Review
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

// UPDATE Review
app.MapPut("/test/reviews/{id}", async (int id, UpdateReviewCommand command, BookingPlatformDbContext db) =>
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

// DELETE Review
app.MapDelete("/test/reviews/{id}", async (int id, BookingPlatformDbContext db) =>
{
    var command = new DeleteReviewCommand { ReviewId = id };
    var handler = new DeleteReviewCommandHandler(db);
    var result = await handler.Handle(command);

    return result.IsSuccess
        ? Results.Ok(new { message = "Review deleted successfully" })
        : Results.BadRequest(new { error = result.Error });
});

// GET Review by ID
app.MapGet("/test/reviews/{id}", async (int id, BookingPlatformDbContext db) =>
{
    var query = new GetReviewByIdQuery { ReviewId = id };
    var handler = new GetReviewByIdQueryHandler(db);
    var result = await handler.Handle(query);

    return result.IsSuccess
        ? Results.Ok(result.Review)
        : Results.NotFound(new { error = result.Error });
});

// GET Reviews by Property
app.MapGet("/test/properties/{propertyId}/reviews", async (int propertyId, BookingPlatformDbContext db) =>
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

app.Run();

public record UserRegistration(
    [property: JsonPropertyName("firstName")] string FirstName,
    [property: JsonPropertyName("lastName")] string LastName,
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("password")] string Password,
    [property: JsonPropertyName("phoneNumber")] string PhoneNumber
);
public record AddressRequest(string Country, string City, string Street, string PostalCode);