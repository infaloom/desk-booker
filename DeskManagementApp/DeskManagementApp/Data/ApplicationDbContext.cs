using DeskManagementApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;

namespace DeskManagementApp.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options )
            : base(options)
        {
        }

        public DbSet<Desk> Desks { get; set; }
        public DbSet<Reservation> Reservations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Reservation>()
                .HasIndex(msq => new { msq.Date, msq.DeskId, msq.Type }).IsUnique(true);
            modelBuilder.Entity<Reservation>()
                .HasIndex(msq => new { msq.Date, msq.UserId, msq.Type }).IsUnique(true);
        }
    }
}