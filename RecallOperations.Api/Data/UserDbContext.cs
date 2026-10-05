using Microsoft.EntityFrameworkCore;
using VehicleRecall.Shared.Models;

namespace RecallOperations.Api.Data;

public class UserDbContext : DbContext
{
    public UserDbContext(DbContextOptions<UserDbContext> options)
        : base(options)
    {
    }

    public DbSet<UserAccount> Users => Set<UserAccount>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<RecallCustomer> Customers => Set<RecallCustomer>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<RecallCustomerVehicle> CustomerVehicles => Set<RecallCustomerVehicle>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RecallCampaign> Campaigns => Set<RecallCampaign>();
    public DbSet<CampaignVehicle> CampaignVehicles => Set<CampaignVehicle>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserAccount>(entity =>
        {
            entity.ToTable("KS_RecallUsers", "dbo");
            entity.HasKey(user => user.UserId);
            entity.Property(user => user.IsActive).HasDefaultValue(true);
            entity.Property(user => user.CreatedAt).HasDefaultValueSql("getdate()");
            entity.HasIndex(user => user.Username).IsUnique();
            entity.HasIndex(user => user.CustomerId).IsUnique();
            entity.HasOne(user => user.Department)
                .WithMany(department => department.Users)
                .HasForeignKey(user => user.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(user => user.Customer)
                .WithOne(customer => customer.UserAccount)
                .HasForeignKey<UserAccount>(user => user.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.ToTable("KS_Departments", "dbo");
            entity.HasKey(department => department.DepartmentId);
            entity.Property(department => department.Name).HasMaxLength(100).IsRequired();
            entity.Property(department => department.Description).HasMaxLength(500).IsRequired();
            entity.HasIndex(department => department.Name).IsUnique();
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("KS_Roles", "dbo");
            entity.HasKey(role => role.RoleId);
            entity.Property(role => role.Name).HasMaxLength(50).IsRequired();
            entity.HasIndex(role => role.Name).IsUnique();
            entity.HasData(
                new Role { RoleId = 1, Name = "Customer" },
                new Role { RoleId = 2, Name = "Manager" },
                new Role { RoleId = 3, Name = "Technician" });
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.ToTable("KS_UserRoles", "dbo");
            entity.HasKey(userRole => new { userRole.UserId, userRole.RoleId });
            entity.HasOne(userRole => userRole.User)
                .WithMany(user => user.UserRoles)
                .HasForeignKey(userRole => userRole.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(userRole => userRole.Role)
                .WithMany(role => role.UserRoles)
                .HasForeignKey(userRole => userRole.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RecallCustomer>(entity =>
        {
            entity.ToTable("KS_RecallCustomers", "dbo", table =>
                table.HasTrigger("TR_KS_RecallCustomers_UpdatedAt"));
            entity.HasKey(customer => customer.CustomerId);
            entity.Property(customer => customer.Email).HasMaxLength(254).IsRequired();
            entity.Property(customer => customer.FullName).HasMaxLength(100).IsRequired();
            entity.Property(customer => customer.Address).HasMaxLength(250).HasColumnType("varchar(250)");
            entity.Property(customer => customer.City).HasMaxLength(50).HasColumnType("varchar(50)");
            entity.Property(customer => customer.State).HasMaxLength(50).HasColumnType("varchar(50)");
            entity.Property(customer => customer.PostalCode).HasMaxLength(10).HasColumnType("varchar(10)");
            entity.Property(customer => customer.UpdatedAt)
                .HasColumnType("datetime")
                .HasDefaultValueSql("(getdate())")
                .ValueGeneratedOnAddOrUpdate();
            entity.HasIndex(customer => customer.Email).IsUnique();
            entity.HasData(
                new RecallCustomer { CustomerId = 1, Email = "arjun.sharma@example.com", FullName = "Arjun Sharma" },
                new RecallCustomer { CustomerId = 2, Email = "priya.reddy@example.com", FullName = "Priya Reddy" },
                new RecallCustomer { CustomerId = 3, Email = "rahul.verma@example.com", FullName = "Rahul Verma" },
                new RecallCustomer { CustomerId = 4, Email = "sneha.patel@example.com", FullName = "Sneha Patel" },
                new RecallCustomer { CustomerId = 5, Email = "vikram.nair@example.com", FullName = "Vikram Nair" },
                new RecallCustomer { CustomerId = 6, Email = "ananya.rao@example.com", FullName = "Ananya Rao" },
                new RecallCustomer { CustomerId = 7, Email = "rohit.kumar@example.com", FullName = "Rohit Kumar" },
                new RecallCustomer { CustomerId = 8, Email = "meera.iyer@example.com", FullName = "Meera Iyer" },
                new RecallCustomer { CustomerId = 9, Email = "aditya.singh@example.com", FullName = "Aditya Singh" },
                new RecallCustomer { CustomerId = 10, Email = "kavya.menon@example.com", FullName = "Kavya Menon" });
        });

        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.ToTable("KS_Vehicles", "dbo");
            entity.HasKey(vehicle => vehicle.Vin);
            entity.Property(vehicle => vehicle.Vin).HasMaxLength(17);
            entity.Property(vehicle => vehicle.Make).HasMaxLength(80).IsRequired();
            entity.Property(vehicle => vehicle.Model).HasMaxLength(100).IsRequired();
            entity.Property(vehicle => vehicle.RecallStatus).HasMaxLength(50).IsRequired();
            entity.HasData(
                new Vehicle { Vin = "1HGCM82633A123456", Make = "Honda", Model = "Accord", Year = 2020, RecallStatus = "Open" },
                new Vehicle { Vin = "5YFBURHE5FP123457", Make = "Toyota", Model = "Corolla", Year = 2019, RecallStatus = "Closed" },
                new Vehicle { Vin = "1C4RJFAG5FC123458", Make = "Jeep", Model = "Grand Cherokee", Year = 2018, RecallStatus = "Open" },
                new Vehicle { Vin = "3FA6P0H76KR123459", Make = "Ford", Model = "Fusion", Year = 2021, RecallStatus = "No Recall" },
                new Vehicle { Vin = "JM1BL1SF8A1234560", Make = "Mazda", Model = "3", Year = 2022, RecallStatus = "Open" },
                new Vehicle { Vin = "WVWZZZ3CZWE123461", Make = "Volkswagen", Model = "Jetta", Year = 2017, RecallStatus = "Closed" },
                new Vehicle { Vin = "2T1BURHE7JC123462", Make = "Toyota", Model = "Camry", Year = 2020, RecallStatus = "No Recall" },
                new Vehicle { Vin = "1N4AL3AP9HC123463", Make = "Nissan", Model = "Altima", Year = 2019, RecallStatus = "Open" },
                new Vehicle { Vin = "SALWR2RV7JA123464", Make = "Land Rover", Model = "Range Rover", Year = 2021, RecallStatus = "Closed" },
                new Vehicle { Vin = "KMHD84LF5KU123465", Make = "Hyundai", Model = "Elantra", Year = 2022, RecallStatus = "No Recall" });
        });

        modelBuilder.Entity<RecallCustomerVehicle>(entity =>
        {
            entity.ToTable("KS_CustomerVehicles", "dbo");
            entity.HasKey(link => new { link.CustomerId, link.Vin });
            entity.HasOne(link => link.Customer)
                .WithMany(customer => customer.Vehicles)
                .HasForeignKey(link => link.CustomerId);
            entity.HasOne(link => link.Vehicle)
                .WithMany(vehicle => vehicle.CustomerVehicles)
                .HasForeignKey(link => link.Vin);
            entity.HasData(
                new RecallCustomerVehicle { CustomerId = 1, Vin = "1HGCM82633A123456" },
                new RecallCustomerVehicle { CustomerId = 2, Vin = "5YFBURHE5FP123457" },
                new RecallCustomerVehicle { CustomerId = 3, Vin = "1C4RJFAG5FC123458" },
                new RecallCustomerVehicle { CustomerId = 4, Vin = "3FA6P0H76KR123459" },
                new RecallCustomerVehicle { CustomerId = 5, Vin = "JM1BL1SF8A1234560" },
                new RecallCustomerVehicle { CustomerId = 6, Vin = "WVWZZZ3CZWE123461" },
                new RecallCustomerVehicle { CustomerId = 7, Vin = "2T1BURHE7JC123462" },
                new RecallCustomerVehicle { CustomerId = 8, Vin = "1N4AL3AP9HC123463" },
                new RecallCustomerVehicle { CustomerId = 9, Vin = "SALWR2RV7JA123464" },
                new RecallCustomerVehicle { CustomerId = 10, Vin = "KMHD84LF5KU123465" });
        });

        modelBuilder.Entity<RecallCampaign>(entity =>
        {
            entity.ToTable("KS_Campaigns", "dbo");
            entity.HasKey(c => c.NhtsaId);
            entity.Property(c => c.NhtsaId).HasMaxLength(50).IsRequired();
            entity.Property(c => c.Description).HasMaxLength(500).IsRequired();
            entity.Property(c => c.AffectedComponent).HasMaxLength(100).IsRequired();
            entity.Property(c => c.RemedyInstructions).HasMaxLength(1000);
            entity.Property(c => c.Severity).HasMaxLength(50).IsRequired();
            entity.Property(c => c.Status).HasMaxLength(50).IsRequired();
            entity.Property(c => c.CreatedAt).HasDefaultValueSql("getdate()");
            entity.Ignore(c => c.Users);
            entity.Ignore(c => c.ImportErrors);
        });

        modelBuilder.Entity<CampaignVehicle>(entity =>
        {
            entity.ToTable("KS_CampaignVehicles", "dbo");
            entity.HasKey(cv => new { cv.CampaignId, cv.Vin });
            entity.Property(cv => cv.CampaignId).HasMaxLength(50);
            entity.Property(cv => cv.Vin).HasMaxLength(17);
            entity.HasOne(cv => cv.Campaign)
                .WithMany(c => c.CampaignVehicles)
                .HasForeignKey(cv => cv.CampaignId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(cv => cv.Vehicle)
                .WithMany(v => v.CampaignVehicles)
                .HasForeignKey(cv => cv.Vin)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}