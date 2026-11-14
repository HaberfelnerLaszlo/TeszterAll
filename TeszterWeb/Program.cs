using ApexCharts;

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

using Microsoft.Extensions.DependencyInjection.Extensions;
using TeszterLibrary;
using TeszterLibrary.Extensions;
using TeszterLibrary.Services;
using TeszterLibrary.Services.Feladat;
using TeszterLibrary.Services.Images;

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
// FIGYELEM: NE kapcsold be a FallbackPolicy-t (options.FallbackPolicy = options.DefaultPolicy)!
// Akkor a Blazor-infrastruktúra (_framework/blazor.web.js, _blazor hub) is belépést kérne, és a
// nyilvános Index oldal nem lenne interaktív. A védelem helyette oldalszinten van:
// Components/Pages/_Imports.razor ([Authorize]) + mappánként szerepkör (Teacher/, Student/, Feladat/),
// a nyilvános oldalak pedig [AllowAnonymous]-t kapnak (Index, Error).
builder.Services.AddDbContext<TeszterLocalDB>(opt => opt.UseInMemoryDatabase("Teszter"));
builder.Services.AddControllersWithViews()
    .AddMicrosoftIdentityUI();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddMicrosoftIdentityConsentHandler()
    .AddHubOptions(options => options.MaximumReceiveMessageSize = 4 * 1024 * 1024);   // a szerkesztő a base64 képekkel együtt küldi a HTML-t; a képkorlát: Uploads:MaxImageBytes (2 MB)
builder.Services.AddHttpClient();
builder.Services.AddBlazorDragDrop();
builder.Services.AddFluentUIComponents();
builder.Services.AddApexCharts();
builder.Services.TryAddSingleton(TimeProvider.System);   // az órai nézet idejét tesztben lecserélhetjük
builder.Services.AddImageStorage(builder.Configuration);   // képfeltöltés: ellenőrzés + tárolás (App_Data/uploads)
builder.Services.AddTeszterApiClients();   // TeszterApiService + FeladatApiService + nevesített HttpClient-ek
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
builder.Services.AddScoped<FolderData>();

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
app.MapImageUploads();   // GET /uploads/{fájl} – csak bejelentkezett felhasználónak
app.MapControllers();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();