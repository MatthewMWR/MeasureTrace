//  Written and shared by Microsoft employee Matthew Reynolds in the spirit of "Small OSS libraries, tool, and sample code" OSS policy
//  MIT license https://github.com/MatthewMWR/MeasureTrace/blob/master/LICENSE 
namespace MeasureTrace.TraceModel
{
    public enum ServiceStartupType
    {
        // http://support.microsoft.com/kb/103000
        Boot = 0,
        System = 1,
        Auto = 2,
        Demand = 3,
        Disabled = 4
    }
}