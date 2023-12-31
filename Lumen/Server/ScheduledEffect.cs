using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Lumen.Server
{
    public class ScheduledEffect
    {
        public const DayOfWeek WeekEnds = DayOfWeek.Saturday | DayOfWeek.Sunday;
        public const DayOfWeek WeekDays = DayOfWeek.Monday | DayOfWeek.Tuesday | DayOfWeek.Wednesday | DayOfWeek.Thursday | DayOfWeek.Friday;
        public const DayOfWeek AllDays = WeekDays | WeekEnds;

        [JsonProperty("effectName")]
        public string EffectName { get; protected set; }
        [JsonProperty("daysOfWeek")]
        public string[] DaysOfWeek { get; protected set; }
        [JsonProperty("startHour")]
        public uint StartHour { get; protected set; }
        [JsonProperty("startMinute")]
        public uint StartMinute { get; protected set;}
        [JsonProperty("endHour")]
        public uint EndHour { get; protected set; }
        [JsonProperty("endMinute")]
        public uint EndMinute { get; protected set;}

        [JsonProperty("settings")]
        public Dictionary<string,object> Settings { get; protected set; }

        [JsonProperty("id")]
        public string Id { get; protected set; } = Guid.NewGuid().ToString("N").Substring(0, 8);

        public ScheduledEffect(string[] daysOfWeek, string effectName, uint startHour, uint endHour,
            uint startMinute = 0, uint endMinute = 60)
        {
            EffectName = effectName;
            DaysOfWeek = daysOfWeek;
            StartHour = startHour;
            StartMinute = startMinute;
            EndHour = endHour;
            EndMinute = endMinute;
        }

        public bool IsEffectScheduledToRunNow
        {
            get
            {
                if (DaysOfWeek.Any(d => (d.Equals(DateTime.Now.DayOfWeek.ToString(), StringComparison.OrdinalIgnoreCase) || d.Equals("all", StringComparison.OrdinalIgnoreCase))))
                    if (DateTime.Now.Hour > StartHour || DateTime.Now.Hour == StartHour && DateTime.Now.Minute >= StartMinute)
                        if (DateTime.Now.Hour < EndHour || DateTime.Now.Hour == EndHour && DateTime.Now.Minute <= EndMinute)
                            return true;

                return false;
            }
        }
    }
}
