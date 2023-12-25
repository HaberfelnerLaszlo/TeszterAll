using TeszterLibrary.Services;
using Microsoft.Identity.Web;
using Plk.Blazor.DragDrop;
using TeszterTeams;
using TeszterTeams.Interop.TeamsSDK;
using Microsoft.FluentUI.AspNetCore.Components;

var builder = WebApplication.CreateBuilder(args);
builder.Services
    // Use Web API authentication (default JWT bearer token scheme)
    .AddMicrosoftIdentityWebApiAuthentication(builder.Configuration, "TeamsFx:Authentication")
    // Enable token acquisition via on-behalf-of flow
    .EnableTokenAcquisitionToCallDownstreamApi()
    // Add authenticated Graph client via dependency injection
    .AddMicrosoftGraph(builder.Configuration.GetSection("MicrosoftGraph"))
    // Use in-memory token cache
    // See https://github.com/AzureAD/microsoft-identity-web/wiki/token-cache-serialization
    .AddInMemoryTokenCaches();
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddBlazorDragDrop();
var config = builder.Configuration.Get<ConfigOptions>();
builder.Services.AddTeamsFx(config.TeamsFx.Authentication);
builder.Services.AddScoped<MicrosoftTeams>();
builder.Services.AddScoped<GraphService>();
builder.Services.AddSingleton<MathService>();
builder.Services.AddSingleton<TeszterApiService>();
builder.Services.AddSingleton<QuestionServices>();
builder.Services.AddSingleton<SubjectServices>();
builder.Services.AddSingleton<TestServices>();
builder.Services.AddControllers();
builder.Services.AddHttpClient("WebClient", client => client.Timeout = TimeSpan.FromSeconds(600));
builder.Services.AddFluentUIComponents();
builder.Services.AddHttpContextAccessor();
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");
app.MapControllers();
//app.UseEndpoints(endpoints =>
//{
//    endpoints.MapBlazorHub();
//    endpoints.
//    endpoints.MapControllers();
//});

app.Run();