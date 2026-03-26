using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RockSolid.Foundation.Modeling.AspNetCore;

internal sealed class OutboxMessageEntityTypeConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.HasKey(e => e.Index);
        builder.Property(e => e.Index)
            .IsRequired();
        builder.Property(e => e.Id)
            .IsRequired();
        builder.Property(e => e.Format)
            .IsRequired();
        builder.Property(e => e.SentAt)
            .IsRequired();
        builder.Property(e => e.Data)
            .IsRequired();
    }
}