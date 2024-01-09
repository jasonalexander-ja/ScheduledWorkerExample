using System.Threading.Channels;
using ScheduledWorker.Models;

namespace ScheduledWorker.Services;

public class SchedulingService<TReq, TRes>
{
    /// <summary>
    ///  Thread to run managing and scheduling incomming requests. 
    /// </summary>
    private Thread SchedulingThread { get; set; }

    /// <summary>
    /// Channel in which requests can be sent and received in the SchedulingThread. 
    /// </summary>
    public Channel<Request<TReq, TRes>> SchedulingChannel { get; set; }

    /// <summary>
    /// Channel writer where work requests can be sent when requested via WorkRequestReceiver. 
    /// </summary>
    private ChannelWriter<Request<TReq, TRes>> WorkSender { get; set; }

    /// <summary>
    /// Channel reader where work can be requested. 
    /// </summary>
    private ChannelReader<bool> WorkRequestReceiver { get; set; }

    public SchedulingService(ChannelWriter<Request<TReq, TRes>> workerSender, ChannelReader<bool> workRequestReceiver)
    {
        SchedulingChannel = Channel.CreateUnbounded<Request<TReq, TRes>>();
        WorkSender = workerSender;
        WorkRequestReceiver = workRequestReceiver;
        SchedulingThread = new Thread(async () => await Scheduler(SchedulingChannel.Reader));
        SchedulingThread.Start();
    }

    private async Task Scheduler(ChannelReader<Request<TReq, TRes>> requestReceiver)
    {
        var requests = new Queue<Request<TReq, TRes>>();
        while (true)
        {
            var isReq = requestReceiver.TryRead(out var req);
            if (isReq && req is not null)
            {
                await SendScheduleMessage(req, requests.Count() + 1);
                requests.Enqueue(req);
            }
            var isWorkReq = WorkRequestReceiver.TryRead(out var _);
            if (isWorkReq)
            {
                // If work request is received but no work is scheduled, just wait 
                var item = requests.Any() ? requests.Dequeue() : await requestReceiver.ReadAsync();
                await WorkSender.WriteAsync(item);
                await SendClientsSchedulePos(requests);
            }
        }
    }

    private async Task SendClientsSchedulePos(Queue<Request<TReq, TRes>> requests)
    {
        foreach (var (v, i) in requests.Select((v, i) => (v, i)))
        {
            await SendScheduleMessage(v, i + 1);
        }
    }

    private async Task SendScheduleMessage(Request<TReq, TRes> req, int pos)
    {
        var schedMessage = Response<TRes>.SchedulerMessage(pos);
        try
        {
            await req.ResponseWriter.WriteAsync(schedMessage);
        }
        catch
        {
            // Client has probably disconected 
        }
    }
}
