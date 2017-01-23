//  Written and shared by Microsoft employee Matthew Reynolds in the spirit of "Small OSS libraries, tool, and sample code" OSS policy
//  MIT license https://github.com/MatthewMWR/MeasureTrace/blob/master/LICENSE 
namespace MeasureTrace.TraceModel
{
    public enum BootPhaseType
    {
        None = 0,
        PreSmss,
        Smss,
        Winlogon,
        Explorer,
        PostBoot,
        FromPowerOnUntilReadyForLogon,
        FromLogonUntilDesktopAppears,
        FromDesktopAppearsUntilDesktopResponsive,
        FromPowerOnUntilDesktopAppears,
        FromPowerOnUntilDesktopResponsive
    }
}