using System;

namespace MoneyBaseChatSupport.Models
{
    public class ChatSession
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Agent AssignedAgent { get; set; }
        public int MissedPolls { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public DateTime LastPollTime { get; set; } = DateTime.UtcNow;
    }
}
