using MySql.Data.MySqlClient;
using reactpersonaddress.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
        builder.SetIsOriginAllowed(_ => true)
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials());
});

// Add a singleton service for the MySQL connection string
builder.Services.AddSingleton<MySqlConnection>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("DefaultConnection");
    return new MySqlConnection(connectionString);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Optional: Enable Swagger in Development mode for API documentation
    // Uncomment if you want to use Swagger
    // app.UseSwagger();
    // app.UseSwaggerUI();
}
app.UseMiddleware<ExceptionMiddleware>();
app.UseCors();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
