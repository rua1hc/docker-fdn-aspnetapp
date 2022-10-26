using MassTransit;

using Microsoft.EntityFrameworkCore;

using System.Collections.Generic;
using System.Reflection.Emit;

namespace ReportService.Models
{
    public class NetReportDbContext : DbContext
    {
        public NetReportDbContext(DbContextOptions<NetReportDbContext> options) : base(options) { }

        public virtual DbSet<Report> Reports { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Report>(entity =>
            {
                entity.Property(e => e.CourseName).IsRequired().HasMaxLength(255);
            });
        }
    }
}
