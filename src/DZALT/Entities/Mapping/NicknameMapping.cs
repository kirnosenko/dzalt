using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DZALT.Entities.Mapping
{
	public class NicknameMapping : IEntityTypeConfiguration<Nickname>
	{
		public void Configure(EntityTypeBuilder<Nickname> builder)
		{
			builder.ToTable("Nicknames");

			builder.HasKey(x => x.Id);

			builder.Property(x => x.Name)
				.HasMaxLength(256)
				.IsRequired();
			builder.HasIndex(x => x.Name);

			builder.HasOne(x => x.Player)
				.WithMany((string)null)
				.HasForeignKey(x => x.PlayerId)
				.IsRequired();
		}
	}
}
