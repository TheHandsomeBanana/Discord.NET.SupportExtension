using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord.NET.SupportExtension.Models {
    public class RunLogEntry {
        public DateTime StartedAt { get; set; }
        public DateTime FinishedAt { get; set; }
        public JobStatus Status { get; set; }
        public TimeSpan Duration { get => FinishedAt - StartedAt; }
    }

    public enum JobStatus {
        Failed,
        Succeeded
    }
}
