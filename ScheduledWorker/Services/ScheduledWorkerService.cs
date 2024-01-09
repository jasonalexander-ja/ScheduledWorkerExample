using ScheduledWorker.Models;
using System.Threading.Channels;

namespace ScheduledWorker.Services;

public class ScheduledWorkerService<TReq, TRes>
{
    private WorkerService<TReq, TRes> Worker;
    private SchedulingService<TReq, TRes> Scheduler;

    public ChannelWriter<Request<TReq, TRes>> SchedulingSender
    {
        get => Scheduler.SchedulingChannel.Writer;
    }

    public ScheduledWorkerService(Func<Request<TReq, TRes>, Task> task)
    {
        Worker = new WorkerService<TReq, TRes>(task);
        Scheduler = new SchedulingService<TReq, TRes>(Worker.WorkerChannel.Writer, Worker.WorkRequestChannel.Reader);
    }
}
