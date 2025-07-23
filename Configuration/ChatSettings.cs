using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoneyBaseChatSupport.Configuration
{
    public class ChatSettings
    {
        public double MaxQueueMultiplier { get; set; }
        public double MaxConcurrentChats { get; set; }
        public string OfficeHoursStart { get; set; }
        public string OfficeHoursEnd { get; set; }
        public SeniorityEfficiencySettings SeniorityEfficiency { get; set; }
    }
}
