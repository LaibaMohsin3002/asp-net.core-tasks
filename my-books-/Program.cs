using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using my_books.Data;
using my_books.Data.Services;
using my_books.Exceptions;
using Serilog;

try
{
    var configuration = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .Build();

    Log.Logger = new LoggerConfiguration()
        .ReadFrom.Configuration(configuration)
        .CreateLogger();

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog();

    // Add services to the container.
    builder.Services.AddControllers();
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
    builder.Services.AddScoped<BooksService>();
    builder.Services.AddScoped<AuthorsService>();
    builder.Services.AddScoped<PublishersService>();
    builder.Services.AddApiVersioning(options =>
    {
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.ReportApiVersions = true;
        //options.ApiVersionReader = new HeaderApiVersionReader("custom-version-header");
        options.ApiVersionReader = new MediaTypeApiVersionReader();
    });

    builder.Services.AddScoped<LogsService>();

    var app = builder.Build();
    AppDbInitializer.Seed(app);

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.ConfigureBuildInExceptionHandler(app.Services.GetRequiredService<ILoggerFactory>());    //app.ConfigureCustomExceptionHandler();

    app.UseHttpsRedirection();
    app.UseAuthorization();
    app.MapControllers();

    app.Run();
}
finally
{
    Log.CloseAndFlush();
}