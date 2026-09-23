var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAuthorization();
builder.Services.AddSingleton<RecallOperations.Api.Services.UserStore>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:5048")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Recall Operations API V1");
    c.RoutePrefix = "swagger";
});

app.UseCors("FrontendPolicy");
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseAuthorization();

app.MapGet("/", () => Results.Ok(new
{
    service = "Recall Operations API",
    status = "running",
    endpoints = new[]
    {
        "/api/vehicles",
        "/api/vehicles/{vin}",
        "/api/users",
        "/api/users/login",
        "/swagger"
    }
}));

app.MapControllers();

app.Run();
