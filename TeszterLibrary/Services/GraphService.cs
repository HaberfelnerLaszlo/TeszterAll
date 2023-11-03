using Azure.Core;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Kiota.Abstractions.Authentication;

using Newtonsoft.Json.Linq;

using TeszterLibrary.Model;

namespace TeszterLibrary.Services
{
    
	public class GraphService
	{
        private GraphServiceClient? client;

		public GraphServiceClient GetGraphServiceClient(IAuthenticationProvider authenticationProvider)
		{
			client = new GraphServiceClient(authenticationProvider);
			return client;
		}
		public async Task<List<EducationUser>?> GetStudents(string Id)
		{
			//var graphServiceClient = Gsc();
			List<EducationUser> Students = new();
			try
			{
				if (Id != null && client is not null)
				{
					var members = await client.Education.Classes[Id].Members.GetAsync();
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
			//var graphServiceClient = Gsc();
			List<EducationClass> Groups = new();
			try
			{
				if (UserId != null&& client is not null)
				{
					var response = await client.Education.Users[UserId].Classes.GetAsync();
					Groups = response.Value.ToList();
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
			//var graphServiceClient = Gsc();
			string token = string.Empty;
			if (client is null)
			{
				return default;
			}
			var user = await client.Education.Me.GetAsync();
			if (user == null) return default;
			return new Student() { Id = Guid.Parse(user.Id), DisplayName = user.DisplayName, Role = user.PrimaryRole.ToString(), AccessToken = token };
		}
		public async Task<List<DriveItem>> GetFolders()
		{
			//var graphServiceClient = Gsc();
			var ChildrenCollection = client.Me.Drive.GetAsync().Result.Root.Children.ToList();
			return ChildrenCollection.Where(item => item.Folder != null).ToList();
		}

		public async Task<string> CreateAppFolders()
		{
			//var graphServiceClient = Gsc();
			DriveItem driveItem = new()
			{
				Name = "Teszter",
				Folder = new Folder
				{
				},
				AdditionalData = new Dictionary<string, object>()
				{
					{"@microsoft.graph.conflictBehavior", "fail"}
				}
			};
			var mydrive = await client.Me.Drive.GetAsync();
			var ChildrenCollection = mydrive.Root.Children;
			if (ChildrenCollection.Contains(driveItem))
			{
				return ChildrenCollection.First(f => f.Name == driveItem.Name).Id;
			}
			//DriveItem appFolder;
			try
			{
				client.Drives[mydrive.Id].Root.GetAsync().Result.Children.Add(driveItem);
				//var type = "view";

				//var scope = "organization";

				//await graphServiceClient.Me.Drive.GetAsync().Result.Items[0]
				//	.CreateLink(type, scope, null, null, null, null)
				//	.Request()
				//	.PostAsync();
				//            driveItem.Name = "Images";
				//var imagesFolder = await graphServiceClient.Me.Drive.GetAsync().Result.Items[appFolder.Id].Children
				//	.Request()
				//	.AddAsync(driveItem);
				//driveItem.Name = "Uploads";
				//var uploadFolder = await graphServiceClient.Me.Drive.GetAsync().Result.Items[appFolder.Id].Children
				//	.Request()
				//	.AddAsync(driveItem);
				//return appFolder.Id;
				return string.Empty;
			}
			catch //(ServiceException exception)
			{
				//if (exception.StatusCode == HttpStatusCode.Conflict && exception.Error.Code == "nameAlreadyExists")
				//{
				//	var newFolder = graphServiceClient
				//		.Me
				//		.Drive.GetAsync().Result.Items.Find(d=>d.Equals(driveItem)); // the API lets us run searches https://docs.microsoft.com/en-us/graph/api/driveitem-search?view=graph-rest-1.0&tabs=csharp

				//	// since the search is likely to return more results we should filter it further
				//	driveItem = newFolder;// Just to ensure we're finding a folder, not a file with this name
				//	Console.WriteLine(driveItem?.Id); // your ID here
				//	return driveItem.Id;
				//}
				//else
				//{
				//	Console.WriteLine("Other ServiceException");
				throw;// handle this
					  //}
			}
		}

	}

}
