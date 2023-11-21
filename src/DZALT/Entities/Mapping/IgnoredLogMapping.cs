using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DZALT.Entities.Mapping
{
	public class IgnoredLogMapping : IEntityTypeConfiguration<IgnoredLog>
	{
		public void Configure(EntityTypeBuilder<IgnoredLog> builder)
		{
			builder.ToTable("IgnoredLogs");

			builder.HasKey(x => x.Id);

			builder.HasOne(x => x.Player)
				.WithMany((string)null)
				.HasForeignKey(x => x.PlayerId)
				.IsRequired();

			builder.Property(x => x.Date);
			builder.HasIndex(x => x.Date);

			builder.Property(x => x.Body)
				.HasMaxLength(1024)
				.IsRequired(true);
		}
	}
}
