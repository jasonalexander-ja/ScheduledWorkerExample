namespace ScheduledWorker.Models;

public class SchedulingMessage
{
    public int Position { get; set; }
    
    public SchedulingMessage(int position)
    {
        Position = position;
    }
}
