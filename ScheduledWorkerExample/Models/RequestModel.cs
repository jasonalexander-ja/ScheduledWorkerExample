namespace ScheduledWorkerExample.Models;

public class RequestModel
{
    public string Message { get; set; }

    public RequestModel(string message)
    { 
        Message = message;
    }
}
