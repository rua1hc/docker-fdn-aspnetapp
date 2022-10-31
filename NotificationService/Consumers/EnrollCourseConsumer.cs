using MassTransit;

using NotificationService.Models;
using NotificationService.Services;

using SharedModels;

namespace NotificationService.Consumers
{
    class EnrollCourseConsumer : IConsumer<CourseEnrolled>
    {
        readonly ILogger<EnrollCourseConsumer> _logger;
        private readonly IMailService _mailService;

        public EnrollCourseConsumer(ILogger<EnrollCourseConsumer> logger,
                                    IMailService mailService)
        {
            _logger = logger;
            _mailService = mailService;
        }

        public async Task Consume(ConsumeContext<CourseEnrolled> context)
        {
            var mailContent = $"{context.Message.UserName} - {context.Message.UserEmail} - {context.Message.CourseName} - {context.Message.CoursePrice}USD";
            _logger.LogInformation("New enrollment: {mailContent}", mailContent);
            //_mailService.SendMail(mailContent);

            var mailMessage = new MailMessage(body: mailContent,
                                subject: $"Course Enrolled: {context.Message.CourseName}");
            await _mailService.SendMailAsync(mailMessage, new CancellationToken());
        }
    }

    class EnrollCourseConsumerDefinition : ConsumerDefinition<EnrollCourseConsumer>
    {
        public EnrollCourseConsumerDefinition()
        {
            // override the default endpoint name
            EndpointName = "course-enrolled";

            // limit the number of messages consumed concurrently
            // this applies to the consumer only, not the endpoint
            ConcurrentMessageLimit = 6;
        }

        protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
                                    IConsumerConfigurator<EnrollCourseConsumer> consumerConfigurator)
        {
            // configure message retry with millisecond intervals
            endpointConfigurator.UseMessageRetry(r => r.Intervals(2000, 4000, 8000));

            // use the outbox to prevent duplicate events from being published
            endpointConfigurator.UseInMemoryOutbox();
        }
    }

}
