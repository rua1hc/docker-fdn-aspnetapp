using MassTransit;
using Microsoft.Extensions.Logging;
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
            var mailContent = $"New enrollment: {context.Message.Id} - {context.Message.UserId} - {context.Message.CourseId} - {context.Message.EnrolledDate}";
            _logger.LogInformation(mailContent);

            var mailMessage = new MailMessage(subject: "Course Enrollment Registration 2", body: mailContent);
            await _mailService.SendMailAsync(mailMessage, new CancellationToken());

            _mailService.SendMail(mailContent);
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
            endpointConfigurator.UseMessageRetry(r => r.Intervals(2000, 4000, 8000));

            // use the outbox to prevent duplicate events from being published
            endpointConfigurator.UseInMemoryOutbox();
        }
    }

}
