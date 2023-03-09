using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScheduleEase.Models
{
    class Session
    {
        public string Name { get; set; }
        public string Subject { get; set; }
        public string Professor { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public override string ToString()
        {
            return $"Session {Name} ensured by {Professor} starting from {StartTime} to {EndTime}" ;
        }
    }
}
