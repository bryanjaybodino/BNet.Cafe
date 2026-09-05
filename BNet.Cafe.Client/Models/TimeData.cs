using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BNet.Cafe.Client.Models
{
    /// <summary>
    /// Represents time allocation data for a client computer.
    /// Stores start time, end time, and time-related metadata.
    /// </summary>
    public class TimeData
    {
        /// <summary>Client/Account name.</summary>
        public string AccountName { get; set; }

        /// <summary>When the client session started (UTC).</summary>
        public DateTime StartTime { get; set; }

        /// <summary>When the client session ends/will end (UTC).</summary>
        public DateTime EndTime { get; set; }

        /// <summary>Total allocated minutes for this session.</summary>
        public int TotalMinutes { get; set; }

        /// <summary>Remaining minutes.</summary>
        public int RemainingMinutes { get; set; }

        /// <summary>Is the session currently expired?</summary>
        public bool IsExpired { get; set; }

        /// <summary>Timestamp when this data was last updated.</summary>
        public DateTime LastUpdated { get; set; }

        /// <summary>Optional message (e.g., "Session expired" or "Time extended").</summary>
        public string Message { get; set; }
    }
}