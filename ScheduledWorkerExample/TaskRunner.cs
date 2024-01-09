using ScheduledWorker.Models;
using ScheduledWorkerExample.Models;

namespace ScheduledWorkerExample;

public static class TaskRunner
{

    public static async Task Runner(Request<RequestModel, ResponseModel> request)
    {
        try
        {
            var response = new ResponseModel($"Received \"{request.RequestBody.Message}\", at: {DateTime.Now.ToString("mm:ss")} ");
            await request.ResponseWriter.WriteAsync(Response<ResponseModel>.WorkerMessage(response));
        }
        catch
        {
            // Client probably disconected. 
        }
        await Task.Delay(30000);
    }
}
