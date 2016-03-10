using System.Diagnostics.Tracing;
namespace MsEventSourceActivities
{
    [EventSource(Name = "MS-Activities")]
    class MsEventSource : EventSource
    {
        public static MsEventSource Log = new MsEventSource();
        
        public void RequestStart(string url) { WriteEvent(1, url); }
        public void RequestStop(bool success) { WriteEvent(2, success); }
    }
}