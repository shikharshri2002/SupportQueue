using Helpdesk.Domain;
using Microsoft.EntityFrameworkCore;

namespace Helpdesk.Infrastructure;

public sealed class HelpdeskDbContext(DbContextOptions<HelpdeskDbContext> options) : DbContext(options)
{
    public DbSet<SupportUser> Users => Set<SupportUser>();
    public DbSet<Ticket> Tickets => Set<Ticket>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SupportUser>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(user => user.Id);
            entity.Property(user => user.Email).HasMaxLength(254).IsRequired();
            entity.HasIndex(user => user.Email).IsUnique();
            entity.Property(user => user.PasswordHash).HasMaxLength(512).IsRequired();
            entity.Property(user => user.FullName).HasMaxLength(120).IsRequired();
            entity.Property(user => user.Department).HasMaxLength(80).IsRequired();
            entity.Property(user => user.Role).HasConversion<string>().HasMaxLength(24).IsRequired();
        });

        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.ToTable("Tickets");
            entity.HasKey(ticket => ticket.Id);
            entity.Property(ticket => ticket.TicketNumber).HasMaxLength(20).IsRequired();
            entity.HasIndex(ticket => ticket.TicketNumber).IsUnique();
            entity.HasIndex(ticket => new { ticket.Status, ticket.CreatedAtUtc });
            entity.HasIndex(ticket => new { ticket.Priority, ticket.Status });
            entity.HasIndex(ticket => new { ticket.CreatedById, ticket.CreatedAtUtc });
            entity.Property(ticket => ticket.Subject).HasMaxLength(120).IsRequired();
            entity.Property(ticket => ticket.Description).HasMaxLength(3000).IsRequired();
            entity.Property(ticket => ticket.Category).HasMaxLength(40).IsRequired();
            entity.Property(ticket => ticket.Priority).HasConversion<string>().HasMaxLength(16).IsRequired();
            entity.Property(ticket => ticket.Status).HasConversion<string>().HasMaxLength(16).IsRequired();
            entity.HasOne(ticket => ticket.CreatedBy)
                .WithMany(user => user.CreatedTickets)
                .HasForeignKey(ticket => ticket.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(ticket => ticket.AssignedTo)
                .WithMany(user => user.AssignedTickets)
                .HasForeignKey(ticket => ticket.AssignedToId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
