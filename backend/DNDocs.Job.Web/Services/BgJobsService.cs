
using DNDocs.Job.Api.Management;
using DNDocs.Job.Web.Model;
using DNDocs.Job.Web.Shared;
using DNDocs.Job.Web.ValueTypes;
using Markdig.Extensions.TaskLists;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;
using Vinca.Utils;

namespace DNDocs.Job.Web.Services
{
    public interface IBgJobsService
    {
        Task AppStart();
        Task AppStopAsync();
        Task<bool> TryQueueBuildProjectAsync(BuildProjectModel BuildProjectModel);
    }

    public class BgJobsService : IBgJobsService
    {
        private IDJobRepository repository;
        private DJobSettings options;
        private ILogger<BgJobsService> logger;
        private IServiceProvider serviceProvider;
        bool isStarted = false;
        CancellationTokenSource cancellationTokenSource;
        int runningTasks;

        public BgJobsService(
            IOptions<DJobSettings> settings,
            ILogger<BgJobsService> logger,
            IServiceProvider serviceProvider,
            IDJobRepository repository)
        {
            this.repository = repository;
            this.options = settings.Value;
            this.logger = logger;
            this.serviceProvider = serviceProvider;
            cancellationTokenSource = new CancellationTokenSource();
            runningTasks = 0;
        }


        public async Task AppStart()
        {
            if (isStarted) throw new InvalidOperationException("already started");
            isStarted = true;

            // fail builds because app restarted
            var jobs = await repository.SelectBgJobByState(BgJobState.Processing);
            foreach (var job in jobs)
            {
                job.State = BgJobState.Failed;
                job.Exception = "system aborted processing job because of system restart";
                await repository.UpdateBgJobAsync(job);
            }

            await StartTasksIfNeeded();
        }

        public async Task AppStopAsync()
        {
            cancellationTokenSource.Cancel();

        }

        public async Task<bool> TryQueueBuildProjectAsync(BuildProjectModel BuildProjectModel)
        {
            if (await repository.CountJobsWaiting() >= options.MaxParallelBuildCount * 2) return false;

            var data = JsonSerializer.Serialize(BuildProjectModel);
            await repository.InsertJobAsync(BgJobState.Waiting, data, DateTime.UtcNow);

            await StartTasksIfNeeded();

            return true;
        }

        async Task StartTasksIfNeeded()
        {
            if (await repository.CountJobsWaiting() == 0) return;

            int threadSafeNowRunningOldVal = Interlocked.Increment(ref runningTasks);

            if (threadSafeNowRunningOldVal > options.MaxParallelBuildCount)
            {
                Interlocked.Decrement(ref runningTasks);
                return;
            }

            _ = Task.Run(StartTask);
        }

        async Task StartTask()
        {
            try
            {
                CancellationToken cancellationToken = cancellationTokenSource.Token;
                int threadId = Thread.CurrentThread.ManagedThreadId;
                BgJob nextJob = null;

                for (int i = 0; !cancellationToken.IsCancellationRequested; i++)
                {
                    logger.LogTrace("starting build  project task. loop iteration: {0},  taskid: {1}, ", i, Task.CurrentId);

                    using var scope = serviceProvider.CreateScope();
                    var docsBuilderService = scope.ServiceProvider.GetRequiredService<IDocsBuilderService>();
                    var repository = scope.ServiceProvider.GetRequiredService<IDJobRepository>();

                    try
                    {
                        nextJob = await repository.DequeueBgJob();
                        

                        if (nextJob == null) break;
                        logger.LogTrace("dequeue job to process, jobid: {0} loop iteration: {1},  taskid: {2}, ", nextJob.Id, i, Task.CurrentId);

                        nextJob.State = BgJobState.Processing;
                        nextJob.StartOn = DateTime.UtcNow;

                        await repository.UpdateBgJobAsync(nextJob);

                        BuildProjectModel project = JsonSerializer.Deserialize<BuildProjectModel>(nextJob.BuildData);

                        logger.LogInformation("starting thread {0}, job id: {1}", threadId, nextJob.Id);

                        await docsBuilderService.Handle(project);

                        nextJob.State = BgJobState.Success;
                        nextJob.CompletedOn = DateTime.UtcNow;

                    }
                    catch (Exception e)
                    {
                        logger.LogError(e, "thread exception: jobid: {0}, taskid: {1}", nextJob.Id, threadId);

                        nextJob.State = BgJobState.Success;
                        nextJob.CompletedOn = DateTime.UtcNow;
                        nextJob.Exception = Helpers.ExceptionToStringForLogs(e);
                    }
                    finally
                    {
                    }

                    repository.UpdateBgJobAsync(nextJob).Wait();
                }

                logger.LogInformation("successfully completed task: {0}. Task exited infinite loop {1}", threadId, DateTime.UtcNow);
            }
            catch
            {
            }
            finally
            {
                Interlocked.Decrement(ref runningTasks);
            }
        }
    }
}
