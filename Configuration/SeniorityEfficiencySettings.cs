using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoneyBaseChatSupport.Configuration
{
    public class SeniorityEfficiencySettings
    {
        public double Junior { get; set; }
        public double MidLevel { get; set; }
        public double Senior { get; set; }
        public double TeamLead { get; set; }

        public double GetEfficiency(string level)
        {
            return level switch
            {
                "Junior" => Junior,
                "MidLevel" => MidLevel,
                "Senior" => Senior,
                "TeamLead" => TeamLead,
                _ => 0.0
            };
        }
    }
}
