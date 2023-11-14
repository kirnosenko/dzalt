using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DZALT.Entities.Mapping
{
	public class PlayerMapping : IEntityTypeConfiguration<Player>
	{
		public void Configure(EntityTypeBuilder<Player> builder)
		{
			builder.ToTable("Players");

			builder.HasKey(a => a.Id);

			builder.Property(a => a.Guid)
				.HasMaxLength(44)
				.IsRequired();
			builder.HasIndex(a => a.Guid);
		}
	}
}
