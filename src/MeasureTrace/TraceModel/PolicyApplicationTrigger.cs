//  Written and shared by Microsoft employee Matthew Reynolds in the spirit of "Small OSS libraries, tool, and sample code" OSS policy
//  MIT license https://github.com/MatthewMWR/MeasureTrace/blob/master/LICENSE 
namespace MeasureTrace.TraceModel
{
    public enum PolicyApplicationTrigger
    {
        None = 0,
        Boot,
        LogOn,
        NetworkStateChange,
        Manual,
        Periodic,
        Invalid
    }
}