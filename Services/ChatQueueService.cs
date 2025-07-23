using Microsoft.Extensions.Logging;
using MoneyBaseChatSupport.Configuration;
using MoneyBaseChatSupport.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoneyBaseChatSupport.Services
{
    public class ChatQueueService
    {
        private readonly Queue<ChatSession> _queue = new();
        private readonly ChatSettings _settings;
        private readonly ILogger<ChatQueueService> _logger;

        public List<Agent> Agents { get; }
        public List<Agent> OverflowAgents { get; }

        public List<ChatSession> AllChats { get; } = new();

        public ChatQueueService(List<Agent> agents, List<Agent> overflowAgents, ChatSettings settings, ILogger<ChatQueueService> logger)
        {
            Agents = agents;
            OverflowAgents = overflowAgents;
            _settings = settings;
            _logger = logger;
        }

        public int MaxCapacity => (int)Math.Floor(Agents.Sum(a => _settings.MaxConcurrentChats));
        public int MaxQueueSize => (int)(MaxCapacity * _settings.MaxQueueMultiplier);

        public ChatSession CreateChat()
        {
            _logger.LogDebug("Creating a new chat session");

            if (_queue.Count >= MaxQueueSize && !IsOfficeHours())
            {
                _logger.LogWarning("Chat request rejected. Reason: Queue full (Count: {Count}, Max: {Max}) and outside office hours.",
                    _queue.Count, MaxQueueSize);
                return null;
            }

            var chat = new ChatSession();
            _queue.Enqueue(chat);
            AllChats.Add(chat);
            _logger.LogInformation("New chat created with ID {ChatId}. Queue size is now {Count}.", chat.Id, _queue.Count);
            return chat;
        }

        public bool IsOfficeHours()
        {
            var now = DateTime.Now.TimeOfDay;
            var start = TimeSpan.Parse(_settings.OfficeHoursStart);
            var end = TimeSpan.Parse(_settings.OfficeHoursEnd);
            return now >= start && now < end;
        }

        public void AssignChats()
        {
            _logger.LogInformation("Assigning chats. Current queue size: {QueueSize}", _queue.Count);
            while (_queue.Any())
            {
                var chat = _queue.Peek();
                var agent = GetNextAvailableAgent();

                if (agent == null) {
                    _logger.LogWarning("No available agent to assign chat {ChatId}.", chat.Id);
                    break; 
                }

                chat.AssignedAgent = agent;
                _logger.LogInformation("Chat {ChatId} assigned to agent {AgentName} (Level: {Level}).", chat.Id, agent.Name, agent.Level);
                agent.AssignedChats.Add(chat.Id);
                _queue.Dequeue();
            }
        }

        private Agent GetNextAvailableAgent()
        {
            var sorted = Agents
                .OrderBy(a => a.Level) 
                .ToList();

            foreach (var agent in sorted)
            {
                if (agent.IsOnShift && agent.AssignedChats.Count < _settings.MaxConcurrentChats)
                    return agent;
            }

            if (IsOfficeHours())
            {
                return OverflowAgents.FirstOrDefault(o => o.AssignedChats.Count < _settings.MaxConcurrentChats);
            }

            return null;
        }
    }
}
