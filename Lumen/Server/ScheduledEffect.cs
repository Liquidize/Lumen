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
        public DayOfWeek DaysOfWeek { get; protected set; }
        [JsonProperty("startHour")]
        public uint StartHour { get; protected set; }
        [JsonProperty("startMinute")]
        public uint StartMinute { get; protected set;}
        [JsonProperty("endHour")]
        public uint EndHour { get; protected set; }
        [JsonProperty("endMinute")]
        public uint EndMinute { get; protected set;}

        [JsonProperty("effectSettings")]
        public Dictionary<string,object> EffectSettings { get; protected set; }

        public ScheduledEffect(DayOfWeek daysOfWeek, string effectName, uint startHour, uint endHour,
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
                if (DaysOfWeek.HasFlag(DateTime.Now.DayOfWeek))
                    if (DateTime.Now.Hour > StartHour || DateTime.Now.Hour == StartHour && DateTime.Now.Minute >= StartMinute)
                        if (DateTime.Now.Hour < EndHour || DateTime.Now.Hour == EndHour && DateTime.Now.Minute <= EndMinute)
                            return true;

                return false;
            }
        }
    }
}
