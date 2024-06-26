using SwiftUpdate.Models;
using SwiftUpdate.ViewModels;

namespace SwiftUpdate.Helpers
{
    public static class Methods
    {

        public static VersionsViewModel? FindAndReturnModelVersions(string AppFolder, ApplicationModel applicationModel)
        {
            try
            {
                // Construct the folder path based on application name
                string appDataFolderPath = AppFolder;

                // Check if the directory exists
                if (!Directory.Exists(appDataFolderPath))
                {
                    return null; // Handle appropriately if the directory does not exist
                }

                // Search for APK files
                var apkFiles = Directory.GetFiles(appDataFolderPath, "*.apk");

                List<int> versionCodes = new List<int>();
                List<string> fileNames = new List<string>();
                foreach (var apkFile in apkFiles)
                {
                    // Extract version code from the file name and convert to int
                    var versionCode = ExtractAndConvertVersionCode(apkFile);
                    if (versionCode.HasValue)
                    {
                        versionCodes.Add(versionCode.Value);
                    }

                    fileNames.Add(apkFile);
                }

                // Determine the highest version code (active version)
                int activeVersion = versionCodes.Any() ? versionCodes.Max() : 0;

                // Pass the versions and active version to the view
                var viewModel = new VersionsViewModel
                {
                    ApplicationModel = applicationModel,
                    Versions = versionCodes,
                    ActiveVersion = activeVersion,
                    FileNames = fileNames
                };


                return viewModel;
            } catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                return null;
            }
        }


        public static List<int> FindAndReturnModelVersionsApi(string AppFolder)
        {
            try
            {
                // Construct the folder path based on application name
                string appDataFolderPath = AppFolder;

                // Check if the directory exists
                if (!Directory.Exists(appDataFolderPath))
                {
                    return null; // Handle appropriately if the directory does not exist
                }

                // Search for APK files
                var apkFiles = Directory.GetFiles(appDataFolderPath, "*.apk");
                List<int> versionCodes = new List<int>();
                foreach (var apkFile in apkFiles)
                {
                    // Extract version code from the file name and convert to int
                    var versionCode = ExtractAndConvertVersionCode(apkFile);
                    if (versionCode.HasValue)
                    {
                        versionCodes.Add(versionCode.Value);
                    }

                }
                // Determine the highest version code (active version)
                int activeVersion = versionCodes.Any() ? versionCodes.Max() : 0;
                return versionCodes;
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                return new List<int>();
            }
        }

        public static string FindAndReturnUpdatePath(string AppFolder)
        {
            string updatePath = string.Empty;
            try
            {
                // Construct the folder path based on application name
                string appDataFolderPath = AppFolder;
                // Check if the directory exists
                if (!Directory.Exists(appDataFolderPath))
                {
                    updatePath = string.Empty;
                }
                // Search for APK files
                var apkFiles = Directory.GetFiles(appDataFolderPath, "*.apk");
                List<int> versionCodes = new List<int>();
                foreach (var apkFile in apkFiles)
                {
                    // Extract version code from the file name and convert to int
                    var versionCode = ExtractAndConvertVersionCode(apkFile);
                    if (versionCode.HasValue)
                    {
                        versionCodes.Add(versionCode.Value);
                    }
                }
                int max = versionCodes.Max();
                // Determine the highest version code (active version)
                foreach (var apkFile in apkFiles)
                {
                    // Extract version code from the file name and convert to int
                    var versionCode = ExtractAndConvertVersionCode(apkFile);
                    if (versionCode == max)
                    {
                        updatePath = apkFile;
                    }
                }

            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
            } 

            return updatePath;
        }



        private static int? ExtractAndConvertVersionCode(string fileName)
        {

            try
            {
                // Example: Extract version code from file name following '__v'
                // Convert to int and return the highest number
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
                var startIndex = fileNameWithoutExtension.LastIndexOf("__v", StringComparison.OrdinalIgnoreCase);
                if (startIndex != -1 && startIndex + 3 < fileNameWithoutExtension.Length)
                {
                    var versionString = fileNameWithoutExtension.Substring(startIndex + 3);
                    if (int.TryParse(versionString, out int versionCode))
                    {
                        return versionCode;
                    }
                }

                return null; // Return null if version code pattern not found or cannot be parsed
            } catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                return -1;
            }
        }
    }
}
