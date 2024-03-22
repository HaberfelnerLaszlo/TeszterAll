using Azure.Core;
using Azure.Identity;

using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.TeamsFx;

using TeszterLibrary.Model;

using TeszterTeams.Components.Pages;

namespace TeszterTeams
{
    public class GraphServices
    {
        public bool NeedConsent { get; set; }
        public User Profile { get; set; }
        public string UserPhotoUri { get; set; }
        public string ErrorMessage = string.Empty;

        private readonly string _scope = "User.Read EduRoster.ReadBasic";
        private GraphServiceClient _client;
        private readonly TeamsUserCredential teamsUserCredential;
        private readonly IConfiguration Configuration;

        public GraphServices(IConfiguration configuration, TeamsUserCredential teamsUserCredential)
        {
            Configuration = configuration;
            this.teamsUserCredential = teamsUserCredential;
            if (HasPermission(_scope).Result)
            {
                ShowProfile().GetAwaiter();
            }

        }
        private async Task ShowProfile()
        {
            var graph = await GetGraphServiceClient();

            Profile = await graph.Me.GetAsync();
        }

        private async Task<bool> HasPermission(string scope)
        {
            try
            {
                var tokenCredential = await GetOnBehalfOfCredential();
                await tokenCredential.GetTokenAsync(new TokenRequestContext(new string[] { _scope }), new CancellationToken());

                return true;
            }
            catch (Exception e)
            {
                if (e.Message.Contains("AADSTS65001"))
                {
                    NeedConsent = true;
                }
                else
                {
                    ErrorMessage = e.Message;
                }

            }
            return false;
        }

        private async Task<OnBehalfOfCredential> GetOnBehalfOfCredential()
        {
            var config = Configuration.Get<ConfigOptions>();
            var tenantId = config.TeamsFx.Authentication.OAuthAuthority.Remove(0, "https://login.microsoftonline.com/".Length);
            AccessToken ssoToken = await teamsUserCredential.GetTokenAsync(new TokenRequestContext(null), new CancellationToken());
            return new OnBehalfOfCredential(
                tenantId,
                config.TeamsFx.Authentication.ClientId,
                config.TeamsFx.Authentication.ClientSecret,
                ssoToken.Token
            );
        }

        private async Task<GraphServiceClient> GetGraphServiceClient()
        {
            var tokenCredential = await GetOnBehalfOfCredential();

            var client = new GraphServiceClient(tokenCredential, new string[] { _scope });
            return client;
        }
        public async Task<List<EducationUser>?> GetStudents(string Id)
        {
            _client = await GetGraphServiceClient();
            List<EducationUser> Students = new();
            try
            {
                if (Id != null && _client is not null)
                {
                    var members = await _client.Education.Classes[Id].Members.GetAsync();
                    Students = members.Value.Where(m => m.UserType == "Member" && m.PrimaryRole != EducationUserRole.Teacher).ToList();
                }

                return Students;
            }
            catch (ServiceException ex)
            {
                Console.WriteLine($"Error getting signed-in user: {ex.Message}");
                return null;
            }
        }
        public async Task<List<EducationClass>?> GetClasses(string UserId)
        {
            _client = await GetGraphServiceClient();
            List<EducationClass> Groups = new();
            try
            {
                if (UserId != null && _client is not null)
                {
                    var response = await _client.Education.Users[UserId].Classes.GetAsync();
                    Groups = response.Value.Where(c => !c.DisplayName.Contains("Exp")).ToList();
                }
                return Groups;
            }
            catch (ServiceException ex)
            {
                Console.WriteLine($"Error getting signed-in user: {ex.Message}");
                return null;
            }
        }
        public async Task<Student?> GetMe()
        {
            _client = await GetGraphServiceClient();
            string token = string.Empty;
            if (_client is null)
            {
                return default;
            }
            var user = await _client.Education.Me.GetAsync();
            if (user == null) return default;
            return new Student() { Id = Guid.Parse(user.Id), DisplayName = user.DisplayName, Role = user.PrimaryRole.ToString(), AccessToken = token };
        }
    }
}
