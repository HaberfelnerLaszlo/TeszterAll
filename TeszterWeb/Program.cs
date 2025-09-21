using Blazored.TextEditor;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;

using Plk.Blazor.DragDrop;

using TeszterLibrary.Services;
using TeszterLibrary.Services.Feladat;

using TeszterWeb.Components;
using TeszterWeb.Data;
using TeszterWeb.Services;

var builder = WebApplication.CreateBuilder(args);

var initialScopes = builder.Configuration["TeszterApi:Scopes"]?.Split(' ') ?? builder.Configuration["MicrosoftGraph:Scopes"]?.Split(' ');

// Add services to the container.
builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
     .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"))
     .EnableTokenAcquisitionToCallDownstreamApi(initialScopes)
.AddMicrosoftGraph(builder.Configuration.GetSection("MicrosoftGraph"))
            .AddInMemoryTokenCaches()
.AddDownstreamApi("DownstreamApi", builder.Configuration.GetSection("TeszterApi"))
                .AddInMemoryTokenCaches();
//builder.Services.AddAuthorization(options =>
//{
//    // By default, all incoming requests will be authorized according to the default policy
//    options.FallbackPolicy = options.DefaultPolicy;
//});
builder.Services.AddDbContext<TeszterLocalDB>(opt => opt.UseInMemoryDatabase("Teszter"));
builder.Services.AddControllersWithViews()
    .AddMicrosoftIdentityUI();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddMicrosoftIdentityConsentHandler();
builder.Services.AddHttpClient();
builder.Services.AddBlazorDragDrop();
builder.Services.AddFluentUIComponents();

builder.Services.AddScoped<TeszterApiService>();
builder.Services.AddScoped<FeladatApiService>();
builder.Services.AddScoped<TanuloService>();
builder.Services.AddScoped<EduUserServices>();
builder.Services.AddScoped<TestServices>();
builder.Services.AddScoped<QuestionServices>();
builder.Services.AddScoped<SubjectServices>();
builder.Services.AddScoped<SectionServices>();
builder.Services.AddScoped<TestUserServices>();
builder.Services.AddScoped<TestQuestionServices>();
builder.Services.AddScoped<TestQuestionUserServices>();
builder.Services.AddScoped<MathService>();
builder.Services.AddScoped<PontServices>();
builder.Services.AddScoped<GroupsService>();
builder.Services.AddScoped<PageHistoryState>();
builder.Services.AddScoped<GraphService>();
builder.Services.AddScoped<GlobalData>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler(new ExceptionHandlerOptions
    {
        ExceptionHandler = async ctx => {
            var feature = ctx.Features.Get<IExceptionHandlerFeature>();
            if (feature?.Error is MsalUiRequiredException
                or { InnerException: MsalUiRequiredException }
                or { InnerException.InnerException: MsalUiRequiredException })
            {
                ctx.Response.Cookies.Delete($"{CookieAuthenticationDefaults.CookiePrefix}{CookieAuthenticationDefaults.AuthenticationScheme}");
                ctx.Response.Redirect(ctx.Request.GetEncodedPathAndQuery());
            }
        }
    });
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();
app.UseStaticFiles();
app.MapControllers();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();