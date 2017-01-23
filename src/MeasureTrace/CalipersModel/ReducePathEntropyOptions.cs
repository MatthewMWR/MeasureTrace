//  Written and shared by Microsoft employee Matthew Reynolds in the spirit of "Small OSS libraries, tool, and sample code" OSS policy
//  MIT license https://github.com/MatthewMWR/MeasureTrace/blob/master/LICENSE 
namespace MeasureTrace.CalipersModel
{
    public class ReducePathEntropyOptions
    {
        //public string ProgramFilesPath { get; set; }
        //public string ProgramFilesx86Path { get; set; }
        //public string ProgramW6432Path { get; set; }
        //public string CommonProgramFilesPath { get; set; }
        //public string CommonProgramFilesx86Path { get; set; }
        //public string CommonProgramW6432Path { get; set; }
        //public string ProgramDataPath { get; set; }
        //public string UserProfilesRootPath { get; set; }
        //public string WinDirPath { get; set; }

        public ReducePathEntropyOptions()
        {
            // Defaults
            DepthLimit = 2;
            LengthLimit = 80;
            DepthBoostSpecial = 1;
            //ProgramFilesPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            //ProgramFilesx86Path = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            //ProgramW6432Path = Environment.GetEnvironmentVariable("ProgramW6432");
            //CommonProgramFilesPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFiles);
            //CommonProgramFilesx86Path = Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFilesX86);
            //CommonProgramW6432Path = Environment.GetEnvironmentVariable("CommonProgramW6432Path");
            //ProgramDataPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            //WinDirPath = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
            //UserProfilesRootPath = Regex.Replace(Environment.GetEnvironmentVariable("PUBLIC"), "PUBLIC", "");
        }

        public int DepthLimit { get; set; }
        public int LengthLimit { get; set; }
        public int DepthBoostSpecial { get; set; }
    }
}