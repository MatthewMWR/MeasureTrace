// Copyright and license at https://github.com/MatthewMWR/MeasureTrace/blob/master/LICENSE

namespace MeasureTrace.Calipers
{
    public static class WinlogonDomainKnowledge
    {
        internal const string WinlogonProviderName = "Microsoft-Windows-Winlogon";
        internal const int WinlogonSystemBootEventId = 5007;
        internal const int WinlogonWelcomeScreenStartId = 201;
    }

    /// <summary>
    ///     Definitions limited to those found in public WPA FullBoot Profile, others labeled as unknown pending better public
    ///     documentation
    /// </summary>
    public enum WinlogonNotificationType
    {
        CreateSession = 0,
        Unknown1,
        Logon = 2,
        Unknown3 = 3,
        Unknown4 = 4,
        Unknown5 = 5,
        Unknown6 = 6,
        Unknown7 = 7,
        ConsoleDisconnect = 8,
        Unknown9 = 9,
        Unknown10 = 10,
        Unknown11 = 11,
        StartShell = 12,
        Unknown13 = 13,
        Unknown14 = 14,
        Unknown15 = 15,
        Unknown16 = 16
    }
}