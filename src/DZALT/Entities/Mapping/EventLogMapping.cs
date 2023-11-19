using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DZALT.Entities.Mapping
{
	public class EventLogMapping : IEntityTypeConfiguration<EventLog>
	{
		public void Configure(EntityTypeBuilder<EventLog> builder)
		{
			builder.ToTable("EventLogs");

			builder.HasKey(x => x.Id);

			builder.HasOne(x => x.Player)
				.WithMany((string)null)
				.HasForeignKey(x => x.PlayerId)
				.IsRequired();

			builder.Property(x => x.Date);
			builder.HasIndex(x => x.Date);

			builder.Property(x => x.X);
			builder.Property(x => x.Y);
			builder.Property(x => x.Z);

			builder.HasOne(x => x.EnemyPlayer)
				.WithMany((string)null)
				.HasForeignKey(x => x.EnemyPlayerId)
				.IsRequired(false);

			builder.Property(x => x.EnemyPlayerX);
			builder.Property(x => x.EnemyPlayerY);
			builder.Property(x => x.EnemyPlayerZ);

			builder.Property(x => x.Event);
			builder.HasIndex(x => x.Event);

			builder.Property(x => x.Damage);

			builder.Property(x => x.Health);

			builder.Property(x => x.Enemy)
				.HasMaxLength(64)
				.IsRequired(false);

			builder.Property(x => x.BodyPart)
				.HasMaxLength(32)
				.IsRequired(false);

			builder.Property(x => x.Hitter)
				.HasMaxLength(64)
				.IsRequired(false);

			builder.Property(x => x.Weapon)
				.HasMaxLength(64)
				.IsRequired(false);

			builder.Property(x => x.Distance);
		}
	}
}
