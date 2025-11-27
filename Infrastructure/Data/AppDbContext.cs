using Domain.Entities;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Data
{
    public class  AppDbContext : IdentityDbContext<Admin>
    {
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
        public DbSet<PurchaseOrderItem> PurchaseOrderItems { get; set; }
        public DbSet<Signature> Signatures { get; set; }


        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ==== Seed Admins ====
            var hasher = new PasswordHasher<Admin>();

            var admin1 = new Admin
            {
                Id = "A1",
                UserName = "marwanosama",
                NormalizedUserName = "MARWANOSAMA",
                Email = "marwan@example.com",
                NormalizedEmail = "MARWAN@EXAMPLE.COM",
                FirstName = "مروان",
                LastName = "أسامة",
                JopTitle = "Manager",
                phone = "01202044132",
                EmailConfirmed = true,
                Createtime = DateTime.UtcNow
            };
            admin1.PasswordHash = hasher.HashPassword(admin1, "Admin@123");

            var admin2 = new Admin
            {
                Id = "A2",
                UserName = "ahmedhareedy",
                NormalizedUserName = "AHMEDHAREEDY",
                Email = "ahmed@example.com",
                NormalizedEmail = "AHMED@EXAMPLE.COM",
                JopTitle= "Director",
                FirstName = "أحمد",
                LastName = "هريدي",
                phone = "01094184590",
                EmailConfirmed = true,
                Createtime = DateTime.UtcNow
            };
            admin2.PasswordHash = hasher.HashPassword(admin2, "Admin@123");


            modelBuilder.Entity<Admin>().HasData(admin1, admin2);

            // ==== Seed Signatures ====
            modelBuilder.Entity<Signature>().HasData(
      new Signature
      {
          Id = 1,
          SignaturePath = "/images/marwanS.png",
          AdminId = "A1",
          SignedDate = DateTime.UtcNow
      },
      new Signature
      {
          Id = 2,
          SignaturePath = "/images/AhmedS.png",
          AdminId = "A2",
          SignedDate = DateTime.UtcNow
      }
  );


            // ==== Seed Suppliers (كما كتبت) ====
            modelBuilder.Entity<Supplier>().HasData(
                new Supplier { Id = 1, Name = "المورد الأول" },
                new Supplier { Id = 2, Name = "المورد الثاني" },
                new Supplier { Id = 3, Name = "المورد الثالث" }
            );

            modelBuilder.Entity<Supplier>()
                .HasMany(s => s.PurchaseOrders)
                .WithOne(p => p.Supplier)
                .HasForeignKey(p => p.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PurchaseOrder>()
                .HasMany(p => p.Items)
                .WithOne(i => i.PurchaseOrder)
                .HasForeignKey(i => i.PurchaseOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PurchaseOrder>()
       .HasOne(p => p.Signature)
       .WithMany(s => s.PurchaseOrders)
       .HasForeignKey(p => p.SignatureId)
       .OnDelete(DeleteBehavior.SetNull); // إذا تم حذف التوقيع

            modelBuilder.Entity<Admin>()
                .HasOne(a => a.Signature)
                .WithOne()
                .HasForeignKey<Signature>(s => s.AdminId)
                .OnDelete(DeleteBehavior.Cascade);

        }
    }


    
}
