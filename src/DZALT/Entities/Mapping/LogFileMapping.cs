using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DZALT.Entities.Mapping
{
	public class LogFileMapping : IEntityTypeConfiguration<LogFile>
	{
		public void Configure(EntityTypeBuilder<LogFile> builder)
		{
			builder.ToTable("LogFiles");

			builder.HasKey(a => a.Id);

			builder.Property(a => a.Name)
				.HasMaxLength(256)
				.IsRequired();
			builder.HasIndex(a => a.Name);

			builder.Property(a => a.DateFrom);

			builder.Property(a => a.DateTo);
		}
	}
}
