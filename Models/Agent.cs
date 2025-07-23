using System;
using System.Collections.Generic;

namespace MoneyBaseChatSupport.Models
{
    public class Agent
    {
        public string Name { get; set; }
        public Seniority Level { get; set; }
        public bool IsOnShift { get; set; } = true;
        public double Efficiency { get; set; }
        public List<Guid> AssignedChats { get; set; } = new();
    }
}
