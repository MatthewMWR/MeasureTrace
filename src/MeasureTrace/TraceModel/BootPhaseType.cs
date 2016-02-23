// Copyright and license at https://github.com/MatthewMWR/MeasureTrace/blob/master/LICENSE

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