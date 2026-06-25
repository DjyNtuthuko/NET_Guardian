using System;
using System.Collections.Generic;
using System.Linq;

namespace NET_Guardian
{
    public class ActivityLogger
    {
        public void Log(string action, string details)
        {
            ActivityLogEntry entry = new ActivityLogEntry
            {
                Action = action,
                Details = details,
                CreatedAt = DateTime.Now
            };

            using NetGuardianDbContext database = new NetGuardianDbContext();
            database.ActivityLogEntries.Add(entry);
            database.SaveChanges();
        }

        public List<ActivityLogEntry> GetEntries()
        {
            using NetGuardianDbContext database = new NetGuardianDbContext();
            return database.ActivityLogEntries
                .OrderByDescending(entry => entry.CreatedAt)
                .ToList();
        }
    }
}