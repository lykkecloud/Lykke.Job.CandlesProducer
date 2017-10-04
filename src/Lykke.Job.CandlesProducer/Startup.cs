using System;
using System.Collections.Generic;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using AzureStorage.Tables;
using Common.Log;
using Lykke.Common.ApiLibrary.Middleware;
using Lykke.Common.ApiLibrary.Swagger;
using Lykke.Job.CandlesProducer.Core.Domain.Candles;
using Lykke.Job.CandlesProducer.Core.Services;
using Lykke.Job.CandlesProducer.Models;
using Lykke.Job.CandlesProducer.Modules;
using Lykke.Job.CandlesProducer.Services.Settings;
using Lykke.Logs;
using Lykke.SettingsReader;
using Lykke.SlackNotification.AzureQueue;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lykke.Job.CandlesProducer
{
    public class Startup
    {
        public IHostingEnvironment Environment { get; }
        public IContainer ApplicationContainer { get; private set; }
        public IConfigurationRoot Configuration { get; }
        public ILog Log { get; private set; }

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
            Environment = env;

            Console.WriteLine($"ENV_INFO: {System.Environment.GetEnvironmentVariable("ENV_INFO")}");
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddMvc()
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.ContractResolver =
                        new Newtonsoft.Json.Serialization.DefaultContractResolver();
                });

            services.AddSwaggerGen(options =>
            {
                options.DefaultLykkeConfiguration("v1", "CandlesProducer API");
            });

            var builder = new ContainerBuilder();
            var appSettings = Configuration.LoadSettings<AppSettings>();
            var quotesSourceType = appSettings.CurrentValue.CandlesProducerJob != null ? QuotesSourceType.Spot : QuotesSourceType.Mt;
            var jobSettings = quotesSourceType == QuotesSourceType.Spot 
                ? appSettings.Nested(x => x.CandlesProducerJob) 
                : appSettings.Nested(x => x.MtCandlesProducerJob);

            Log = CreateLogWithSlack(
                services, 
                appSettings.CurrentValue.SlackNotifications,
                jobSettings.ConnectionString(x => x.Db.LogsConnString));
            
            builder.RegisterModule(new JobModule(jobSettings, appSettings.Nested(x => x.Assets), quotesSourceType, Log));

            builder.Populate(services);

            ApplicationContainer = builder.Build();

            return new AutofacServiceProvider(ApplicationContainer);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime appLifetime)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseLykkeMiddleware("CandlesProducer", ex => new ErrorResponse { ErrorMessage = "Technical problem" });

            app.UseMvc();
            app.UseSwagger();
            app.UseSwaggerUi();
            app.UseStaticFiles();

            appLifetime.ApplicationStarted.Register(StartApplication);
            appLifetime.ApplicationStopping.Register(StopApplication);
            appLifetime.ApplicationStopped.Register(CleanUp);
        }

        private void StartApplication()
        {
            try
            {
                Console.WriteLine("Starting...");

                var startupManager = ApplicationContainer.Resolve<IStartupManager>();

                startupManager.StartAsync().Wait();

                Console.WriteLine("Started");
            }
            catch (Exception ex)
            {
                Log.WriteFatalErrorAsync(nameof(Startup), nameof(StartApplication), "", ex);
            }
        }

        private void StopApplication()
        {
            try
            {
                Console.WriteLine("Stopping...");

                var shutdownManager = ApplicationContainer.Resolve<IShutdownManager>();

                shutdownManager.ShutdownAsync();

                Console.WriteLine("Stopped");
            }
            catch (Exception ex)
            {
                Log.WriteFatalErrorAsync(nameof(Startup), nameof(StopApplication), "", ex);
            }
        }

        private void CleanUp()
        {
            try
            {
                Console.WriteLine("Cleaning up...");

                ApplicationContainer.Dispose();

                Console.WriteLine("Cleaned up");
            }
            catch (Exception ex)
            {
                Log.WriteFatalErrorAsync(nameof(Startup), nameof(CleanUp), "", ex);
            }
        }

        private static ILog CreateLogWithSlack(IServiceCollection services, SlackNotificationsSettings slackSettings, IReloadingManager<string> dbLogConnectionStringManager)
        {
            var consoleLogger = new LogToConsole();
            var aggregateLogger = new AggregateLogger();

            aggregateLogger.AddLog(consoleLogger);

            // Creating slack notification service, which logs own azure queue processing messages to aggregate log
            var slackService = services.UseSlackNotificationsSenderViaAzureQueue(new AzureQueueIntegration.AzureQueueSettings
            {
                ConnectionString = slackSettings.AzureQueue.ConnectionString,
                QueueName = slackSettings.AzureQueue.QueueName
            }, aggregateLogger);

            var dbLogConnectionString = dbLogConnectionStringManager.CurrentValue;

            // Creating azure storage logger, which logs own messages to concole log
            if (!string.IsNullOrEmpty(dbLogConnectionString) && !(dbLogConnectionString.StartsWith("${") && dbLogConnectionString.EndsWith("}")))
            {
                var persistenceManager = new LykkeLogToAzureStoragePersistenceManager(
                    AzureTableStorage<LogEntity>.Create(dbLogConnectionStringManager, "CandlesProducerLog", consoleLogger),
                    consoleLogger);

                var slackNotificationsManager = new LykkeLogToAzureSlackNotificationsManager(slackService, consoleLogger);

                var azureStorageLogger = new LykkeLogToAzureStorage(
                    persistenceManager,
                    slackNotificationsManager,
                    consoleLogger);

                azureStorageLogger.Start();

                aggregateLogger.AddLog(azureStorageLogger);
            }

            return aggregateLogger;
        }
    }
}