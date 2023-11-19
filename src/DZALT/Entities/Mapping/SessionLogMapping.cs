using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DZALT.Entities.Mapping
{
	public class SessionLogMapping : IEntityTypeConfiguration<SessionLog>
	{
		public void Configure(EntityTypeBuilder<SessionLog> builder)
		{
			builder.ToTable("SessionLogs");

			builder.HasKey(x => x.Id);

			builder.HasOne(x => x.Player)
				.WithMany((string)null)
				.HasForeignKey(x => x.PlayerId)
				.IsRequired();

			builder.Property(x => x.Date);
			builder.HasIndex(x => x.Date);

			builder.Property(x => x.Type);
			builder.HasIndex(x => x.Type);
		}
	}
}
