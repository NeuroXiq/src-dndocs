
using DNDocs.Application.Application;
using DNDocs.Application.Shared;
using DNDocs.Domain.Entity.App;
using DNDocs.Domain.Repository;
using DNDocs.Domain.UnitOfWork;
using DNDocs.Infrastructure.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DNDocs.Application.Services
{
    public interface IBgJobQueue
    {
        public Task Enqueue(ICommand command, int userId, int? buildsProjectId);
        bool TryDequeue(out BgJobItem item);
        Task OnSystemStart();
    }

    internal class BgJobQueue : IBgJobQueue
    {
        private IServiceProvider serviceProvider;
        private ConcurrentQueue<BgJobItem> queue;

        public BgJobQueue(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
            queue = new ConcurrentQueue<BgJobItem>();
        }

        public async Task Enqueue(ICommand command, int userId, int? buildsProjectId)
        {
            using var scope = serviceProvider.CreateAsyncScope();
            var uow = scope.ServiceProvider.GetRequiredService<IAppUnitOfWork>();

            var job = new BgJob(command?.GetType().FullName, JsonConvert.SerializeObject(command), userId);
            job.BuildsProjectId = buildsProjectId;

            await uow.BgJobRepository.CreateAsync(job);
            await uow.SaveChangesAsync();

            queue.Enqueue(new BgJobItem(command, userId, job.Id));
        }

        public bool TryDequeue(out BgJobItem item)
        {
            return queue.TryDequeue(out item);
        }

        public async Task OnSystemStart()
        {
            using var scope = this.serviceProvider.CreateScope();
            var uow = scope.ServiceProvider.GetRequiredService<IAppUnitOfWork>();
            var bgjobQueue = scope.ServiceProvider.GetRequiredService<IBgJobQueue>();
            var jobsToFail = await uow.BgJobRepository.Query().Where(t => t.Status == Domain.Enums.BgJobStatus.InProgress).ToListAsync();
            var jobsToRestore = await uow.BgJobRepository.Query().Where(t => t.Status == Domain.Enums.BgJobStatus.WaitingToStart).ToListAsync();

            jobsToFail.ForEach(t => t.Status = Domain.Enums.BgJobStatus.FailedToStart);

            foreach (var job in jobsToRestore)
            {
                ICommand command = (ICommand)JsonConvert.DeserializeObject(job.DoWorkCommandData, Assembly.GetExecutingAssembly().GetType(job.DoWorkCommandType));
                this.queue.Enqueue(new BgJobItem(command, job.ExecuteAsUserId, job.Id));
            }

            await uow.SaveChangesAsync();
        }


        public async Task<int> GetJobsCountInQueueBeforeJob(int bgjob)
        {
            using var scope = this.serviceProvider.CreateScope();
            var uow = scope.ServiceProvider.GetRequiredService<IAppUnitOfWork>();
            var job = uow.BgJobRepository.GetByIdChecked(bgjob);

            return  await uow.BgJobRepository.Query()
                .Where(t => t.QueuedDateTime < job.QueuedDateTime && job.Status == Domain.Enums.BgJobStatus.WaitingToStart)
                .CountAsync();
        }

        public async Task<double> GetJobsExecutionEstimates()
        {
            using var scope = this.serviceProvider.CreateScope();
            var uow = scope.ServiceProvider.GetRequiredService<IAppUnitOfWork>();

            var lastJobs = await uow.BgJobRepository.Query().Where(t =>
                t.Status == Domain.Enums.BgJobStatus.Completed &&
                t.CommandHandlerSuccess == true &&
                t.StartedDateTime != null &&
                t.CompletedDateTime != null
                )
                .OrderByDescending(t => t.CompletedDateTime)
                .Select(t => new { t.StartedDateTime, t.CompletedDateTime })
                .Take(150)
                .ToListAsync();

            double avgTimeSeconds = lastJobs.Count == 0 ? 0 : lastJobs.Average(t => (t.StartedDateTime.Value - t.CompletedDateTime.Value).TotalSeconds);

            return avgTimeSeconds;
        }
    }

    public class BgJobItem
    {
        public BgJobItem(ICommand command, int userId, int bgjobId)
        {
            Command = command;
            UserId = userId;
            BgJobId = bgjobId;
        }

        public ICommand Command { get; private set; }
        public int UserId { get; private set; }
        public int BgJobId { get; private set; }
    }
}
