using System.Reflection;
using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Domain.Common;
using ECommercePlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommercePlatform.Infrastructure.Persistence;

/// <summary>
/// The EF Core context. It implements <see cref="IUnitOfWork"/> so the Application
/// layer can commit a use case without knowing EF Core exists.
/// </summary>
public sealed class ApplicationDbContext : DbContext, IUnitOfWork
{
    private readonly TimeProvider _timeProvider;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, TimeProvider timeProvider)
        : base(options)
        => _timeProvider = timeProvider;

    public DbSet<User> Users => Set<User>();

    public DbSet<UserSession> UserSessions => Set<UserSession>();

    public DbSet<Category> Categories => Set<Category>();

    public DbSet<SubCategory> SubCategories => Set<SubCategory>();

    public DbSet<CategoryImage> CategoryImages => Set<CategoryImage>();

    public DbSet<Product> Products => Set<Product>();

    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<BrandImage> BrandImages => Set<BrandImage>();
    public DbSet<Store> Stores  => Set<Store>();

    public DbSet<StoreImage> StoreImages => Set<StoreImage>();

    public DbSet<TaxRule> TaxRules => Set<TaxRule>();

    public DbSet<DeliveryRule> DeliveryRules => Set<DeliveryRule>();
    public DbSet<Cart> Carts  => Set<Cart>();

    public DbSet<CartItem> CartItems => Set<CartItem>();

    public DbSet<WishlistItem> WishlistItems  => Set<WishlistItem>();

    public DbSet<SavedItem> SavedItems => Set<SavedItem>();
    public DbSet<Coupon> Coupons  => Set<Coupon>();
    public DbSet<Order> Orders   => Set<Order>();

    public DbSet<OrderItem> OrderItems  => Set<OrderItem>();

    public DbSet<ProductImage> ProductImages  => Set<ProductImage>();

    public DbSet<Warehouse> Warehouses  => Set<Warehouse>();

    public DbSet<Inventory> Inventory => Set<Inventory>();

    public DbSet<InventoryMovement> InventoryMovements  => Set<InventoryMovement>();

    public DbSet<WholesalePriceTier> WholesalePriceTiers => Set<WholesalePriceTier>();
    public DbSet<SubCategoryImage> SubCategoryImages  => Set<SubCategoryImage>();

    public DbSet<OrderAddress> OrderAddresses => Set<OrderAddress>();

    public DbSet<OrderStatusHistory> OrderStatusHistory  => Set<OrderStatusHistory>();

    public DbSet<Invoice> Invoices  => Set<Invoice>();

    public DbSet<Payment> Payments   => Set<Payment>();

    public DbSet<Refund> Refunds => Set<Refund>();

    public DbSet<SavedPaymentMethod> SavedPaymentMethods  => Set<SavedPaymentMethod>();
    public DbSet<Shipment> Shipments  => Set<Shipment>();

    public DbSet<DeliveryZone> DeliveryZones  => Set<DeliveryZone>();

    public DbSet<DeliveryPartner> DeliveryPartners => Set<DeliveryPartner>();

    public DbSet<DeliveryAssignment> DeliveryAssignments  => Set<DeliveryAssignment>();

    public DbSet<Agent> Agents => Set<Agent>();

    public DbSet<Dealer> Dealers => Set<Dealer>();

    public DbSet<BusinessAccount> BusinessAccounts  => Set<BusinessAccount>();

    public DbSet<Address> Addresses => Set<Address>();

    public DbSet<Role> Roles => Set<Role>();

    public DbSet<Permission> Permissions  => Set<Permission>();

    public DbSet<RolePermission> RolePermissions  => Set<RolePermission>();

    public DbSet<AdminUserRole> AdminUserRoles => Set<AdminUserRole>();

    public DbSet<AgentAssignment> AgentAssignments  => Set<AgentAssignment>();

    public DbSet<ShopAssignment> ShopAssignments  => Set<ShopAssignment>();

    public DbSet<AgentCommission> AgentCommissions => Set<AgentCommission>();

    public DbSet<AgentCommissionEarning> AgentCommissionEarnings => Set<AgentCommissionEarning>();

    public DbSet<ShopLedger> ShopLedgers => Set<ShopLedger>();

    // ---- Admin CMS tables (banners, offers, requests, ...). Previously served
    // from in-memory stores; now persisted so the admin UI reads SQL Server. ----

    public DbSet<Banner> Banners => Set<Banner>();

    public DbSet<Offer> Offers => Set<Offer>();

    public DbSet<Combo> Combos => Set<Combo>();

    public DbSet<CartRule> CartRules => Set<CartRule>();

    public DbSet<StoreOffer> StoreOffers => Set<StoreOffer>();

    public DbSet<Quote> Quotes => Set<Quote>();

    public DbSet<Review> Reviews => Set<Review>();

    public DbSet<Enquiry> Enquiries => Set<Enquiry>();

    public DbSet<PricingRequest> PricingRequests => Set<PricingRequest>();

    public DbSet<Notification> Notifications => Set<Notification>();

    public DbSet<AppSetting> AppSettings => Set<AppSetting>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        StampAuditColumns();

        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        StampAuditColumns();

        return base.SaveChanges();
    }

    /// <summary>
    /// Maintains CreatedAt/UpdatedAt centrally. Doing this here rather than in each
    /// handler means a forgotten assignment cannot leave a row with default
    /// timestamps.
    /// </summary>
    private void StampAuditColumns()
    {
        var now = _timeProvider.GetUtcNow();

        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    entry.Entity.UpdatedAt = now;
                    break;

                case EntityState.Modified:
                    // Never let a client rewrite history.
                    entry.Property(e => e.CreatedAt).IsModified = false;
                    entry.Entity.UpdatedAt = now;
                    break;
            }
        }
    }
}
