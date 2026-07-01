using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NexusERP.Domain.Entities;
using NexusERP.Domain.Enums;
using NexusERP.Infrastructure.Persistence;
using TaskStatus = NexusERP.Domain.Enums.TaskStatus;

namespace NexusERP.Infrastructure.BackgroundServices;

public class DataSeederHostedService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DataSeederHostedService> _logger;

    public DataSeederHostedService(IServiceProvider serviceProvider, ILogger<DataSeederHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

        await EnsureDatabaseAsync(context, cancellationToken);
        await EnsureMeetingsSchemaAsync(context, cancellationToken);

        await SeedRolesAsync(roleManager);
        await SeedPermissionsAsync(context);
        await SeedRolePermissionsAsync(context, roleManager);
        await EnsureMeetingPermissionsAsync(context, roleManager);
        await SeedAdminUserAsync(userManager, roleManager);
        await SeedSampleUsersAsync(userManager, roleManager);
        await SeedProjectsAsync(context, userManager, cancellationToken);
        await SeedTasksAsync(context, userManager, cancellationToken);
        await SeedAuditLogsAsync(context, userManager, cancellationToken);
        await SeedNotificationsAsync(context, userManager, cancellationToken);
        await SeedMeetingsAsync(context, userManager, cancellationToken);
        _logger.LogInformation("Database seeded successfully");
    }

    private async Task EnsureDatabaseAsync(ApplicationDbContext context, CancellationToken cancellationToken)
    {
        if (await context.Database.CanConnectAsync(cancellationToken) && await SchemaExistsAsync(context, cancellationToken))
        {
            _logger.LogInformation("Database schema already exists");
            return;
        }

        if (await context.Database.CanConnectAsync(cancellationToken))
        {
            _logger.LogWarning("Database exists but schema is incomplete — recreating");
            await context.Database.EnsureDeletedAsync(cancellationToken);
        }

        await context.Database.EnsureCreatedAsync(cancellationToken);
        _logger.LogInformation("Database schema created");
    }

    private static async Task<bool> SchemaExistsAsync(ApplicationDbContext context, CancellationToken cancellationToken)
    {
        try
        {
            var connection = context.Database.GetDbConnection();
            if (connection.State != System.Data.ConnectionState.Open)
                await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'AspNetRoles'";
            var result = await command.ExecuteScalarAsync(cancellationToken);
            return result != null;
        }
        catch
        {
            return false;
        }
    }

    private async Task EnsureMeetingsSchemaAsync(ApplicationDbContext context, CancellationToken cancellationToken)
    {
        if (!await context.Database.CanConnectAsync(cancellationToken)) return;
        if (await TableExistsAsync(context, "Meetings", cancellationToken)) return;

        _logger.LogWarning("Meetings table missing — applying incremental schema update");

        await context.Database.ExecuteSqlRawAsync("""
            IF OBJECT_ID(N'[Meetings]', N'U') IS NULL
            BEGIN
                CREATE TABLE [Meetings] (
                    [Id] uniqueidentifier NOT NULL,
                    [CreatedAt] datetime2 NOT NULL,
                    [CreatedBy] nvarchar(max) NULL,
                    [UpdatedAt] datetime2 NULL,
                    [UpdatedBy] nvarchar(max) NULL,
                    [IsDeleted] bit NOT NULL DEFAULT 0,
                    [Title] nvarchar(max) NOT NULL,
                    [Description] nvarchar(max) NULL,
                    [Location] nvarchar(max) NULL,
                    [StartAt] datetime2 NOT NULL,
                    [EndAt] datetime2 NOT NULL,
                    [Status] int NOT NULL,
                    [OrganizerId] uniqueidentifier NOT NULL,
                    [ProjectId] uniqueidentifier NULL,
                    CONSTRAINT [PK_Meetings] PRIMARY KEY ([Id]),
                    CONSTRAINT [FK_Meetings_AspNetUsers_OrganizerId] FOREIGN KEY ([OrganizerId]) REFERENCES [AspNetUsers] ([Id]),
                    CONSTRAINT [FK_Meetings_Projects_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES [Projects] ([Id])
                );
            END

            IF OBJECT_ID(N'[MeetingAttendees]', N'U') IS NULL
            BEGIN
                CREATE TABLE [MeetingAttendees] (
                    [MeetingId] uniqueidentifier NOT NULL,
                    [UserId] uniqueidentifier NOT NULL,
                    [Role] int NOT NULL,
                    [InvitedAt] datetime2 NOT NULL,
                    CONSTRAINT [PK_MeetingAttendees] PRIMARY KEY ([MeetingId], [UserId]),
                    CONSTRAINT [FK_MeetingAttendees_Meetings_MeetingId] FOREIGN KEY ([MeetingId]) REFERENCES [Meetings] ([Id]) ON DELETE CASCADE,
                    CONSTRAINT [FK_MeetingAttendees_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
                );
            END
            """, cancellationToken);

        _logger.LogInformation("Meetings schema applied");
    }

    private static async Task<bool> TableExistsAsync(ApplicationDbContext context, string tableName, CancellationToken cancellationToken)
    {
        try
        {
            var connection = context.Database.GetDbConnection();
            if (connection.State != System.Data.ConnectionState.Open)
                await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = $"SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{tableName}'";
            var result = await command.ExecuteScalarAsync(cancellationToken);
            return result != null;
        }
        catch
        {
            return false;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private static async Task SeedRolesAsync(RoleManager<ApplicationRole> roleManager)
    {
        (string Name, string Description)[] roles =
        [
            ("Admin", "Full system access"),
            ("Manager", "Manage projects, tasks, and view users/roles"),
            ("Member", "Standard team member with task access"),
            ("Viewer", "Read-only access to projects and reports"),
            ("ProjectLead", "Lead projects and coordinate team deliverables"),
            ("Developer", "Build and maintain application features"),
            ("QA", "Quality assurance and testing"),
            ("Finance", "Budget tracking and financial reporting"),
            ("SupportAgent", "Customer support and ticketing workflows"),
            ("DevOps", "Infrastructure, CI/CD, and deployment"),
            ("Client", "External client stakeholder"),
        ];

        foreach (var (name, description) in roles)
        {
            if (!await roleManager.RoleExistsAsync(name))
                await roleManager.CreateAsync(new ApplicationRole { Name = name, Description = description });
        }
    }

    private static async Task SeedPermissionsAsync(ApplicationDbContext context)
    {
        var permissions = new[]
        {
            ("projects.view", "Projects"), ("projects.create", "Projects"), ("projects.edit", "Projects"), ("projects.delete", "Projects"),
            ("tasks.view", "Tasks"), ("tasks.create", "Tasks"), ("tasks.edit", "Tasks"), ("tasks.delete", "Tasks"),
            ("meetings.view", "Meetings"), ("meetings.create", "Meetings"), ("meetings.edit", "Meetings"), ("meetings.delete", "Meetings"),
            ("users.view", "Users"), ("users.manage", "Users"),
            ("roles.view", "Roles"), ("roles.manage", "Roles"),
            ("reports.view", "Reports"), ("audit.view", "Audit"), ("settings.manage", "Settings")
        };

        foreach (var (name, module) in permissions)
        {
            if (!await context.Permissions.AnyAsync(p => p.Name == name))
                context.Permissions.Add(new Permission { Name = name, Module = module });
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedRolePermissionsAsync(ApplicationDbContext context, RoleManager<ApplicationRole> roleManager)
    {
        var permissions = await context.Permissions.ToListAsync();
        if (permissions.Count == 0) return;

        var byName = permissions.ToDictionary(p => p.Name);

        async Task EnsureRolePermissions(string roleName, string[]? names = null)
        {
            var role = await roleManager.FindByNameAsync(roleName);
            if (role == null) return;

            if (await context.RolePermissions.AnyAsync(rp => rp.RoleId == role.Id))
                return;

            var toAssign = names == null
                ? permissions
                : names.Where(byName.ContainsKey).Select(n => byName[n]).ToList();

            foreach (var p in toAssign)
                context.RolePermissions.Add(new RolePermission { RoleId = role.Id, PermissionId = p.Id });
        }

        await EnsureRolePermissions("Admin");
        await EnsureRolePermissions("Manager",
        [
            "projects.view", "projects.create", "projects.edit", "projects.delete",
            "tasks.view", "tasks.create", "tasks.edit", "tasks.delete",
            "meetings.view", "meetings.create", "meetings.edit", "meetings.delete",
            "users.view", "roles.view", "reports.view", "audit.view"
        ]);
        await EnsureRolePermissions("Member",
            ["projects.view", "tasks.view", "tasks.create", "tasks.edit", "meetings.view", "reports.view"]);
        await EnsureRolePermissions("Viewer",
            ["projects.view", "tasks.view", "meetings.view", "reports.view", "audit.view"]);
        await EnsureRolePermissions("ProjectLead",
        [
            "projects.view", "projects.create", "projects.edit",
            "tasks.view", "tasks.create", "tasks.edit", "tasks.delete",
            "meetings.view", "meetings.create", "meetings.edit",
            "reports.view", "users.view"
        ]);
        await EnsureRolePermissions("Developer",
            ["projects.view", "tasks.view", "tasks.create", "tasks.edit", "meetings.view", "reports.view"]);
        await EnsureRolePermissions("QA",
            ["projects.view", "tasks.view", "tasks.edit", "reports.view", "audit.view"]);
        await EnsureRolePermissions("Finance",
            ["projects.view", "tasks.view", "reports.view", "audit.view"]);
        await EnsureRolePermissions("SupportAgent",
            ["projects.view", "tasks.view", "tasks.create", "tasks.edit", "reports.view"]);
        await EnsureRolePermissions("DevOps",
        [
            "projects.view", "projects.edit",
            "tasks.view", "tasks.create", "tasks.edit",
            "reports.view", "audit.view", "settings.manage"
        ]);
        await EnsureRolePermissions("Client",
            ["meetings.view"]);

        await context.SaveChangesAsync();
    }

    private static async Task EnsureMeetingPermissionsAsync(ApplicationDbContext context, RoleManager<ApplicationRole> roleManager)
    {
        var meetingPerms = await context.Permissions.Where(p => p.Name.StartsWith("meetings.")).ToListAsync();
        if (meetingPerms.Count == 0) return;

        var rolePermissionMap = new Dictionary<string, string[]>
        {
            ["Admin"] = meetingPerms.Select(p => p.Name).ToArray(),
            ["Manager"] = ["meetings.view", "meetings.create", "meetings.edit", "meetings.delete"],
            ["ProjectLead"] = ["meetings.view", "meetings.create", "meetings.edit"],
            ["Member"] = ["meetings.view"],
            ["Developer"] = ["meetings.view"],
            ["Viewer"] = ["meetings.view"],
            ["Client"] = ["meetings.view"],
        };

        var byName = meetingPerms.ToDictionary(p => p.Name);

        foreach (var (roleName, permNames) in rolePermissionMap)
        {
            var role = await roleManager.FindByNameAsync(roleName);
            if (role == null) continue;

            foreach (var permName in permNames)
            {
                if (!byName.TryGetValue(permName, out var permission)) continue;
                if (await context.RolePermissions.AnyAsync(rp => rp.RoleId == role.Id && rp.PermissionId == permission.Id))
                    continue;
                context.RolePermissions.Add(new RolePermission { RoleId = role.Id, PermissionId = permission.Id });
            }
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
    {
        const string email = "admin@nexuserp.com";
        if (await userManager.FindByEmailAsync(email) != null) return;

        var admin = new ApplicationUser
        {
            Email = email,
            UserName = email,
            FirstName = "System",
            LastName = "Admin",
            EmailConfirmed = true
        };

        await userManager.CreateAsync(admin, "Admin@123");
        await userManager.AddToRoleAsync(admin, "Admin");
    }

    private static async Task SeedSampleUsersAsync(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
    {
        (string Email, string FirstName, string LastName, string Role)[] users =
        [
            ("sarah.manager@nexuserp.com", "Sarah", "Chen", "Manager"),
            ("james.lead@nexuserp.com", "James", "Wilson", "ProjectLead"),
            ("emily.dev@nexuserp.com", "Emily", "Rodriguez", "Developer"),
            ("michael.dev@nexuserp.com", "Michael", "Park", "Developer"),
            ("lisa.qa@nexuserp.com", "Lisa", "Thompson", "QA"),
            ("david.viewer@nexuserp.com", "David", "Okonkwo", "Viewer"),
            ("anna.member@nexuserp.com", "Anna", "Kowalski", "Member"),
            ("robert.finance@nexuserp.com", "Robert", "Hughes", "Finance"),
            ("kate.support@nexuserp.com", "Kate", "Miller", "SupportAgent"),
            ("alex.devops@nexuserp.com", "Alex", "Nakamura", "DevOps"),
            ("priya.lead@nexuserp.com", "Priya", "Sharma", "ProjectLead"),
            ("tom.developer@nexuserp.com", "Tom", "Anderson", "Developer"),
            ("client.acme@external.com", "John", "Acheson", "Client"),
            ("client.globex@external.com", "Maria", "Santos", "Client"),
            ("client.initech@external.com", "Peter", "Gibbons", "Client"),
        ];

        const string defaultPassword = "User@123";

        foreach (var (email, firstName, lastName, role) in users)
        {
            if (await userManager.FindByEmailAsync(email) != null)
                continue;

            var user = new ApplicationUser
            {
                Email = email,
                UserName = email,
                FirstName = firstName,
                LastName = lastName,
                EmailConfirmed = true,
                IsActive = true,
                LastLoginAt = DateTime.UtcNow.AddDays(-Random.Shared.Next(1, 30))
            };

            var result = await userManager.CreateAsync(user, defaultPassword);
            if (!result.Succeeded) continue;

            if (await roleManager.RoleExistsAsync(role))
                await userManager.AddToRoleAsync(user, role);
        }
    }

    private static async Task SeedProjectsAsync(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        CancellationToken cancellationToken)
    {
        var sampleProjects = new[]
        {
            new Project
            {
                Name = "GoGreen Sustainability Portal",
                Code = "PRJ-GGR-001",
                Description = "Internal portal to track carbon footprint, waste reduction, and ESG reporting across departments.",
                Status = ProjectStatus.Active,
                Budget = 125000,
                StartDate = new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc),
                EndDate = new DateTime(2026, 9, 30, 0, 0, 0, DateTimeKind.Utc),
                CreatedBy = "system"
            },
            new Project
            {
                Name = "Nexus Mobile App v2",
                Code = "PRJ-MOB-002",
                Description = "Redesign and rebuild the mobile app with offline sync, push notifications, and improved UX.",
                Status = ProjectStatus.Planning,
                Budget = 210000,
                StartDate = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc),
                EndDate = new DateTime(2026, 12, 15, 0, 0, 0, DateTimeKind.Utc),
                CreatedBy = "system"
            },
            new Project
            {
                Name = "ERP Data Migration",
                Code = "PRJ-MIG-003",
                Description = "Migrate legacy finance and inventory data into NexusERP with validation, reconciliation, and rollback plan.",
                Status = ProjectStatus.Active,
                Budget = 89000,
                StartDate = new DateTime(2026, 2, 10, 0, 0, 0, DateTimeKind.Utc),
                EndDate = new DateTime(2026, 6, 30, 0, 0, 0, DateTimeKind.Utc),
                CreatedBy = "system"
            },
            new Project
            {
                Name = "Customer Support Automation",
                Code = "PRJ-CSA-004",
                Description = "Implement ticketing workflows, SLA tracking, knowledge base, and chatbot integration for support teams.",
                Status = ProjectStatus.OnHold,
                Budget = 56000,
                StartDate = new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc),
                EndDate = new DateTime(2026, 10, 31, 0, 0, 0, DateTimeKind.Utc),
                CreatedBy = "system"
            },
            new Project
            {
                Name = "Q4 Security Hardening",
                Code = "PRJ-SEC-005",
                Description = "Penetration testing remediation, MFA rollout, audit log enhancements, and role-permission review.",
                Status = ProjectStatus.Planning,
                Budget = 45000,
                StartDate = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc),
                EndDate = new DateTime(2026, 9, 30, 0, 0, 0, DateTimeKind.Utc),
                CreatedBy = "system"
            },
            new Project
            {
                Name = "HR Onboarding Portal",
                Code = "PRJ-HR-006",
                Description = "Digital onboarding workflows, document signing, and new-hire checklist automation.",
                Status = ProjectStatus.Active,
                Budget = 72000,
                StartDate = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc),
                EndDate = new DateTime(2026, 8, 31, 0, 0, 0, DateTimeKind.Utc),
                CreatedBy = "system"
            },
            new Project
            {
                Name = "Business Analytics Platform",
                Code = "PRJ-ANL-007",
                Description = "Self-service BI dashboards, KPI scorecards, and executive reporting suite.",
                Status = ProjectStatus.Active,
                Budget = 185000,
                StartDate = new DateTime(2026, 1, 20, 0, 0, 0, DateTimeKind.Utc),
                EndDate = new DateTime(2026, 11, 30, 0, 0, 0, DateTimeKind.Utc),
                CreatedBy = "system"
            },
            new Project
            {
                Name = "Public API Gateway",
                Code = "PRJ-API-008",
                Description = "REST/GraphQL gateway with rate limiting, API keys, and partner developer portal.",
                Status = ProjectStatus.Planning,
                Budget = 95000,
                StartDate = new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc),
                EndDate = new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc),
                CreatedBy = "system"
            },
            new Project
            {
                Name = "Employee Training LMS",
                Code = "PRJ-TRN-009",
                Description = "Learning management system with course catalog, certifications, and progress tracking.",
                Status = ProjectStatus.Active,
                Budget = 68000,
                StartDate = new DateTime(2026, 3, 15, 0, 0, 0, DateTimeKind.Utc),
                EndDate = new DateTime(2026, 9, 15, 0, 0, 0, DateTimeKind.Utc),
                CreatedBy = "system"
            },
            new Project
            {
                Name = "Cloud Infrastructure Upgrade",
                Code = "PRJ-INF-010",
                Description = "Migrate workloads to Kubernetes, implement auto-scaling, and improve observability.",
                Status = ProjectStatus.Active,
                Budget = 142000,
                StartDate = new DateTime(2026, 2, 20, 0, 0, 0, DateTimeKind.Utc),
                EndDate = new DateTime(2026, 7, 31, 0, 0, 0, DateTimeKind.Utc),
                CreatedBy = "system"
            },
            new Project
            {
                Name = "CRM Integration Hub",
                Code = "PRJ-CRM-011",
                Description = "Sync leads, contacts, and deals between NexusERP and Salesforce/HubSpot.",
                Status = ProjectStatus.Planning,
                Budget = 78000,
                StartDate = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc),
                EndDate = new DateTime(2026, 11, 15, 0, 0, 0, DateTimeKind.Utc),
                CreatedBy = "system"
            },
            new Project
            {
                Name = "Corporate Website Redesign",
                Code = "PRJ-WEB-012",
                Description = "Modern marketing site with CMS, SEO optimization, and multilingual support.",
                Status = ProjectStatus.Completed,
                Budget = 52000,
                StartDate = new DateTime(2025, 10, 1, 0, 0, 0, DateTimeKind.Utc),
                EndDate = new DateTime(2026, 3, 31, 0, 0, 0, DateTimeKind.Utc),
                CreatedBy = "system"
            },
            new Project
            {
                Name = "Warehouse Management System",
                Code = "PRJ-WMS-013",
                Description = "Barcode scanning, pick-pack-ship workflows, and real-time inventory tracking.",
                Status = ProjectStatus.Active,
                Budget = 198000,
                StartDate = new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc),
                EndDate = new DateTime(2027, 2, 28, 0, 0, 0, DateTimeKind.Utc),
                CreatedBy = "system"
            },
            new Project
            {
                Name = "Vendor Portal",
                Code = "PRJ-VND-014",
                Description = "Self-service portal for suppliers to submit invoices, POs, and shipment notices.",
                Status = ProjectStatus.OnHold,
                Budget = 64000,
                StartDate = new DateTime(2026, 5, 15, 0, 0, 0, DateTimeKind.Utc),
                EndDate = new DateTime(2026, 12, 20, 0, 0, 0, DateTimeKind.Utc),
                CreatedBy = "system"
            }
        };

        var managers = await userManager.Users
            .Where(u => u.Email != null)
            .ToDictionaryAsync(u => u.Email!, cancellationToken);

        Guid? ManagerFor(params string[] emails)
        {
            foreach (var email in emails)
                if (managers.TryGetValue(email, out var user)) return user.Id;
            return managers.GetValueOrDefault("admin@nexuserp.com")?.Id;
        }
        var existingCodes = await context.Projects
            .Select(p => p.Code)
            .ToListAsync(cancellationToken);

        var added = 0;
        foreach (var project in sampleProjects)
        {
            if (existingCodes.Contains(project.Code))
                continue;

            project.ManagerId = project.Code switch
            {
                "PRJ-GGR-001" or "PRJ-ANL-007" => ManagerFor("sarah.manager@nexuserp.com"),
                "PRJ-MOB-002" or "PRJ-API-008" => ManagerFor("james.lead@nexuserp.com", "priya.lead@nexuserp.com"),
                "PRJ-MIG-003" or "PRJ-INF-010" => ManagerFor("alex.devops@nexuserp.com"),
                "PRJ-CSA-004" or "PRJ-VND-014" => ManagerFor("kate.support@nexuserp.com"),
                "PRJ-SEC-005" => ManagerFor("alex.devops@nexuserp.com", "sarah.manager@nexuserp.com"),
                "PRJ-HR-006" or "PRJ-TRN-009" => ManagerFor("priya.lead@nexuserp.com"),
                "PRJ-CRM-011" or "PRJ-WEB-012" => ManagerFor("james.lead@nexuserp.com"),
                "PRJ-WMS-013" => ManagerFor("sarah.manager@nexuserp.com"),
                _ => ManagerFor("admin@nexuserp.com")
            };
            context.Projects.Add(project);
            added++;
        }

        if (added > 0)
            await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedTasksAsync(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        CancellationToken cancellationToken)
    {
        var projects = await context.Projects
            .Where(p => p.Code.StartsWith("PRJ-"))
            .ToDictionaryAsync(p => p.Code, cancellationToken);

        if (projects.Count == 0)
            return;

        var usersByEmail = await userManager.Users
            .Where(u => u.Email != null)
            .ToDictionaryAsync(u => u.Email!, cancellationToken);

        Guid? Assignee(params string[] emails)
        {
            foreach (var email in emails)
                if (usersByEmail.TryGetValue(email, out var user)) return user.Id;
            return usersByEmail.GetValueOrDefault("admin@nexuserp.com")?.Id;
        }

        var existingTasks = await context.Tasks
            .Where(t => t.Tags == "seed")
            .Select(t => new { t.ProjectId, t.Title })
            .ToListAsync(cancellationToken);
        var existingSet = existingTasks.Select(t => (t.ProjectId, t.Title)).ToHashSet();

        var tasks = new List<TaskItem>();

        void Add(string projectCode, string title, string? description, TaskStatus status,
            TaskPriority priority, int order, DateTime? due, decimal? estHours,
            decimal? actualHours = null, params string[] assigneeEmails)
        {
            if (!projects.TryGetValue(projectCode, out var project))
                return;
            if (existingSet.Contains((project.Id, title)))
                return;

            tasks.Add(new TaskItem
            {
                Title = title,
                Description = description,
                Status = status,
                Priority = priority,
                Order = order,
                DueDate = due,
                StartDate = due?.AddDays(-7),
                EstimatedHours = estHours,
                ActualHours = actualHours,
                ProjectId = project.Id,
                AssigneeId = Assignee(assigneeEmails),
                Tags = "seed",
                CreatedBy = "system"
            });
        }

        // GoGreen
        Add("PRJ-GGR-001", "Define ESG KPI framework", "Document carbon, waste, and energy KPIs.", TaskStatus.Done, TaskPriority.High, 0, new DateTime(2026, 2, 15, 0, 0, 0, DateTimeKind.Utc), 24, 22, "sarah.manager@nexuserp.com");
        Add("PRJ-GGR-001", "Build emissions dashboard", "Charts and filters for department-level emissions.", TaskStatus.InProgress, TaskPriority.High, 1, new DateTime(2026, 4, 30, 0, 0, 0, DateTimeKind.Utc), 40, 18, "emily.dev@nexuserp.com");
        Add("PRJ-GGR-001", "Integrate utility data feeds", "Connect electricity and water consumption APIs.", TaskStatus.Todo, TaskPriority.Medium, 2, new DateTime(2026, 5, 20, 0, 0, 0, DateTimeKind.Utc), 32, assigneeEmails: "michael.dev@nexuserp.com");
        Add("PRJ-GGR-001", "ESG report export module", "PDF/Excel export for quarterly ESG reports.", TaskStatus.InReview, TaskPriority.Medium, 3, new DateTime(2026, 6, 10, 0, 0, 0, DateTimeKind.Utc), 20, 16, "emily.dev@nexuserp.com");
        Add("PRJ-GGR-001", "Stakeholder review sessions", "Present ESG metrics to leadership board.", TaskStatus.Todo, TaskPriority.Low, 4, new DateTime(2026, 7, 5, 0, 0, 0, DateTimeKind.Utc), 8, assigneeEmails: "sarah.manager@nexuserp.com");

        // Mobile App
        Add("PRJ-MOB-002", "UX wireframes for v2", "Low/high fidelity wireframes for core flows.", TaskStatus.Done, TaskPriority.High, 0, new DateTime(2026, 3, 20, 0, 0, 0, DateTimeKind.Utc), 30, 28, "james.lead@nexuserp.com");
        Add("PRJ-MOB-002", "Offline sync architecture", "Design local cache and conflict resolution.", TaskStatus.InProgress, TaskPriority.Critical, 1, new DateTime(2026, 5, 15, 0, 0, 0, DateTimeKind.Utc), 48, assigneeEmails: "michael.dev@nexuserp.com");
        Add("PRJ-MOB-002", "Push notification service", "FCM/APNs integration and preference center.", TaskStatus.Todo, TaskPriority.High, 2, new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc), 36, assigneeEmails: "emily.dev@nexuserp.com");
        Add("PRJ-MOB-002", "Beta release checklist", "QA sign-off and app store submission prep.", TaskStatus.Todo, TaskPriority.Medium, 3, new DateTime(2026, 11, 30, 0, 0, 0, DateTimeKind.Utc), 16, assigneeEmails: "lisa.qa@nexuserp.com");
        Add("PRJ-MOB-002", "Performance profiling", "Profile startup time and memory on low-end devices.", TaskStatus.Todo, TaskPriority.High, 4, new DateTime(2026, 8, 15, 0, 0, 0, DateTimeKind.Utc), 24, assigneeEmails: "tom.developer@nexuserp.com");
        Add("PRJ-MOB-002", "Accessibility audit", "WCAG 2.1 AA compliance review for mobile screens.", TaskStatus.InReview, TaskPriority.Medium, 5, new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc), 16, 12, "lisa.qa@nexuserp.com");

        // Data Migration
        Add("PRJ-MIG-003", "Inventory data mapping", "Map legacy SKU and warehouse fields.", TaskStatus.Done, TaskPriority.High, 0, new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc), 20, 19, "robert.finance@nexuserp.com");
        Add("PRJ-MIG-003", "Finance ledger migration script", "ETL for GL accounts and journal entries.", TaskStatus.InProgress, TaskPriority.Critical, 1, new DateTime(2026, 4, 15, 0, 0, 0, DateTimeKind.Utc), 56, 30, "alex.devops@nexuserp.com");
        Add("PRJ-MIG-003", "Reconciliation report", "Compare source vs target totals.", TaskStatus.InReview, TaskPriority.High, 2, new DateTime(2026, 5, 30, 0, 0, 0, DateTimeKind.Utc), 24, 10, "robert.finance@nexuserp.com");
        Add("PRJ-MIG-003", "Rollback procedure test", "Validate rollback in staging environment.", TaskStatus.Todo, TaskPriority.Medium, 3, new DateTime(2026, 6, 15, 0, 0, 0, DateTimeKind.Utc), 12, assigneeEmails: "alex.devops@nexuserp.com");
        Add("PRJ-MIG-003", "Data quality rules engine", "Define validation rules for migrated records.", TaskStatus.Todo, TaskPriority.High, 4, new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc), 28, assigneeEmails: "anna.member@nexuserp.com");

        // Customer Support
        Add("PRJ-CSA-004", "SLA policy configuration", "Define priority tiers and escalation rules.", TaskStatus.Todo, TaskPriority.Medium, 0, new DateTime(2026, 5, 10, 0, 0, 0, DateTimeKind.Utc), 16, assigneeEmails: "kate.support@nexuserp.com");
        Add("PRJ-CSA-004", "Knowledge base MVP", "Article editor, categories, and search.", TaskStatus.Todo, TaskPriority.Low, 1, new DateTime(2026, 6, 30, 0, 0, 0, DateTimeKind.Utc), 40, assigneeEmails: "kate.support@nexuserp.com");
        Add("PRJ-CSA-004", "Chatbot intent training", "Train NLP model on top 50 support topics.", TaskStatus.InProgress, TaskPriority.Medium, 2, new DateTime(2026, 6, 15, 0, 0, 0, DateTimeKind.Utc), 32, 14, "emily.dev@nexuserp.com");

        // Security
        Add("PRJ-SEC-005", "MFA rollout plan", "Phased MFA enforcement for all users.", TaskStatus.Todo, TaskPriority.Critical, 0, new DateTime(2026, 7, 15, 0, 0, 0, DateTimeKind.Utc), 20, assigneeEmails: "alex.devops@nexuserp.com");
        Add("PRJ-SEC-005", "Pen test remediation batch 1", "Fix critical and high findings from Q3 pen test.", TaskStatus.Todo, TaskPriority.Critical, 1, new DateTime(2026, 8, 1, 0, 0, 0, DateTimeKind.Utc), 32, assigneeEmails: "alex.devops@nexuserp.com");
        Add("PRJ-SEC-005", "Permission matrix review", "Audit role-permission assignments.", TaskStatus.Todo, TaskPriority.High, 2, new DateTime(2026, 8, 20, 0, 0, 0, DateTimeKind.Utc), 16, assigneeEmails: "sarah.manager@nexuserp.com");
        Add("PRJ-SEC-005", "Secrets rotation automation", "Automate API key and certificate rotation.", TaskStatus.InProgress, TaskPriority.High, 3, new DateTime(2026, 7, 30, 0, 0, 0, DateTimeKind.Utc), 24, 8, "alex.devops@nexuserp.com");

        // HR Onboarding
        Add("PRJ-HR-006", "Onboarding workflow design", "Map hire-to-day-30 journey and approvals.", TaskStatus.Done, TaskPriority.High, 0, new DateTime(2026, 3, 10, 0, 0, 0, DateTimeKind.Utc), 20, 18, "priya.lead@nexuserp.com");
        Add("PRJ-HR-006", "E-signature integration", "DocuSign/Adobe Sign for offer letters.", TaskStatus.InProgress, TaskPriority.High, 1, new DateTime(2026, 5, 20, 0, 0, 0, DateTimeKind.Utc), 36, 20, "emily.dev@nexuserp.com");
        Add("PRJ-HR-006", "New-hire checklist UI", "Task checklist for HR and new employees.", TaskStatus.Todo, TaskPriority.Medium, 2, new DateTime(2026, 6, 30, 0, 0, 0, DateTimeKind.Utc), 28, assigneeEmails: "tom.developer@nexuserp.com");
        Add("PRJ-HR-006", "IT provisioning hooks", "Auto-create accounts in AD, Slack, and email.", TaskStatus.Todo, TaskPriority.High, 3, new DateTime(2026, 7, 15, 0, 0, 0, DateTimeKind.Utc), 40, assigneeEmails: "alex.devops@nexuserp.com");

        // Analytics
        Add("PRJ-ANL-007", "Data warehouse schema", "Star schema for sales, ops, and finance.", TaskStatus.Done, TaskPriority.Critical, 0, new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc), 48, 45, "robert.finance@nexuserp.com");
        Add("PRJ-ANL-007", "Executive dashboard v1", "CEO/CFO KPI dashboard with drill-down.", TaskStatus.InProgress, TaskPriority.High, 1, new DateTime(2026, 6, 30, 0, 0, 0, DateTimeKind.Utc), 56, 30, "emily.dev@nexuserp.com");
        Add("PRJ-ANL-007", "Self-service report builder", "Drag-and-drop report designer for analysts.", TaskStatus.Todo, TaskPriority.High, 2, new DateTime(2026, 9, 30, 0, 0, 0, DateTimeKind.Utc), 64, assigneeEmails: "michael.dev@nexuserp.com");
        Add("PRJ-ANL-007", "ETL pipeline monitoring", "Alerts for failed data sync jobs.", TaskStatus.Todo, TaskPriority.Medium, 3, new DateTime(2026, 8, 1, 0, 0, 0, DateTimeKind.Utc), 20, assigneeEmails: "alex.devops@nexuserp.com");
        Add("PRJ-ANL-007", "UAT with finance team", "Validate report accuracy with sample GL data.", TaskStatus.InReview, TaskPriority.High, 4, new DateTime(2026, 7, 20, 0, 0, 0, DateTimeKind.Utc), 16, 10, "robert.finance@nexuserp.com");

        // API Gateway
        Add("PRJ-API-008", "API design standards doc", "OpenAPI conventions and versioning policy.", TaskStatus.Done, TaskPriority.High, 0, new DateTime(2026, 5, 15, 0, 0, 0, DateTimeKind.Utc), 16, 14, "james.lead@nexuserp.com");
        Add("PRJ-API-008", "Rate limiting middleware", "Per-key and per-IP throttling rules.", TaskStatus.InProgress, TaskPriority.Critical, 1, new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc), 40, 22, "alex.devops@nexuserp.com");
        Add("PRJ-API-008", "Developer portal MVP", "API docs, sandbox keys, and usage analytics.", TaskStatus.Todo, TaskPriority.High, 2, new DateTime(2026, 9, 15, 0, 0, 0, DateTimeKind.Utc), 48, assigneeEmails: "tom.developer@nexuserp.com");
        Add("PRJ-API-008", "OAuth2 client credentials", "Machine-to-machine auth for partners.", TaskStatus.Todo, TaskPriority.High, 3, new DateTime(2026, 8, 30, 0, 0, 0, DateTimeKind.Utc), 32, assigneeEmails: "michael.dev@nexuserp.com");

        // Training LMS
        Add("PRJ-TRN-009", "Course catalog structure", "Categories, prerequisites, and learning paths.", TaskStatus.Done, TaskPriority.Medium, 0, new DateTime(2026, 4, 15, 0, 0, 0, DateTimeKind.Utc), 20, 18, "priya.lead@nexuserp.com");
        Add("PRJ-TRN-009", "Video player integration", "HLS streaming with progress bookmarking.", TaskStatus.InProgress, TaskPriority.High, 1, new DateTime(2026, 6, 30, 0, 0, 0, DateTimeKind.Utc), 36, 20, "tom.developer@nexuserp.com");
        Add("PRJ-TRN-009", "Certification tracking", "Issue and verify completion certificates.", TaskStatus.Todo, TaskPriority.Medium, 2, new DateTime(2026, 8, 15, 0, 0, 0, DateTimeKind.Utc), 24, assigneeEmails: "anna.member@nexuserp.com");
        Add("PRJ-TRN-009", "Manager progress reports", "Team completion rates and overdue courses.", TaskStatus.Todo, TaskPriority.Low, 3, new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc), 16, assigneeEmails: "sarah.manager@nexuserp.com");

        // Infrastructure
        Add("PRJ-INF-010", "Kubernetes cluster setup", "EKS/AKS cluster with node pools and RBAC.", TaskStatus.Done, TaskPriority.Critical, 0, new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc), 56, 52, "alex.devops@nexuserp.com");
        Add("PRJ-INF-010", "CI/CD pipeline migration", "GitHub Actions to deploy all services.", TaskStatus.InProgress, TaskPriority.High, 1, new DateTime(2026, 6, 15, 0, 0, 0, DateTimeKind.Utc), 40, 28, "alex.devops@nexuserp.com");
        Add("PRJ-INF-010", "Observability stack", "Prometheus, Grafana, and distributed tracing.", TaskStatus.InProgress, TaskPriority.High, 2, new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc), 48, 24, "alex.devops@nexuserp.com");
        Add("PRJ-INF-010", "Disaster recovery drill", "Test failover to secondary region.", TaskStatus.Todo, TaskPriority.Critical, 3, new DateTime(2026, 7, 31, 0, 0, 0, DateTimeKind.Utc), 24, assigneeEmails: "alex.devops@nexuserp.com");
        Add("PRJ-INF-010", "Cost optimization review", "Right-size instances and reserved capacity.", TaskStatus.Todo, TaskPriority.Medium, 4, new DateTime(2026, 8, 15, 0, 0, 0, DateTimeKind.Utc), 16, assigneeEmails: "robert.finance@nexuserp.com");

        // CRM Integration
        Add("PRJ-CRM-011", "Salesforce connector", "Bidirectional sync for accounts and opportunities.", TaskStatus.Todo, TaskPriority.High, 0, new DateTime(2026, 7, 30, 0, 0, 0, DateTimeKind.Utc), 48, assigneeEmails: "michael.dev@nexuserp.com");
        Add("PRJ-CRM-011", "HubSpot webhook handler", "Real-time lead capture from marketing forms.", TaskStatus.Todo, TaskPriority.Medium, 1, new DateTime(2026, 8, 20, 0, 0, 0, DateTimeKind.Utc), 32, assigneeEmails: "emily.dev@nexuserp.com");
        Add("PRJ-CRM-011", "Duplicate detection rules", "Merge logic for contacts across systems.", TaskStatus.Todo, TaskPriority.Medium, 2, new DateTime(2026, 9, 10, 0, 0, 0, DateTimeKind.Utc), 24, assigneeEmails: "anna.member@nexuserp.com");

        // Website
        Add("PRJ-WEB-012", "Design system components", "Reusable UI kit for marketing pages.", TaskStatus.Done, TaskPriority.High, 0, new DateTime(2026, 1, 31, 0, 0, 0, DateTimeKind.Utc), 40, 38, "james.lead@nexuserp.com");
        Add("PRJ-WEB-012", "CMS integration", "Headless CMS for blog and landing pages.", TaskStatus.Done, TaskPriority.High, 1, new DateTime(2026, 2, 28, 0, 0, 0, DateTimeKind.Utc), 32, 30, "tom.developer@nexuserp.com");
        Add("PRJ-WEB-012", "SEO audit and fixes", "Meta tags, sitemap, and Core Web Vitals.", TaskStatus.Done, TaskPriority.Medium, 2, new DateTime(2026, 3, 15, 0, 0, 0, DateTimeKind.Utc), 20, 19, "lisa.qa@nexuserp.com");
        Add("PRJ-WEB-012", "Multilingual content rollout", "EN, FR, DE translations for top 20 pages.", TaskStatus.Done, TaskPriority.Low, 3, new DateTime(2026, 3, 31, 0, 0, 0, DateTimeKind.Utc), 24, 22, "anna.member@nexuserp.com");

        // WMS
        Add("PRJ-WMS-013", "Barcode scanner SDK integration", "Support Zebra and Honeywell handheld devices.", TaskStatus.InProgress, TaskPriority.Critical, 0, new DateTime(2026, 6, 30, 0, 0, 0, DateTimeKind.Utc), 48, 20, "michael.dev@nexuserp.com");
        Add("PRJ-WMS-013", "Pick-pack-ship workflow", "Wave picking, packing slips, and carrier labels.", TaskStatus.Todo, TaskPriority.High, 1, new DateTime(2026, 8, 31, 0, 0, 0, DateTimeKind.Utc), 56, assigneeEmails: "tom.developer@nexuserp.com");
        Add("PRJ-WMS-013", "Real-time inventory sync", "Stock levels updated on every scan event.", TaskStatus.Todo, TaskPriority.Critical, 2, new DateTime(2026, 9, 30, 0, 0, 0, DateTimeKind.Utc), 40, assigneeEmails: "emily.dev@nexuserp.com");
        Add("PRJ-WMS-013", "Warehouse floor UAT", "Pilot with Distribution Center East.", TaskStatus.Todo, TaskPriority.High, 3, new DateTime(2026, 11, 15, 0, 0, 0, DateTimeKind.Utc), 32, assigneeEmails: "lisa.qa@nexuserp.com");
        Add("PRJ-WMS-013", "Cycle count module", "Scheduled and ad-hoc inventory counts.", TaskStatus.Todo, TaskPriority.Medium, 4, new DateTime(2026, 10, 31, 0, 0, 0, DateTimeKind.Utc), 28, assigneeEmails: "anna.member@nexuserp.com");

        // Vendor Portal
        Add("PRJ-VND-014", "Vendor registration flow", "Self-service signup with approval workflow.", TaskStatus.Todo, TaskPriority.Medium, 0, new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc), 24, assigneeEmails: "kate.support@nexuserp.com");
        Add("PRJ-VND-014", "Invoice submission portal", "Upload invoices and track payment status.", TaskStatus.Todo, TaskPriority.High, 1, new DateTime(2026, 8, 15, 0, 0, 0, DateTimeKind.Utc), 36, assigneeEmails: "robert.finance@nexuserp.com");
        Add("PRJ-VND-014", "PO acknowledgment module", "Vendors confirm or reject purchase orders.", TaskStatus.Todo, TaskPriority.Medium, 2, new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc), 20, assigneeEmails: "anna.member@nexuserp.com");

        if (tasks.Count == 0)
            return;

        context.Tasks.AddRange(tasks);
        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedAuditLogsAsync(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        CancellationToken cancellationToken)
    {
        if (await context.AuditLogs.AnyAsync(cancellationToken))
            return;

        var admin = await userManager.FindByEmailAsync("admin@nexuserp.com");
        var userId = admin?.Id.ToString() ?? "system";
        var userName = admin?.FullName ?? "System Admin";

        var projects = await context.Projects.OrderBy(p => p.CreatedAt).Take(5).ToListAsync(cancellationToken);
        var tasks = await context.Tasks.OrderBy(t => t.CreatedAt).Take(5).ToListAsync(cancellationToken);

        var logs = new List<AuditLog>
        {
            new()
            {
                UserId = userId,
                UserName = userName,
                Action = AuditAction.Login,
                EntityType = "Auth",
                EntityId = userId,
                NewValues = """{"event":"login","method":"password"}""",
                IpAddress = "127.0.0.1",
                UserAgent = "NexusERP-Seed",
                CreatedBy = "system"
            },
            new()
            {
                UserId = userId,
                UserName = userName,
                Action = AuditAction.Export,
                EntityType = "Report",
                EntityId = "projects-summary",
                NewValues = """{"format":"xlsx","report":"projects-summary"}""",
                IpAddress = "127.0.0.1",
                UserAgent = "NexusERP-Seed",
                CreatedBy = "system"
            }
        };

        foreach (var project in projects)
        {
            logs.Add(new AuditLog
            {
                UserId = userId,
                UserName = userName,
                Action = AuditAction.Create,
                EntityType = nameof(Project),
                EntityId = project.Id.ToString(),
                NewValues = $$"""{"name":"{{project.Name}}","code":"{{project.Code}}"}""",
                IpAddress = "127.0.0.1",
                UserAgent = "NexusERP-Seed",
                CreatedBy = "system",
                CreatedAt = project.CreatedAt
            });
        }

        foreach (var task in tasks)
        {
            logs.Add(new AuditLog
            {
                UserId = userId,
                UserName = userName,
                Action = AuditAction.Create,
                EntityType = nameof(TaskItem),
                EntityId = task.Id.ToString(),
                NewValues = $$"""{"title":"{{task.Title}}","status":"{{task.Status}}"}""",
                IpAddress = "127.0.0.1",
                UserAgent = "NexusERP-Seed",
                CreatedBy = "system",
                CreatedAt = task.CreatedAt
            });
        }

        logs.Add(new AuditLog
        {
            UserId = userId,
            UserName = userName,
            Action = AuditAction.Update,
            EntityType = nameof(TaskItem),
            EntityId = tasks.FirstOrDefault()?.Id.ToString(),
            OldValues = """{"status":"Todo"}""",
            NewValues = """{"status":"InProgress"}""",
            IpAddress = "127.0.0.1",
            UserAgent = "NexusERP-Seed",
            CreatedBy = "system"
        });

        logs.Add(new AuditLog
        {
            UserId = userId,
            UserName = userName,
            Action = AuditAction.Export,
            EntityType = "Report",
            EntityId = "tasks-by-status",
            NewValues = """{"format":"pdf","report":"tasks-by-status"}""",
            IpAddress = "127.0.0.1",
            UserAgent = "NexusERP-Seed",
            CreatedBy = "system"
        });

        context.AuditLogs.AddRange(logs);
        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedNotificationsAsync(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        CancellationToken cancellationToken)
    {
        if (await context.Notifications.AnyAsync(n => n.CreatedBy == "seed", cancellationToken))
            return;

        var usersByEmail = await userManager.Users
            .Where(u => u.Email != null)
            .ToDictionaryAsync(u => u.Email!, cancellationToken);

        Guid? UserId(string email) => usersByEmail.GetValueOrDefault(email)?.Id;

        var notifications = new List<Notification>();
        var now = DateTime.UtcNow;

        void Add(string email, string title, string message, NotificationType type,
            bool isRead, string? actionUrl = null, int daysAgo = 0)
        {
            var userId = UserId(email);
            if (userId == null) return;

            notifications.Add(new Notification
            {
                UserId = userId.Value,
                Title = title,
                Message = message,
                Type = type,
                IsRead = isRead,
                ActionUrl = actionUrl,
                CreatedBy = "seed",
                CreatedAt = now.AddDays(-daysAgo).AddHours(-Random.Shared.Next(1, 12))
            });
        }

        // Admin — mix of unread and read
        Add("admin@nexuserp.com", "Task assigned to you",
            "Emily Rodriguez assigned you \"Build emissions dashboard\" on GoGreen Sustainability Portal.",
            NotificationType.TaskAssigned, false, "/tasks", 0);
        Add("admin@nexuserp.com", "Project status updated",
            "ERP Data Migration moved to Active. Review the updated timeline and budget.",
            NotificationType.ProjectUpdated, false, "/projects", 0);
        Add("admin@nexuserp.com", "Security review required",
            "Q4 Security Hardening has 3 critical tasks due this week. Action needed.",
            NotificationType.Warning, false, "/tasks", 1);
        Add("admin@nexuserp.com", "New user registered",
            "Tom Anderson was added as Developer. Review role assignments in Users.",
            NotificationType.Info, false, "/users", 1);
        Add("admin@nexuserp.com", "Report export completed",
            "Your projects summary Excel export is ready for download.",
            NotificationType.Success, true, "/reports", 2);
        Add("admin@nexuserp.com", "Comment on task",
            "Lisa Thompson commented on \"Finance ledger migration script\".",
            NotificationType.CommentAdded, false, "/tasks", 2);
        Add("admin@nexuserp.com", "SLA breach warning",
            "Customer Support Automation has tasks past due date.",
            NotificationType.Error, false, "/tasks", 3);
        Add("admin@nexuserp.com", "Welcome to NexusERP",
            "Your admin account is configured. Explore projects, tasks, and reports.",
            NotificationType.Info, true, "/dashboard", 7);

        // Manager
        Add("sarah.manager@nexuserp.com", "Budget threshold reached",
            "Business Analytics Platform has consumed 78% of allocated budget.",
            NotificationType.Warning, false, "/projects", 0);
        Add("sarah.manager@nexuserp.com", "Task completed",
            "James Wilson marked \"UX wireframes for v2\" as Done.",
            NotificationType.TaskUpdated, false, "/tasks", 1);
        Add("sarah.manager@nexuserp.com", "Weekly digest",
            "5 projects active, 12 tasks in progress across your teams.",
            NotificationType.Info, true, "/dashboard", 4);

        // Developer
        Add("emily.dev@nexuserp.com", "Task assigned to you",
            "You were assigned \"Integrate utility data feeds\" on GoGreen Sustainability Portal.",
            NotificationType.TaskAssigned, false, "/tasks", 0);
        Add("emily.dev@nexuserp.com", "Code review requested",
            "Michael Park requested review on offline sync architecture changes.",
            NotificationType.Info, false, "/tasks", 1);
        Add("emily.dev@nexuserp.com", "Build pipeline passed",
            "CI/CD pipeline for Nexus Mobile App v2 completed successfully.",
            NotificationType.Success, true, "/projects", 3);

        // QA
        Add("lisa.qa@nexuserp.com", "Test cycle started",
            "Beta release checklist is ready for QA on Nexus Mobile App v2.",
            NotificationType.TaskAssigned, false, "/tasks", 0);
        Add("lisa.qa@nexuserp.com", "Bug reported",
            "Accessibility audit found 2 WCAG violations on mobile login screen.",
            NotificationType.Error, false, "/tasks", 1);

        // Support
        Add("kate.support@nexuserp.com", "Ticket escalated",
            "High-priority support ticket #1042 requires manager attention.",
            NotificationType.Warning, false, "/tasks", 0);

        if (notifications.Count == 0)
            return;

        context.Notifications.AddRange(notifications);
        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedMeetingsAsync(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        CancellationToken cancellationToken)
    {
        if (await context.Meetings.AnyAsync(cancellationToken)) return;

        var admin = await userManager.FindByEmailAsync("admin@nexuserp.com");
        var manager = await userManager.FindByEmailAsync("sarah.manager@nexuserp.com");
        var dev = await userManager.FindByEmailAsync("emily.dev@nexuserp.com");
        var client = await userManager.FindByEmailAsync("client.acme@external.com");
        if (admin == null || manager == null) return;

        var project = await context.Projects.OrderBy(p => p.CreatedAt).FirstOrDefaultAsync(cancellationToken);

        var meetings = new List<Meeting>
        {
            new()
            {
                Title = "Q3 Product Roadmap Review",
                Description = "Quarterly review with managers and developers to align on deliverables.",
                Location = "Conference Room A / Teams",
                StartAt = DateTime.UtcNow.AddDays(3).Date.AddHours(10),
                EndAt = DateTime.UtcNow.AddDays(3).Date.AddHours(11).AddMinutes(30),
                Status = MeetingStatus.Scheduled,
                OrganizerId = admin.Id,
                ProjectId = project?.Id,
                CreatedBy = "system"
            },
            new()
            {
                Title = "Client Demo — GoGreen Portal",
                Description = "Walkthrough of sustainability dashboard for Acme Corp stakeholders.",
                Location = "https://teams.microsoft.com/meet/demo",
                StartAt = DateTime.UtcNow.AddDays(7).Date.AddHours(14),
                EndAt = DateTime.UtcNow.AddDays(7).Date.AddHours(15),
                Status = MeetingStatus.Scheduled,
                OrganizerId = admin.Id,
                ProjectId = project?.Id,
                CreatedBy = "system"
            },
            new()
            {
                Title = "Sprint Planning — Mobile v2",
                Description = "Plan next sprint tasks with dev team and project lead.",
                Location = "Room 204",
                StartAt = DateTime.UtcNow.AddDays(-2).Date.AddHours(9),
                EndAt = DateTime.UtcNow.AddDays(-2).Date.AddHours(10),
                Status = MeetingStatus.Completed,
                OrganizerId = manager.Id,
                CreatedBy = "system"
            }
        };

        context.Meetings.AddRange(meetings);
        await context.SaveChangesAsync(cancellationToken);

        var roadmap = meetings[0];
        roadmap.Attendees.Add(new MeetingAttendee { MeetingId = roadmap.Id, UserId = manager.Id, Role = MeetingAttendeeRole.Required });
        if (dev != null)
            roadmap.Attendees.Add(new MeetingAttendee { MeetingId = roadmap.Id, UserId = dev.Id, Role = MeetingAttendeeRole.Required });

        var clientDemo = meetings[1];
        clientDemo.Attendees.Add(new MeetingAttendee { MeetingId = clientDemo.Id, UserId = manager.Id, Role = MeetingAttendeeRole.Required });
        if (dev != null)
            clientDemo.Attendees.Add(new MeetingAttendee { MeetingId = clientDemo.Id, UserId = dev.Id, Role = MeetingAttendeeRole.Optional });
        if (client != null)
            clientDemo.Attendees.Add(new MeetingAttendee { MeetingId = clientDemo.Id, UserId = client.Id, Role = MeetingAttendeeRole.Required });

        var sprint = meetings[2];
        sprint.Attendees.Add(new MeetingAttendee { MeetingId = sprint.Id, UserId = admin.Id, Role = MeetingAttendeeRole.Optional });
        if (dev != null)
            sprint.Attendees.Add(new MeetingAttendee { MeetingId = sprint.Id, UserId = dev.Id, Role = MeetingAttendeeRole.Required });

        await context.SaveChangesAsync(cancellationToken);
    }
}

public class TokenCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TokenCleanupService> _logger;

    public TokenCleanupService(IServiceProvider serviceProvider, ILogger<TokenCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var expired = await context.RefreshTokens
                    .Where(t => t.ExpiresAt < DateTime.UtcNow || t.RevokedAt != null)
                    .Where(t => t.ExpiresAt < DateTime.UtcNow.AddDays(-30))
                    .ToListAsync(stoppingToken);

                if (expired.Count > 0)
                {
                    context.RefreshTokens.RemoveRange(expired);
                    await context.SaveChangesAsync(stoppingToken);
                    _logger.LogInformation("Cleaned up {Count} expired refresh tokens", expired.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token cleanup");
            }

            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
        }
    }
}
