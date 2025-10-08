namespace RHF_Foundation.Models
{
    public class EventItem
    {
        public string MonthAbbrev { get; set; }      // e.g., "OCT"
        public string EventName { get; set; }        // e.g., "RHF Family Camp"
        public string EventDate { get; set; }        // e.g., "Oct 25, 2025"
        public string TimeRange { get; set; }        // e.g., "10:00 AM – 2:00 PM"
        public string LinkText { get; set; }         // e.g., "View event details"
        public string LinkUrl { get; set; }          // e.g., "https://example.com/event/123"
    }
}