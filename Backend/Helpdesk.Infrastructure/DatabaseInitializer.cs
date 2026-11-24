using System.Reflection;
using Helpdesk.Application;
using Helpdesk.Domain;
using Microsoft.EntityFrameworkCore;

namespace Helpdesk.Infrastructure;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(
        HelpdeskDbContext database,
        IPasswordService passwords,
        CancellationToken cancellationToken = default)
    {
        await database.Database.EnsureCreatedAsync(cancellationToken);
        await CreateDemoDataAsync(database, passwords, cancellationToken);

        await using var resource = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream("Helpdesk.Infrastructure.Sql.GetTicketDashboardStats.sql")
            ?? throw new InvalidOperationException("The ticket dashboard SQL procedure resource is missing.");
        using var reader = new StreamReader(resource);
        var sql = await reader.ReadToEndAsync(cancellationToken);
        await database.Database.ExecuteSqlRawAsync(sql, cancellationToken);
    }

    private static async Task CreateDemoDataAsync(
        HelpdeskDbContext database,
        IPasswordService passwords,
        CancellationToken cancellationToken)
    {
        if (await database.Users.AnyAsync(cancellationToken))
            return;

        var admin = CreateUser("admin@supportqueue.com", "Maya Chen", UserRole.Admin, "Operations", passwords);
        var agent = CreateUser("agent@supportqueue.com", "Jordan Ellis", UserRole.SupportAgent, "IT Support", passwords);
        var employee = CreateUser("employee@supportqueue.com", "Alex Morgan", UserRole.Employee, "Finance", passwords);
        var secondEmployee = CreateUser("priya@supportqueue.com", "Priya Shah", UserRole.Employee, "People", passwords);
        database.Users.AddRange(admin, agent, employee, secondEmployee);

        var now = DateTime.UtcNow;
        database.Tickets.AddRange(
            CreateTicket("VPN access drops after sleep mode", "The VPN disconnects whenever my laptop wakes from sleep.", "Hardware", TicketPriority.High, TicketStatus.InProgress, employee, agent, now.AddHours(-2)),
            CreateTicket("New starter laptop setup — Priya Shah", "Please prepare a laptop and standard development access for Priya.", "Hardware", TicketPriority.Medium, TicketStatus.Open, secondEmployee, null, now.AddHours(-5)),
            CreateTicket("Cannot access the finance shared drive", "I receive an access denied message on the finance shared drive.", "Access", TicketPriority.Urgent, TicketStatus.Open, employee, agent, now.AddDays(-1)),
            CreateTicket("Slack notifications delayed", "Notifications are arriving several minutes late on desktop.", "Software", TicketPriority.Low, TicketStatus.Resolved, secondEmployee, agent, now.AddDays(-2)),
            CreateTicket("Request access to design library", "I need read access to the shared design library.", "Access", TicketPriority.Medium, TicketStatus.Resolved, employee, agent, now.AddDays(-3)));

        await database.SaveChangesAsync(cancellationToken);
    }

    private static SupportUser CreateUser(
        string email, string fullName, UserRole role, string department, IPasswordService passwords)
    {
        var user = new SupportUser
        {
            Email = email,
            FullName = fullName,
            Role = role,
            Department = department
        };
        user.PasswordHash = passwords.Hash(user, "QueueDemo!2026");
        return user;
    }

    private static Ticket CreateTicket(
        string subject,
        string description,
        string category,
        TicketPriority priority,
        TicketStatus status,
        SupportUser requester,
        SupportUser? assignee,
        DateTime createdAtUtc)
    {
        var ticket = new Ticket
        {
            TicketNumber = $"SQ-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}",
            Subject = subject,
            Description = description,
            Category = category,
            Priority = priority,
            Status = status,
            CreatedBy = requester,
            CreatedById = requester.Id,
            AssignedTo = assignee,
            AssignedToId = assignee?.Id,
            CreatedAtUtc = createdAtUtc,
            UpdatedAtUtc = createdAtUtc,
            ResolvedAtUtc = status == TicketStatus.Resolved ? createdAtUtc.AddHours(3) : null
        };
        return ticket;
    }
}
