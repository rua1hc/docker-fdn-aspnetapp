using MassTransit;

using Microsoft.EntityFrameworkCore;

using ReportService.Models;

using SharedModels;

namespace ReportService.Consumers
{

    class EnrollCourseConsumer : IConsumer<CourseEnrolled>
    {
        readonly ILogger<EnrollCourseConsumer> _logger;
        private readonly NetReportDbContext _context;

        public EnrollCourseConsumer(ILogger<EnrollCourseConsumer> logger,
                                    NetReportDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task Consume(ConsumeContext<CourseEnrolled> context)
        {
            var mailContent = $"{context.Message.UserName} - {context.Message.UserEmail} - {context.Message.CourseName} - {context.Message.CoursePrice}USD";
            _logger.LogInformation("New enrollment: {mailContent}", mailContent);

            var report = await _context.Reports.FirstOrDefaultAsync(r => r.CourseName == context.Message.CourseName);
            if(report == null)
            {
                _context.Reports.Add(new Report()
                {
                    CourseName = context.Message.CourseName,
                    TotalPayment = context.Message.CoursePrice,
                    Month = context.Message.EnrolledDate.Month,
                    Year = context.Message.EnrolledDate.Year
                });
                await _context.SaveChangesAsync();
            }
            else
            {
                report.TotalPayment+=context.Message.CoursePrice;
                await _context.SaveChangesAsync();
            }
        }
    }

    class EnrollCourseConsumerDefinition : ConsumerDefinition<EnrollCourseConsumer>
    {
        public EnrollCourseConsumerDefinition()
        {
            // override the default endpoint name
            EndpointName = "course-enrolled-rp";

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
