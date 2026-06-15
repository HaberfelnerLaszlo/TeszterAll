using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TeszterLibrary.Services;

namespace TeszterLibrary
{
    public class FolderData(GraphService graphService)
    {
        private readonly GraphService graphService = graphService;

        public string MyDriveId { get; set; } = string.Empty;
        public string ImageFolderId { get; set; } = string.Empty;
        public string AppFolderId { get; set; } = string.Empty;
        public async void AddFolderIds()
        {
            if (string.IsNullOrEmpty(MyDriveId))
                MyDriveId = await graphService.GetMyDrive();
            if (string.IsNullOrEmpty(AppFolderId))
                AppFolderId = await graphService.CreateAppFolder(MyDriveId);
            if (string.IsNullOrEmpty(ImageFolderId))
                ImageFolderId = await graphService.GetFolderIdByName("Images", MyDriveId, AppFolderId);
        }
    }
}
