using caa_mis.Models;
using caa_mis.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace caa_mis.Data
{
    public class InventoryContext : DbContext
    {
        public InventoryContext(DbContextOptions<InventoryContext> options)
            : base(options)
        {
        }
        public DbSet<TransactionStatus> TransactionStatuses { get; set; }
        public DbSet<TransactionType> TransactionTypes { get; set; }

        public DbSet<Bulk> Bulks { get; set; }
        public DbSet<BulkItem> BulkItems { get; set; }

        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<TransactionItem> TransactionItems { get; set; }

        public DbSet<Event> Events { get; set; }
        public DbSet<EventItem> EventItems { get; set; }
        public DbSet<Stock> Stocks { get; set; }
        public DbSet<Branch> Branches { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<ItemSupplier> ItemSuppliers { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<ItemStatus> ItemStatuses { get; set; }
        public DbSet<Manufacturer> Manufacturers { get; set; }
        public DbSet<ItemPhoto> ItemPhotos { get; set; }
        public DbSet<ItemThumbnail> ItemThumbnails { get; set; }
        public DbSet<StockSummaryByBranchVM> StockSummaryByBranch { get; set; }
        public DbSet<EventSummaryVM> EventSummary { get; set; }
        public DbSet<TransactionItemSummaryVM> TransactionItemSummary { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }

        public DbSet<ProductListVM> ProductList { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Item>()
                .HasIndex(s => s.SKUNumber)
                .IsUnique();

            //Cascade delete not applied yet for testing

            //For the AppointmentReasonSummaries View
            modelBuilder
                .Entity<StockSummaryByBranchVM>()
                .ToView(nameof(StockSummaryByBranch))
                .HasKey(a => a.ID);

            //Add a unique index to the Employee Email
            modelBuilder.Entity<Employee>()
            .HasIndex(a => new { a.Email })
            .IsUnique();

            modelBuilder
                .Entity<EventSummaryVM>()
                .ToView(nameof(EventSummary))
                .HasKey(a => a.ID);

            modelBuilder
                .Entity<ProductListVM>()
                .ToView(nameof(ProductList))
                .HasKey(a => a.ID);

            modelBuilder
                .Entity<TransactionItemSummaryVM>()
                .ToView(nameof(TransactionItemSummary))
                .HasKey(a => a.ID);

            modelBuilder.Entity<TransactionStatus>()
                .HasMany<Bulk>(t => t.Bulks)
                .WithOne(b => b.TransactionStatus)
                .HasForeignKey(b => b.TransactionStatusID);

            modelBuilder.Entity<TransactionStatus>()
               .HasMany<Event>(t => t.Events)
               .WithOne(b => b.TransactionStatus)
               .HasForeignKey(b => b.TransactionStatusID);

            modelBuilder.Entity<TransactionStatus>()
                .HasMany<Transaction>(ts => ts.Transactions)
                .WithOne(t => t.TransactionStatus)
                .HasForeignKey(t => t.TransactionStatusID);

            modelBuilder.Entity<TransactionType>()
                .HasMany<Transaction>(tt => tt.Transactions)
                .WithOne(t => t.TransactionType)
                .HasForeignKey(t => t.TransactionTypeID);

            modelBuilder.Entity<Employee>()
                .HasMany<Bulk>(e => e.Bulks)
                .WithOne(b => b.Employee)
                .HasForeignKey(b => b.EmployeeID);

            modelBuilder.Entity<Employee>()
                .HasMany<Event>(e => e.Events)
                .WithOne(b => b.Employee)
                .HasForeignKey(b => b.EmployeeID);

            modelBuilder.Entity<Employee>()
                .HasMany<Transaction>(e => e.Transactions)
                .WithOne(t => t.Employee)
                .HasForeignKey(t => t.EmployeeID);

            modelBuilder.Entity<Branch>()
                .HasMany<Bulk>(b => b.Bulks)
                .WithOne(t => t.Branch)
                .HasForeignKey(t => t.BranchID);

            modelBuilder.Entity<Branch>()
                .HasMany<Event>(b => b.Events)
                .WithOne(t => t.Branch)
                .HasForeignKey(t => t.BranchID);

            modelBuilder.Entity<Branch>()
                .HasMany<Transaction>(b => b.Origins)
                .WithOne(t => t.Origin)
                .HasForeignKey(t => t.OriginID);

            modelBuilder.Entity<Branch>()
               .HasMany<Transaction>(b => b.Destinations)
               .WithOne(t => t.Destination)
               .HasForeignKey(t => t.DestinationID);

            modelBuilder.Entity<Branch>()
                .HasMany<Stock>(b => b.Stocks)
                .WithOne(s => s.Branch)
                .HasForeignKey(s => s.BranchID);

            modelBuilder.Entity<Bulk>()
                .HasMany<BulkItem>(b => b.BulkItems)
                .WithOne(bi => bi.Bulk)
                .HasForeignKey(bi => bi.BulkID);

            modelBuilder.Entity<Event>()
               .HasMany<EventItem>(b => b.EventItems)
               .WithOne(bi => bi.Event)
               .HasForeignKey(bi => bi.EventID);

            modelBuilder.Entity<Transaction>()
                .HasMany<TransactionItem>(t => t.TransactionItems)
                .WithOne(ti => ti.Transaction)
                .HasForeignKey(ti => ti.TransactionID);

            modelBuilder.Entity<Item>()
                .HasMany<TransactionItem>(t => t.TransactionItems)
                .WithOne(ti => ti.Item)
                .HasForeignKey(ti => ti.ItemID);

            modelBuilder.Entity<Item>()
                .HasMany<BulkItem>(i => i.BulkItems)
                .WithOne(bi => bi.Item)
                .HasForeignKey(bi => bi.ItemID);

            modelBuilder.Entity<Item>()
               .HasMany<EventItem>(i => i.EventItems)
               .WithOne(bi => bi.Item)
               .HasForeignKey(bi => bi.ItemID);

            modelBuilder.Entity<Item>()
                .HasMany<Stock>(i => i.Stocks)
                .WithOne(s => s.Item)
                .HasForeignKey(s => s.ItemID);

            //Item-ItemSupplier-Suppliert M:M
            modelBuilder.Entity<ItemSupplier>()
                .HasKey(i => new { i.SupplierID, i.ItemID });

            modelBuilder.Entity<ItemSupplier>()
                .HasOne(i => i.Supplier)
                .WithMany(s => s.ItemSuppliers)
                .HasForeignKey(i => i.SupplierID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ItemSupplier>()
                .HasOne(i => i.Item)
                .WithMany(s => s.ItemSuppliers)
                .HasForeignKey(i => i.ItemID);


            modelBuilder.Entity<Category>()
                .HasMany<Item>(c => c.Items)
                .WithOne(i => i.Category)
                .HasForeignKey(i => i.CategoryID);

            modelBuilder.Entity<ItemStatus>()
                .HasMany<Item>(s => s.Items)
                .WithOne(i => i.ItemStatus)
                .HasForeignKey(i => i.ItemStatusID);

            modelBuilder.Entity<Manufacturer>()
                .HasMany<Item>(s => s.Items)
                .WithOne(i => i.Manufacturer)
                .HasForeignKey(i => i.ManufacturerID);


            modelBuilder.Entity<Event>()
                .HasIndex(e => new { e.Name, e.Date })
                .IsUnique();


        }

    }
}
