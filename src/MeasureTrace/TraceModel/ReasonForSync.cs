// Copyright and license at https://github.com/MatthewMWR/MeasureTrace/blob/master/LICENSE

namespace MeasureTrace.TraceModel
{
    public enum ReasonForSync
    {
        NoNeedForSync = 0,
        FirstPolicyRefresh = 1,
        CseRequiresForeground = 2,
        CseReturnedError = 3,
        ForcedSyncRefresh = 4,
        SyncPolicy = 5,
        Sku = 7,
        ScriptsSync = 8,
        SyncDueToInternalProcessingError = 9,
        Unknown = 6
    }
}