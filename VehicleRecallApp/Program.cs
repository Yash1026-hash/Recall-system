using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/Login";
            options.Cookie.Name = "VehicleRecall.Auth";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ManagerOnly", policy => policy.RequireRole("Manager"));
    options.AddPolicy("CustomerOnly", policy => policy.RequireRole("Customer"));
    options.AddPolicy("TechnicianOnly", policy => policy.RequireRole("Technician"));
});

builder.Services.AddHttpClient("ApiClient", client =>
{
    client.BaseAddress = new Uri("http://localhost:5070");
    client.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json"));
}).AddHttpMessageHandler<VehicleRecallApp.Services.BearerTokenForwardingHandler>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<VehicleRecallApp.Services.BearerTokenForwardingHandler>();

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AllowAnonymousToPage("/Index");
    options.Conventions.AllowAnonymousToPage("/Privacy");
    options.Conventions.AllowAnonymousToPage("/Account/Login");
    options.Conventions.AuthorizeFolder("/Manager", "ManagerOnly");
    options.Conventions.AuthorizeFolder("/Customer", "CustomerOnly");
    options.Conventions.AuthorizeFolder("/Technician", "TechnicianOnly");
});
builder.Services.AddSingleton<VehicleRecallApp.Services.InMemoryCampaignStore>();
builder.Services.AddSingleton<VehicleRecallApp.Services.CampaignCsvImporter>();
builder.Services.AddTransient<VehicleRecallApp.Services.CustomerRosterSyncService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();
app.Run();
