using System;

namespace NET_Guardian
{
    public class ActivityLogEntry
    {
        public int ActivityLogEntryId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}