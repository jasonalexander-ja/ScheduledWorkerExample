using System.Threading.Channels;


namespace ScheduledWorker.Models;

public class Request<TReq, TRes>
{
    public ChannelWriter<Response<TRes>> ResponseWriter { get; set; }
    public TReq RequestBody { get; set; }

    
    public Request(ChannelWriter<Response<TRes>> responseWriter, TReq requestBody)
    {
        ResponseWriter = responseWriter;
        RequestBody = requestBody;
    }
}
