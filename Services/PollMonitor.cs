using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MoneyBaseChatSupport.Services
{
    public class PollMonitor : BackgroundService
    {
        private readonly ChatQueueService _chatQueueService;
        private readonly ILogger<PollMonitor> _logger;

        public PollMonitor(ChatQueueService chatQueueService, ILogger<PollMonitor> logger)
        {
            _chatQueueService = chatQueueService;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("PollMonitor started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.UtcNow;

                foreach (var chat in _chatQueueService.AllChats.Where(c => c.IsActive))
                {

                    var secondsSinceLastPoll = (now - chat.LastPollTime).TotalSeconds;

                    if (secondsSinceLastPoll >= 3)
                    {
                        chat.MissedPolls++;

                        if (chat.MissedPolls >= 3)
                        {
                            chat.IsActive = false;
                            chat.AssignedAgent?.AssignedChats.Remove(chat.Id);
                            _logger.LogWarning("Chat {ChatId} marked inactive after {Seconds} seconds and {MissedPolls} missed polls.",
                                                chat.Id, secondsSinceLastPoll, chat.MissedPolls);
                        }
                        {
                            _logger.LogInformation("Chat {ChatId} has missed {MissedPolls} polls.", chat.Id, chat.MissedPolls);
                        }
                    }
                }

                // increased to 5 seconds, for testing pourpose
                await Task.Delay(5000, stoppingToken);
            }

            _logger.LogInformation("PollMonitor stopped.");
        }
    }
}
