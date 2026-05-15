using System.Linq.Expressions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using mvmclean.backend.Domain.Aggregates.Booking;
using mvmclean.backend.Domain.Aggregates.Contact;
using mvmclean.backend.Domain.Aggregates.Contractor;
using mvmclean.backend.Domain.Aggregates.Invoice;
using mvmclean.backend.Domain.Aggregates.Promotion;
using mvmclean.backend.Domain.Aggregates.SeoPage;
using mvmclean.backend.Domain.Aggregates.Service;
using mvmclean.backend.Domain.Aggregates.WhatsAppChat;
using mvmclean.backend.Domain.Core.BaseClasses;
using mvmclean.backend.Domain.Core.Interfaces;

namespace mvmclean.backend.Infrastructure.Persistence;

public class MVMdbContext : DbContext
{
    private readonly IMediator _mediator;
    public MVMdbContext(DbContextOptions<MVMdbContext> options, IMediator mediator) : base(options)
    {
        _mediator = mediator;
    }

    public MVMdbContext(IMediator mediator) : base()
    {
        _mediator = mediator;
    }

    public DbSet<Contractor> Contractors { get; set; } = null!;
    public DbSet<Booking> Bookings { get; set; } = null!;
    public DbSet<Invoice> Invoices { get; set; } = null!;
    public DbSet<SeoPage> SeoPages { get; set; } = null!;
    public DbSet<Service> Services { get; set; } = null!;
    public DbSet<Promotion> Promotions { get; set; } = null!;
    public DbSet<Contact> Contacts { get; set; } = null!;
    public DbSet<WhatsAppChat> WhatsAppChats { get; set; } = null!;


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
    
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MVMdbContext).Assembly);
    
        ApplyGlobalQueryFilters(modelBuilder);
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

    }
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<Enum>()
            .HaveConversion<string>();
    }



    private void ApplyGlobalQueryFilters(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (entityType.IsOwned())
                continue;
            
            if (typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var isDeletedProperty = Expression.Property(parameter, "IsDeleted");
                var notExpression = Expression.Not(isDeletedProperty);
                var lambda = Expression.Lambda(notExpression, parameter);

                entityType.SetQueryFilter(lambda);
            }
        }

    }
    
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var domainEvents = ChangeTracker
            .Entries<AggregateRoot>()
            .SelectMany(e => e.Entity.DomainEvents)
            .ToList();

        var result = await base.SaveChangesAsync(cancellationToken);

        foreach (var domainEvent in domainEvents)
        {
            await _mediator.Publish(domainEvent, cancellationToken);
        }

        foreach (var entry in ChangeTracker.Entries<AggregateRoot>())
        {
            entry.Entity.ClearDomainEvents();
        }

        return result;
    }

    
}