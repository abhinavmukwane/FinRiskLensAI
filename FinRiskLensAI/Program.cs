using Autofac;
using Autofac.Extensions.DependencyInjection;
using FinRiskLensAI.Core.DI;
using FinRiskLensAI.Data.DI;
using FinRiskLensAI.ML.DI;
using FinRiskLensAI.Services.DI;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

//Serilog
builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext());

//Autofac
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
builder.Host.ConfigureContainer<ContainerBuilder>(container =>
{
    container.RegisterModule(new CoreModule());
    container.RegisterModule(new DataModule());
    container.RegisterModule(new ServicesModule());
    container.RegisterModule(new MLModule());

    // Settings POCOs bound from configuration (EmailService and friends take these directly)
    container.RegisterInstance(
        builder.Configuration.GetSection("Smtp").Get<FinRiskLensAI.Core.Models.Common.SmtpSettings>()
        ?? new FinRiskLensAI.Core.Models.Common.SmtpSettings());
    container.RegisterInstance(
        builder.Configuration.GetSection("Groq").Get<FinRiskLensAI.Core.Models.Common.GroqSettings>()
        ?? new FinRiskLensAI.Core.Models.Common.GroqSettings());
    container.RegisterInstance(
        builder.Configuration.GetSection("Finvu").Get<FinRiskLensAI.Core.Models.Common.FinvuSettings>()
        ?? new FinRiskLensAI.Core.Models.Common.FinvuSettings());
});

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(
        new System.Text.Json.Serialization.JsonStringEnumConverter()));
builder.Services.AddAppDbContext(builder.Configuration);

builder.Services.AddSession(options =>
{
    options.Cookie.Name = "frlai.sid";
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;                          // no JS access
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // HTTPS only
    options.Cookie.SameSite = SameSiteMode.Strict;           // no cross-site send
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
