
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace SwiftUpdate.ViewModels
{
    public class VersionsViewModel
    { 
        public Models.ApplicationModel ApplicationModel;
        public List<int> Versions;
        public List<string> FileNames;
        public int ActiveVersion;
 
    }
}
