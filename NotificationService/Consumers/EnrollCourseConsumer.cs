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
            _logger.LogInformation("_logger enrollment: {Id} - {UserId} - {CourseId} - {EnrolledDate}",
                context.Message.Id, context.Message.UserId,
                context.Message.CourseId, context.Message.EnrolledDate);
            Console.WriteLine($"_CW enrollment: {context.Message.Id} - {context.Message.UserId} - {context.Message.CourseId} - {context.Message.EnrolledDate}");

            var mailContent = $"New enrollment: {context.Message.Id} - {context.Message.UserId} - {context.Message.CourseId} - {context.Message.EnrolledDate}";
            _mailService.SendMail(mailContent);

            //{
            //    "to": [
            //        "joesph.beatty9@ethereal.email"
            //    ],
            //    "displayName": "Christian Schou",
            //    "replyTo": "test@mail.dk",
            //    "replyToName": "Test mail",
            //    "subject": "Hello World",
            //    "body": "Hola - this is just a test to verify that our mailing works. Have a great day!"
            //}
            var mailMessage = new MailMessage("Course Enrollment Registration", body: mailContent);
            await _mailService.SendMailAsync(mailMessage, new CancellationToken());
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
