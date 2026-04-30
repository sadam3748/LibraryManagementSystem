using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Book> Books { get; set; }

        public DbSet<BorrowTransaction> BorrowTransactions { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<BorrowTransaction>()
                .HasOne(bt => bt.Book)
                .WithMany()
                .HasForeignKey(bt => bt.BookId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<BorrowTransaction>()
                .HasOne(bt => bt.ApplicationUser)
                .WithMany()
                .HasForeignKey(bt => bt.ApplicationUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}