using System.Text.Json;

using Microsoft.Graph;
using Microsoft.JSInterop;

namespace TeszterTeams.Interop.TeamsSDK;

public class MicrosoftTeams : InteropModuleBase
{
    protected override string ModulePath => "./js/TeamsJsBlazorInterop.js";

    public MicrosoftTeams(IJSRuntime jsRuntime) : base(jsRuntime) { }

    public Task InitializeAsync()
    {
        return InvokeVoidAsync("initializeAsync");
    }

    public Task<TeamsContext> GetTeamsContextAsync()
    {
        return InvokeAsync<TeamsContext>("getContextAsync");
    }
    public Task<Context.Root> GetTeams_2_ContextAsync()   {
        //var context = await;
        //var root=JsonSerializer.Deserialize<>(context.ToString());
        return  InvokeAsync<Context.Root>("getContextAsync");
    }

    public Task RegisterOnSaveHandlerAsync(TeamsInstanceSettings settings)
    {
        return InvokeVoidAsync("registerOnSaveHandler", settings);
    }

    public Task<bool> IsInTeams()
    {
        try
        {
            return InvokeAsync<bool>("inTeams");
        }
        catch (JSException)
        {
            return Task.FromResult(false);
        }
    }
}
///ValueKind = Object : "{"app":{"locale":"hu-hu","sessionId":"70010fe2-a8c2-4e88-8ca4-4d48cf1d5952","theme":"default","iconPositionVertical":79,"parentMessageId":"","userClickTime":1675016179283,"userFileOpenPreference":"inline","host":{"name":"Teams","clientType":"web","sessionId":"c6c3276b-1a67-a6c8-dec4-ef83806e3903","ringId":"general"},"appLaunchId":"f3d69422-1c76-4512-8f4a-6e1b46a81b1d"},"page":{"id":"8582F13E-8D4A - 4AF8 - 9FA9 - 7DD4D0D993E0","frameContext":"content","subPageId":"","isFullScreen":false,"isMultiWindow":false,"sourceOrigin":null},"user":{"id":"54885fe6-e6cc-4d9e-aed1-0edf7fd6d036","licenseType":"Teacher","loginHint":"haberfelner @peterfysuli.hu","userPrincipalName":"haberfelner @peterfysuli.hu","tenant":{"id":"e635a9ff-0111-4671-b02d-1a544fc50ecf","teamsSku":"edu"}},"channel":{"id":"19:29c76fb02f394c8f9f238630a73a7d1c @thread.tacv2","displayName":"Általános","relativeUrl":"/sites/Tesztertesztcsoport/Megosztott dokumentumok/General","membershipType":"Regular","defaultOneNoteSectionId":""},"team":{"internalId":"19:29c76fb02f394c8f9f238630a73a7d1c @thread.tacv2","displayName":"Teszter teszt csoport","type":2,"groupId":"57da251f-9472-46f2-b8ac-3b6aaeaece1c","templateId":"","isArchived":false,"userRole":0},"sharePointSite":{"teamSiteUrl":"https://miiskola.sharepoint.com/sites/Tesztertesztcsoport","teamSiteDomain":"miiskola.sharepoint.com","teamSitePath":"/sites/Tesztertesztcsoport","teamSiteId":"","mySitePath":"/personal/haberfelner_peterfysuli_hu","mySiteDomain":"miiskola-my.sharepoint.com"}}"