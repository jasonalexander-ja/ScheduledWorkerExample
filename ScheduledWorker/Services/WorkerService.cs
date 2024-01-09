using System.Threading;
using System.Threading.Channels;
using ScheduledWorker.Models;


namespace ScheduledWorker.Services;

public class WorkerService<TReq, TRes>
{
    /// <summary>
    /// Thread to run receiver work requests and dispatching the given task. 
    /// </summary>
    private Thread WorkerThread { get; set; }

    /// <summary>
    /// Custom task to run upon a work request is received. 
    /// </summary>
    private Func<Request<TReq, TRes>, Task> TaskRunner { get; set; }

    /// <summary>
    /// Channel via which work requests can be sent and received. 
    /// </summary>
    public Channel<Request<TReq, TRes>> WorkerChannel { get; set; }

    /// <summary>
    /// Channel via which the WorkerThread can request work from the scheduler. 
    /// </summary>
    public Channel<bool> WorkRequestChannel { get; set; }

    public WorkerService(Func<Request<TReq, TRes>, Task> task)
    {
        TaskRunner = task;
        WorkRequestChannel = Channel.CreateUnbounded<bool>();
        WorkerChannel = Channel.CreateUnbounded<Request<TReq, TRes>>();
        WorkerThread = new Thread(async () => await Worker(WorkerChannel.Reader, WorkRequestChannel.Writer));
        WorkerThread.Start();
    }

    public async Task Worker(ChannelReader<Request<TReq, TRes>> requestReceiver, ChannelWriter<bool> workReqSender)
    {
        while (true)
        {
            await workReqSender.WriteAsync(true);
            var request = await requestReceiver.ReadAsync();

            if (request is null)
                continue;

            await TaskRunner(request);
        }
    }
}
