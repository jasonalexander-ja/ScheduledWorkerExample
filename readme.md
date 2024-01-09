# Scheduled Worker Example 

This is a quick example of implementing a request queuer with dynamic updates to the requester on queue position. 


## Description 

[WorkerService.cs](https://github.com/jasonalexander-ja/ScheduledWorkerExample/blob/main/ScheduledWorker/Services/WorkerService.cs#L39)
contains the main worker method that is started when this method is instantiated, it starts by issuing "not busy" request via a channel,
it then waits for a request to be sent, which it then hands over to a custom task runner that can be whatever the implementer wants it to be. 

The other part is [SchedulingService.cs](https://github.com/jasonalexander-ja/ScheduledWorkerExample/blob/main/ScheduledWorker/Services/SchedulingService.cs#L37), this maintains a queue of requests, when a request is sent by the "client", it is added to the queue with the queue position communicated to the client via a channel embedded in the request model. 

When a "not busy" flag from the worker service [is detected](https://github.com/jasonalexander-ja/ScheduledWorkerExample/blob/main/ScheduledWorker/Services/SchedulingService.cs#L48), the first request from the queue is sent via a channel. 

The SchedulingService never blocks for a while unless a not busy flag is encountered from the worker service and there is no requests in the queue, in which case it makes a blocking call for any client requests. 

This is all tied together in [ScheduledWorkerService.cs](https://github.com/jasonalexander-ja/ScheduledWorkerExample/blob/main/ScheduledWorker/Services/ScheduledWorkerService.cs), which in it's constructor initiates both the worker service, then the queue service, passing references to the relevant communication channels between the 2 (the request channel and "not busy" channel), and also exposing `SchedulingSender` channel writer for a service to easily send requests to the queueing thread. 

ScheduledWorkerService can be instantiated as a singleton and injected into a page where a user can send requests to the queue. 

## Example 

We first define 2 classes, [RequestModel.cs](https://github.com/jasonalexander-ja/ScheduledWorkerExample/blob/main/ScheduledWorkerExample/Models/RequestModel.cs) and [ResponseModel.cs](https://github.com/jasonalexander-ja/ScheduledWorkerExample/blob/main/ScheduledWorkerExample/Models/ResponseModel.cs), these are just simple wrappers for sending string messages. 

We then define a process for the worker thread to run when a request is made, [TaskRunner.cs](https://github.com/jasonalexander-ja/ScheduledWorkerExample/blob/main/ScheduledWorkerExample/TaskRunner.cs) is a simple static class with a static method `Runner` that takes a request model `Request<RequestModel, ResponseModel>` as a parameter, reads the string content of the request body before sending it back via the response channel withing the request model, before waiting 30 seconds to emulate some kind IO event. 

We simple instantiate and add the `ScheduledWorkerService` as a [singleton into the DI](https://github.com/jasonalexander-ja/ScheduledWorkerExample/blob/main/ScheduledWorkerExample/Program.cs#L20), using the mentioned runner. 

The [example page](https://github.com/jasonalexander-ja/ScheduledWorkerExample/blob/main/ScheduledWorkerExample/Pages/Index.razor) is set to the index page, we inject the `ScheduledWorkerService` [here](https://github.com/jasonalexander-ja/ScheduledWorkerExample/blob/main/ScheduledWorkerExample/Pages/Index.razor), then in the lifecycle method `OnAfterRenderAsync` [here](https://github.com/jasonalexander-ja/ScheduledWorkerExample/blob/main/ScheduledWorkerExample/Pages/Index.razor#L22) we check if this is the first render (prevents multiple requests bein sent) and send a request into the `ScheduledWorkerService`. 

We then [iterate over the responses](https://github.com/jasonalexander-ja/ScheduledWorkerExample/blob/main/ScheduledWorkerExample/Pages/Index.razor#L39) from the channel we passed it, there is a `IsSchedulerMessage` and `IsWorkerMessage` on the response model type the differentiate between queue messages and actual worker messages, we then simply update the user and continue if the message is a queue update, and if it's a worker message, we display that message before exiting the loop, in a real work situation we would want to add some kind of "end" flag into our custom response type. 

