using Microsoft.FluentUI.AspNetCore.Components;

using TeszterLibrary.Services;

using TeszterTeams;
using TeszterTeams.Components;
using TeszterTeams.Interop.TeamsSDK;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var config = builder.Configuration.Get<ConfigOptions>();
builder.Services.AddTeamsFx(config.TeamsFx.Authentication);
builder.Services.AddScoped<MicrosoftTeams>();
//builder.Services.AddScoped<GraphService>();
builder.Services.AddSingleton<MathService>();
builder.Services.AddSingleton<TeszterApiService>();
builder.Services.AddSingleton<QuestionServices>();
builder.Services.AddSingleton<SubjectServices>();
builder.Services.AddSingleton<TestServices>();
builder.Services.AddSingleton<TestQuestionServices>();
builder.Services.AddFluentUIComponents();
builder.Services.AddControllers();
builder.Services.AddHttpClient("WebClient", client => client.Timeout = TimeSpan.FromSeconds(600));
builder.Services.AddHttpContextAccessor();
builder.Services.AddAntiforgery(o => o.SuppressXFrameOptionsHeader = true);

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
app.UseAntiforgery();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();