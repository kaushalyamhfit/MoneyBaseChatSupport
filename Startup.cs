using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MoneyBaseChatSupport.Configuration;
using MoneyBaseChatSupport.Models;
using MoneyBaseChatSupport.Services;
using System.Collections.Generic;
using System.Linq;

namespace MoneyBaseChatSupport
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Bind appsettings.json section to ChatSettings
            var chatSettings = Configuration.GetSection("ChatConfig").Get<ChatSettings>();
            services.AddSingleton(chatSettings);

            // Register ChatQueueService with injected settings
            services.AddSingleton<ChatQueueService>(sp =>
            {
                var settings = sp.GetRequiredService<ChatSettings>();
                var logger = sp.GetRequiredService<ILogger<ChatQueueService>>();

                var agents = new List<Agent>
                {
                    new() { Name = "Junior1", Level = Seniority.Junior, Efficiency = settings.SeniorityEfficiency.Junior },
                    new() { Name = "Mid1", Level = Seniority.MidLevel, Efficiency = settings.SeniorityEfficiency.MidLevel },
                    new() { Name = "Mid2", Level = Seniority.MidLevel, Efficiency = settings.SeniorityEfficiency.MidLevel },
                    new() { Name = "TeamLead1", Level = Seniority.TeamLead, Efficiency = settings.SeniorityEfficiency.TeamLead }
                };

                var overflowAgents = Enumerable.Range(1, 6)
                    .Select(i => new Agent
                    {
                        Name = $"Overflow{i}",
                        Level = Seniority.Junior,
                        Efficiency = settings.SeniorityEfficiency.Junior
                    })
                    .ToList();

                return new ChatQueueService(agents, overflowAgents, settings,logger);
            });
            services.AddControllers();
            services.AddHostedService<PollMonitor>();
            services.AddSwaggerGen();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Support Chat API v1");
                });
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
