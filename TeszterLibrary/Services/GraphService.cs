using Azure.Core;

using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Drives.Item.Items.Item.CreateLink;
using Microsoft.Graph.Models;
using Microsoft.Kiota.Abstractions.Authentication;

using Newtonsoft.Json.Linq;

using TeszterLibrary.Model;

namespace TeszterLibrary.Services
{
    
	public class GraphService
	{
        private GraphServiceClient? client;
        private readonly ILogger<GraphService> logger;

        public string ErrorMessage { get; set; } = string.Empty;
        public GraphService(GraphServiceClient client, ILogger<GraphService> logger)
        {
            this.client = client;
            this.logger = logger;
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
					if (members == null)
					{
						throw new NullReferenceException("No members found");
                    }
                    Students = members.Value?.Where(m => m.UserType == "Member" && m.PrimaryRole != EducationUserRole.Teacher).ToList() ?? new List<EducationUser>();
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
			List<EducationClass> Groups = new();
			try
			{
				if (UserId != null&& client is not null)
				{
					var response = await client.Education.Users[UserId].Classes.GetAsync();
					if (response == null)
					{
						throw new NullReferenceException("No classes found");
                    }
                    Groups = response.Value?.Where(c => !c.DisplayName?.Contains("Exp")?? false).ToList() ?? new List<EducationClass>();
				}
				return Groups;
			}
			catch (ServiceException ex)
			{
                ErrorMessage = $"Error getting signed-in user: {ex.Message}";
                Console.WriteLine(ErrorMessage);
				return null;
			}
		}
		public async Task<List<EducationAssignment>?> GetAssignments(string ClassId)
        {
            //var graphServiceClient = Gsc();
            List<EducationAssignment> Assignments = new();
            try
            {
                if (ClassId != null && client is not null)
                {
                    var response = await client.Education.Classes[ClassId].Assignments.GetAsync();
					if (response == null) { 
						throw new NullReferenceException("No assignments found");
                    }
                    Assignments = response.Value?.ToList() ?? new List<EducationAssignment>();
                }
                return Assignments;
            }
            catch (ServiceException ex)
            {
				ErrorMessage = $"Error getting signed-in user: {ex.Message}";
                Console.WriteLine(ErrorMessage);
                return null;
            }
        }
 		public async Task<bool> CreateAssignments(EducationAssignment assignment,string ClassId, string link)
        {
 			if (client is null)
			{
				return false;
			}
           // To initialize your graphClient, see https://learn.microsoft.com/en-us/graph/sdks/create-client?from=snippets&tabs=csharp
            var result = await client.Education.Classes[ClassId].Assignments.PostAsync(assignment);
			if (result == null) {
                ErrorMessage = "Error creating assignment";
				Console.WriteLine(ErrorMessage);
                return false; }
			
            var requestBody = new EducationAssignmentResource
            {
                DistributeForStudentWork = false,
                Resource = new EducationLinkResource
                {
                    DisplayName = assignment.DisplayName,
                    Link = link,
                    OdataType = "#microsoft.graph.educationLinkResource",
                    AdditionalData = new Dictionary<string, object>
        {
            {
                "thumbnailPreviewUrl" ,String.Empty
            },
        },
                },
            };
            var result2 = await client.Education.Classes["{educationClass-id}"].Assignments["{educationAssignment-id}"].Resources.PostAsync(requestBody);
            return result2 != null;
        }
        public async Task<Student?> GetMe()
		{
			string token = string.Empty;
			if (client is null)
			{
				return default;
			}
			var user = await client.Education.Me.GetAsync();
			if (user == null) return default;
			return new Student() { Id = Guid.Parse(user.Id?? ""), DisplayName = user.DisplayName?? "", Role = user.PrimaryRole?.ToString()?? EducationUserRole.Student.ToString(), AccessToken = token };
		}
		public async Task<List<DriveItem>?> GetFolders()
		{
            if (client is null)
            {
				ErrorMessage = "Graph client is null";
				logger.LogError(ErrorMessage);
                return default;
            }
			var ChildrenCollection = await client.Me.Drive.GetAsync();
			return ChildrenCollection?.Root?.Children?.Where(item => item.Folder != null).ToList() ?? new List<DriveItem>();
		}
		public async Task<string> GetMyDrive()
		{
            if (client is null)
            {
				ErrorMessage = "Graph client is null";
				logger.LogError(ErrorMessage);
                return string.Empty;
            }
			var mydrive = await client.Me.Drive.GetAsync();
			if (mydrive == null) {
				ErrorMessage = "Error getting user's drive";
				logger.LogError(ErrorMessage);
				return string.Empty;
			}
			if (string.IsNullOrEmpty(mydrive.Id))
			{
				ErrorMessage = "Drive ID is null or empty";
				logger.LogError(ErrorMessage);
				return string.Empty;
            }
            return mydrive.Id;
        }
        public async Task<string> CreateAppFolder(string mydriveId)
		{
            string token = string.Empty;
            if (client is null)
            {
				ErrorMessage = "Graph client is null";	
                logger.LogError(ErrorMessage);
                return string.Empty;
            }
			var appFolder = await client.Drives[mydriveId].Special["approot"].GetAsync();
			if (appFolder == null)
			{
				ErrorMessage = "Error getting app folder";
				logger.LogError(ErrorMessage);
				return string.Empty;
			}
			if (string.IsNullOrEmpty(appFolder.Id))
			{
				ErrorMessage = "Drive ID is null or empty";
				logger.LogError(ErrorMessage);
				return string.Empty;
			}
			return appFolder.Id;
		}
		public async Task<bool> CheckIfFolderExists(string folderName, string mydriveId, string appFolderId)
		{
			string token = string.Empty;
			if (client is null)
			{
				ErrorMessage = "Graph client is null";
				logger.LogError(ErrorMessage);
				return false;
			}
			var children = await client.Drives[mydriveId].Items[appFolderId].Children.GetAsync();
			if (children == null || children.Value == null)
			{
				ErrorMessage = "Error getting folder children";
				logger.LogError(ErrorMessage);
				return false;
			}
			foreach (var item in children.Value)
			{
				if (item.Name == folderName && item.Folder != null)
				{
					return true;
				}
			}
			return false;
		}
		public async Task<string> GetFolderIdByName(string folderName, string mydriveId, string appFolderId)
		{
			string token = string.Empty;
			if (client is null)
			{
				ErrorMessage = "Graph client is null";
				logger.LogError(ErrorMessage);
				return string.Empty;
			}
			var children = await client.Drives[mydriveId].Items[appFolderId].Children.GetAsync();
			if (children == null)
			{
				ErrorMessage = "Error getting folder children";
				logger.LogError(ErrorMessage);
				return string.Empty;
			}
			if (children.Value == null || children.Value.Count == 0)
			{
				ErrorMessage = "No children found in the folder";
				logger.LogWarning(ErrorMessage);
				var folderId = await CreateImageFolders(mydriveId, appFolderId);
				if (!string.IsNullOrEmpty(folderId)) return folderId;
                return string.Empty;
            }
            var folder = children.Value.FirstOrDefault(v => v.Name == folderName && v.Folder != null);
			if (folder != null)
			{
				return folder.Id??string.Empty;
            }
			else
			{
				var folderId = await CreateImageFolders(mydriveId, appFolderId);
				if (!string.IsNullOrEmpty(folderId)) return folderId;
            }
				return string.Empty;
        }
        public async Task<string> CreateImageFolders(string mydriveId, string appFolderId)
		{
			string token = string.Empty;
			if (client is null)
			{
				ErrorMessage = "Graph client is null";
				logger.LogError(ErrorMessage);
				return string.Empty;
			}
			DriveItem driveItem = new()
			{
				Name = "Images",
				Folder = new Folder
				{
				},
				AdditionalData = new Dictionary<string, object>()
				{
					{"@microsoft.graph.conflictBehavior", "fail"}
				}
			};
			var createdFolder = await client.Drives[mydriveId].Items[appFolderId].Children.PostAsync(driveItem);
			if (createdFolder == null) {
				ErrorMessage = "Error creating folder";
				logger.LogError(ErrorMessage);
				return string.Empty;
			}
			if (string.IsNullOrEmpty(createdFolder.Id))
			{
				ErrorMessage = "Created folder ID is null or empty";
				logger.LogError(ErrorMessage);
				return string.Empty;
            }
            return createdFolder.Id;
		}
        public async Task<string> UploadFileToFolder(byte[] fileBytes, string fileName, string folderId, string mydriveId)
		{
			if (client is null)
			{
				return string.Empty;
			}
			using (var stream = new MemoryStream(fileBytes))
			{
				var uploadedFile = await client.Drives[mydriveId].Items[folderId].ItemWithPath(fileName).Content.PutAsync(stream);
				if (uploadedFile == null) return string.Empty;
                var requestBody = new CreateLinkPostRequestBody
                {
                    Type = "view",
                    Scope = "organization",
                    RetainInheritedPermissions = false,
                };
                var permission = await client.Drives[mydriveId].Items[uploadedFile.Id].CreateLink.PostAsync(requestBody);
				if (permission == null) return string.Empty;
                return permission.Link?.WebUrl ?? string.Empty;
			}
        }
        public void Dispose()
		{
			client = null;
        }
    }

}
