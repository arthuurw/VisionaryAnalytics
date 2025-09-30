namespace VisionaryAnalytics.Application.Configuration
{
    public class RabbitMQOptions
    {
        public required string HostName { get; set; }
        public required string UserName { get; set; }
        public required string Password { get; set; }
        public required RabbitMQQueues Queues { get; set; }
    }

    public class RabbitMQQueues
    {
        public required string ExtrairFrames { get; set; }
        public required string ProcessarFrames { get; set; }
    }
}
