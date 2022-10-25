using MassTransit;
using NotificationService.Contracts;

namespace NotificationService.Consumers
{

    class EnrollCourseConsumer : IConsumer<CourseEnrolled>
    {
        readonly ILogger<EnrollCourseConsumer> _logger;

        public EnrollCourseConsumer(ILogger<EnrollCourseConsumer> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<CourseEnrolled> context)
        {
            _logger.LogInformation("New enrollment: {Id} - {UserId} - {CourseId} - {EnrolledDate}", 
                context.Message.Id, 
                context.Message.UserId,
                context.Message.CourseId, 
                context.Message.EnrolledDate);
        }
    }

    class EnrollCourseConsumerDefinition : ConsumerDefinition<EnrollCourseConsumer>
    {
        public EnrollCourseConsumerDefinition()
        {
            // override the default endpoint name
            EndpointName = "course-service";

            // limit the number of messages consumed concurrently
            // this applies to the consumer only, not the endpoint
            ConcurrentMessageLimit = 8;
        }

        protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
                                    IConsumerConfigurator<EnrollCourseConsumer> consumerConfigurator)
        {
            // configure message retry with millisecond intervals
            endpointConfigurator.UseMessageRetry(r => r.Intervals(100, 200, 500, 800, 1000));

            // use the outbox to prevent duplicate events from being published
            endpointConfigurator.UseInMemoryOutbox();
        }
    }

}
