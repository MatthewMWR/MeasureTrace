//  Written and shared by Microsoft employee Matthew Reynolds in the spirit of "Small OSS libraries, tool, and sample code" OSS policy
//  MIT license https://github.com/MatthewMWR/MeasureTrace/blob/master/LICENSE 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using MeasureTrace.TraceModel;

namespace MeasureTrace.Calipers
{
    public static class GroupPolicyDomainKnowledge
    {
        public const string GroupPolicyProviderName = "Microsoft-Windows-GroupPolicy";
        private const string DistinguishedNameSplitter = @"(?<!\\),";
        public const int GroupPolicyWinlogonNotificationStartEventId = 5324;
        public const int GroupPolicyWinlogonNotificationStopEventId = 5351;
        public const string CseIdFieldName = "CSEExtensionId";
        public const string CseDurationFieldName = "CSEElaspedTimeInMilliSeconds";
        public const int ScriptStartEventId = 4018;
        public const int ScriptStopEventId = 5018;

        public static List<Tuple<Guid, string>> ExtensionIdList = new List<Tuple<Guid, string>>();

        public static bool IsActivityStartEventId(int eventId)
        {
            return eventId >= 4000 && eventId < 4010;
        }

        public static bool IsActivityEndEventId(int eventId)
        {
            return eventId >= 6000 && eventId%1000 < 10;
        }

        public static PolicyApplicationMode MeasurePolicyApplicationMode(bool isBackgroundProcessing,
            bool isAsyncProcessing)
        {
            if (isBackgroundProcessing)
            {
                return PolicyApplicationMode.Background;
            }
            if (isAsyncProcessing)
            {
                return PolicyApplicationMode.ForegroundAsync;
            }
            return PolicyApplicationMode.ForegroundSync;
        }

        public static GpoScope MeasureGpoScope(int isMachine)
        {
            return isMachine == 1 ? GpoScope.Machine : GpoScope.User;
        }

        public static string GetCseLabelInvariant(Guid cseGuid, bool throwOnUnknown)
        {
            var formattedGuid = cseGuid.ToString("B");

            //  See if we can find it in ExtensionIDs
            var extensionIdEntities =
                typeof (ExtensionIDs).GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly);

            var matchFi =
                extensionIdEntities.FirstOrDefault(
                    fi =>
                    {
                        var piVal = (string) fi.GetRawConstantValue();
                        var piGuid = Guid.Parse(piVal);
                        return piGuid == cseGuid;
                    });

            if (matchFi != null) return matchFi.Name;

            //  Try other lookup options (future, none available now)

            //  If lookup failed, either return the original guid back as a string or throw
            if (throwOnUnknown)
            {
                throw new ArgumentOutOfRangeException("cseGuid", cseGuid, "Not found in references");
            }
            return formattedGuid;
        }

        public static string GetCseLabelInvariant(Guid cseGuid)
        {
            return GetCseLabelInvariant(cseGuid, false);
        }

        public static bool IsCseStartEventId(int eventId)
        {
            return eventId == 4016;
        }

        public static bool IsCseEndEventId(int eventId)
        {
            return eventId > 5000 && eventId%1000 == 16;
        }

        public static PolicyApplicationTrigger ResolvePolicyApplicationTrigger(int startEventId)
        {
            if (startEventId == 4000) return PolicyApplicationTrigger.Boot;
            if (startEventId == 4001) return PolicyApplicationTrigger.LogOn;
            if (startEventId == 4002 || startEventId == 4003) return PolicyApplicationTrigger.NetworkStateChange;
            if (startEventId == 4004 || startEventId == 4005) return PolicyApplicationTrigger.Manual;
            if (startEventId == 4006 || startEventId == 4007) return PolicyApplicationTrigger.Periodic;
            return PolicyApplicationTrigger.Invalid;
        }

        public static string ConvertActiveDirectoryDistinguishedNameToDnsDomainName(string distinguishedName)
        {
            if (string.IsNullOrWhiteSpace(distinguishedName))
            {
                return string.Empty;
            }
            return string.Join(".",
                SplitDistinguishedName(distinguishedName)
                    .Where(st => st.StartsWith("DC=", StringComparison.OrdinalIgnoreCase))
                    .Select(st => Regex.Replace(st, "DC=", "")));
        }

        public static string[] SplitDistinguishedName(string distinguishedName)
        {
            return Regex.Split(distinguishedName, DistinguishedNameSplitter);
        }
    }

    public class ExtensionIDs
    {
    }
}