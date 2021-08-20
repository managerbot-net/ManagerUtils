using System;
using System.IO;
using System.Linq;
using System.Threading;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;

namespace ManagerUtilities.BackUp
{
    public class GoogleService
    {
        private static readonly string[] Scopes = { DriveService.Scope.Drive };
        private const string ApplicationName = "ManagerUtils";
        private const string FolderName = "backup";

        private DriveService _service;

        public DriveService Service => _service ?? CreateService();

        private DriveService CreateService()
        {
            UserCredential credential;

            using (var stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {
                const string credPath = "token.json";
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                Console.WriteLine("Credential file saved to: " + credPath);
            }

            _service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            return _service;
        }

        public string UploadFile(Stream file, string fileName, string fileMime)
        {
            var driveFile = new Google.Apis.Drive.v3.Data.File
            {
                Name = fileName,
                MimeType = fileMime,
                Parents = new[] {GetFolderId()}
            };

            var request = Service.Files.Create(driveFile, file, fileMime);
            request.Fields = "id";

            var response = request.Upload();
            if (response.Status != Google.Apis.Upload.UploadStatus.Completed)
                throw response.Exception;

            return request.ResponseBody.Id;
        }

        public string GetFolderId()
        {
            var listRequest = Service.Files.List();
            listRequest.PageSize = 10;
            listRequest.Fields = "nextPageToken, files(id, name)";

            return listRequest.Execute()
                .Files.FirstOrDefault(x => x.Name == FolderName)?.Id;
        }
    }
}