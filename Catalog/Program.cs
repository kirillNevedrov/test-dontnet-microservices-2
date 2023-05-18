var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<CatalogContext>(options =>
            {
                options.UseSqlServer(builder.Configuration["ConnectionString"],
                                        sqlServerOptionsAction: sqlOptions =>
                                        {
                                            sqlOptions.MigrationsAssembly(typeof(Program).GetTypeInfo().Assembly.GetName().Name);
                                            //Configuring Connection Resiliency: https://docs.microsoft.com/en-us/ef/core/miscellaneous/connection-resiliency 
                                            sqlOptions.EnableRetryOnFailure(maxRetryCount: 15, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
                                        });
            });

builder.Services.Configure<CatalogSettings>(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

try
{
    // Log.Information("Configuring web host ({ApplicationContext})...", Program.AppName);
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<CatalogContext>();
    var env = app.Services.GetService<IWebHostEnvironment>();
    var settings = app.Services.GetService<IOptions<CatalogSettings>>();
    var logger = app.Services.GetService<ILogger<CatalogContextSeed>>();
    await context.Database.MigrateAsync();

    await new CatalogContextSeed().SeedAsync(context, env, settings, logger);
    // var integEventContext = scope.ServiceProvider.GetRequiredService<IntegrationEventLogContext>();
    // await integEventContext.Database.MigrateAsync();
    // app.Logger.LogInformation("Starting web host ({ApplicationName})...", AppName);
    await app.RunAsync();

    return 0;
}
catch (Exception ex)
{
    // Log.Fatal(ex, "Program terminated unexpectedly ({ApplicationContext})!", Program.AppName);
    return 1;
}
finally
{
    // Log.CloseAndFlush();
}

// app.Run();
