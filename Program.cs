using BlazorApp1;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/access-denied";
        options.Events.OnRedirectToLogin = context =>
        {
            if (context.Request.Path != "/login" && context.Request.Path != "/access-denied")
            {
                context.Response.Redirect(context.RedirectUri);
            }
            return Task.CompletedTask;
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("OperatorOnly", policy => policy.RequireRole("Operator"));
    options.AddPolicy("ReaderOnly", policy => policy.RequireRole("Reader"));
});

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AllowAnonymousToPage("/login");
    options.Conventions.AllowAnonymousToPage("/access-denied");
});
builder.Services.AddServerSideBlazor();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
builder.Services.AddScoped<ProtectedSessionStorage>();
builder.Services.AddMudServices();
builder.Services.AddSingleton<IWebHostEnvironment>(builder.Environment);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();