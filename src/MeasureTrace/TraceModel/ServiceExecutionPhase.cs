//  Written and shared by Microsoft employee Matthew Reynolds in the spirit of "Small OSS libraries, tool, and sample code" OSS policy
//  MIT license https://github.com/MatthewMWR/MeasureTrace/blob/master/LICENSE 
namespace MeasureTrace.TraceModel
{
    public enum ServiceExecutionPhase
    {
        None = 0,
        AutoStart = 1,
        DelayedAutoStart = 2
    }
}