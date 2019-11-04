﻿// <auto-generated />
using System;
using HES.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace HES.Infrastructure.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.6-servicing-10079")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("HES.Core.Entities.ApplicationUser", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("AccessFailedCount");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<string>("DeviceId");

                    b.Property<string>("Email")
                        .HasMaxLength(256);

                    b.Property<bool>("EmailConfirmed");

                    b.Property<string>("FirstName");

                    b.Property<string>("LastName");

                    b.Property<bool>("LockoutEnabled");

                    b.Property<DateTimeOffset?>("LockoutEnd");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256);

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256);

                    b.Property<string>("PasswordHash");

                    b.Property<string>("PhoneNumber");

                    b.Property<bool>("PhoneNumberConfirmed");

                    b.Property<string>("SecurityStamp");

                    b.Property<bool>("TwoFactorEnabled");

                    b.Property<string>("UserName")
                        .HasMaxLength(256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasName("UserNameIndex");

                    b.ToTable("AspNetUsers");
                });

            modelBuilder.Entity("HES.Core.Entities.Company", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name")
                        .IsRequired();

                    b.HasKey("Id");

                    b.ToTable("Companies");
                });

            modelBuilder.Entity("HES.Core.Entities.DataProtection", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Value");

                    b.HasKey("Id");

                    b.ToTable("DataProtection");
                });

            modelBuilder.Entity("HES.Core.Entities.Department", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("CompanyId")
                        .IsRequired();

                    b.Property<string>("Name")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("CompanyId");

                    b.ToTable("Departments");
                });

            modelBuilder.Entity("HES.Core.Entities.Device", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("AcceessProfileId");

                    b.Property<int>("Battery");

                    b.Property<string>("EmployeeId");

                    b.Property<string>("Firmware");

                    b.Property<DateTime>("ImportedAt");

                    b.Property<DateTime?>("LastSynced");

                    b.Property<string>("MAC");

                    b.Property<string>("MasterPassword");

                    b.Property<string>("Model");

                    b.Property<string>("PrimaryAccountId");

                    b.Property<string>("RFID");

                    b.Property<int>("State");

                    b.HasKey("Id");

                    b.HasIndex("AcceessProfileId");

                    b.HasIndex("EmployeeId");

                    b.ToTable("Devices");
                });

            modelBuilder.Entity("HES.Core.Entities.DeviceAccessProfile", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("ButtonBonding");

                    b.Property<bool>("ButtonConnection");

                    b.Property<bool>("ButtonNewChannel");

                    b.Property<DateTime>("CreatedAt");

                    b.Property<bool>("MasterKeyBonding");

                    b.Property<bool>("MasterKeyConnection");

                    b.Property<bool>("MasterKeyNewChannel");

                    b.Property<string>("Name")
                        .IsRequired();

                    b.Property<bool>("PinBonding");

                    b.Property<bool>("PinConnection");

                    b.Property<int>("PinExpiration");

                    b.Property<int>("PinLength");

                    b.Property<bool>("PinNewChannel");

                    b.Property<int>("PinTryCount");

                    b.Property<DateTime?>("UpdatedAt");

                    b.HasKey("Id");

                    b.ToTable("DeviceAccessProfiles");
                });

            modelBuilder.Entity("HES.Core.Entities.DeviceAccount", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Apps");

                    b.Property<DateTime>("CreatedAt");

                    b.Property<bool>("Deleted");

                    b.Property<string>("DeviceId");

                    b.Property<string>("EmployeeId");

                    b.Property<ushort>("IdFromDevice");

                    b.Property<int>("Kind");

                    b.Property<DateTime?>("LastSyncedAt");

                    b.Property<string>("Login")
                        .IsRequired();

                    b.Property<string>("Name")
                        .IsRequired();

                    b.Property<DateTime?>("OtpUpdatedAt");

                    b.Property<DateTime>("PasswordUpdatedAt");

                    b.Property<string>("SharedAccountId");

                    b.Property<int>("Status");

                    b.Property<int>("Type");

                    b.Property<DateTime?>("UpdatedAt");

                    b.Property<string>("Urls");

                    b.HasKey("Id");

                    b.HasIndex("DeviceId");

                    b.HasIndex("EmployeeId");

                    b.HasIndex("SharedAccountId");

                    b.ToTable("DeviceAccounts");
                });

            modelBuilder.Entity("HES.Core.Entities.DeviceTask", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Apps");

                    b.Property<DateTime>("CreatedAt");

                    b.Property<string>("DeviceAccountId");

                    b.Property<string>("DeviceId");

                    b.Property<string>("Login");

                    b.Property<string>("Name");

                    b.Property<int>("Operation");

                    b.Property<string>("OtpSecret");

                    b.Property<string>("Password");

                    b.Property<string>("Urls");

                    b.HasKey("Id");

                    b.HasIndex("DeviceAccountId");

                    b.ToTable("DeviceTasks");
                });

            modelBuilder.Entity("HES.Core.Entities.Employee", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("DepartmentId")
                        .IsRequired();

                    b.Property<string>("Email")
                        .IsRequired();

                    b.Property<string>("FirstName")
                        .IsRequired();

                    b.Property<string>("LastName")
                        .IsRequired();

                    b.Property<DateTime?>("LastSeen");

                    b.Property<string>("PhoneNumber");

                    b.Property<string>("PositionId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("DepartmentId");

                    b.HasIndex("PositionId");

                    b.ToTable("Employees");
                });

            modelBuilder.Entity("HES.Core.Entities.Position", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name")
                        .IsRequired();

                    b.HasKey("Id");

                    b.ToTable("Positions");
                });

            modelBuilder.Entity("HES.Core.Entities.ProximityDevice", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("DeviceId");

                    b.Property<int>("LockProximity");

                    b.Property<int>("LockTimeout");

                    b.Property<int>("UnlockProximity");

                    b.Property<string>("WorkstationId");

                    b.HasKey("Id");

                    b.HasIndex("DeviceId");

                    b.HasIndex("WorkstationId");

                    b.ToTable("ProximityDevices");
                });

            modelBuilder.Entity("HES.Core.Entities.SamlIdentityProvider", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("Enabled");

                    b.Property<string>("Url")
                        .IsRequired();

                    b.HasKey("Id");

                    b.ToTable("SamlIdentityProvider");
                });

            modelBuilder.Entity("HES.Core.Entities.SharedAccount", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Apps");

                    b.Property<bool>("Deleted");

                    b.Property<int>("Kind");

                    b.Property<string>("Login")
                        .IsRequired();

                    b.Property<string>("Name")
                        .IsRequired();

                    b.Property<string>("OtpSecret");

                    b.Property<DateTime?>("OtpSecretChangedAt");

                    b.Property<string>("Password");

                    b.Property<DateTime?>("PasswordChangedAt");

                    b.Property<string>("Urls");

                    b.HasKey("Id");

                    b.ToTable("SharedAccounts");
                });

            modelBuilder.Entity("HES.Core.Entities.Template", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Apps");

                    b.Property<bool>("Deleted");

                    b.Property<string>("Name")
                        .IsRequired();

                    b.Property<string>("Urls");

                    b.HasKey("Id");

                    b.ToTable("Templates");
                });

            modelBuilder.Entity("HES.Core.Entities.Workstation", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("Approved");

                    b.Property<string>("ClientVersion");

                    b.Property<string>("DepartmentId");

                    b.Property<string>("Domain");

                    b.Property<string>("IP");

                    b.Property<DateTime>("LastSeen");

                    b.Property<string>("Name");

                    b.Property<string>("OS");

                    b.Property<bool>("RFID");

                    b.HasKey("Id");

                    b.HasIndex("DepartmentId");

                    b.ToTable("Workstations");
                });

            modelBuilder.Entity("HES.Core.Entities.WorkstationEvent", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("Date");

                    b.Property<string>("DepartmentId");

                    b.Property<string>("DeviceAccountId");

                    b.Property<string>("DeviceId");

                    b.Property<string>("EmployeeId");

                    b.Property<int>("EventId");

                    b.Property<string>("Note");

                    b.Property<int>("SeverityId");

                    b.Property<string>("UserSession");

                    b.Property<string>("WorkstationId");

                    b.HasKey("Id");

                    b.HasIndex("DepartmentId");

                    b.HasIndex("DeviceAccountId");

                    b.HasIndex("DeviceId");

                    b.HasIndex("EmployeeId");

                    b.HasIndex("WorkstationId");

                    b.ToTable("WorkstationEvents");
                });

            modelBuilder.Entity("HES.Core.Entities.WorkstationSession", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("DepartmentId");

                    b.Property<string>("DeviceAccountId");

                    b.Property<string>("DeviceId");

                    b.Property<string>("EmployeeId");

                    b.Property<DateTime?>("EndDate");

                    b.Property<DateTime>("StartDate");

                    b.Property<int>("UnlockedBy");

                    b.Property<string>("UserSession");

                    b.Property<string>("WorkstationId");

                    b.HasKey("Id");

                    b.HasIndex("DepartmentId");

                    b.HasIndex("DeviceAccountId");

                    b.HasIndex("DeviceId");

                    b.HasIndex("EmployeeId");

                    b.HasIndex("WorkstationId");

                    b.ToTable("WorkstationSessions");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<string>("Name")
                        .HasMaxLength(256);

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasName("RoleNameIndex");

                    b.ToTable("AspNetRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<string>("RoleId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider");

                    b.Property<string>("ProviderKey");

                    b.Property<string>("ProviderDisplayName");

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId");

                    b.Property<string>("RoleId");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId");

                    b.Property<string>("LoginProvider");

                    b.Property<string>("Name");

                    b.Property<string>("Value");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens");
                });

            modelBuilder.Entity("HES.Core.Entities.Department", b =>
                {
                    b.HasOne("HES.Core.Entities.Company", "Company")
                        .WithMany()
                        .HasForeignKey("CompanyId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("HES.Core.Entities.Device", b =>
                {
                    b.HasOne("HES.Core.Entities.DeviceAccessProfile", "DeviceAccessProfile")
                        .WithMany("Devices")
                        .HasForeignKey("AcceessProfileId");

                    b.HasOne("HES.Core.Entities.Employee", "Employee")
                        .WithMany("Devices")
                        .HasForeignKey("EmployeeId");
                });

            modelBuilder.Entity("HES.Core.Entities.DeviceAccount", b =>
                {
                    b.HasOne("HES.Core.Entities.Device", "Device")
                        .WithMany()
                        .HasForeignKey("DeviceId");

                    b.HasOne("HES.Core.Entities.Employee", "Employee")
                        .WithMany()
                        .HasForeignKey("EmployeeId");

                    b.HasOne("HES.Core.Entities.SharedAccount", "SharedAccount")
                        .WithMany()
                        .HasForeignKey("SharedAccountId");
                });

            modelBuilder.Entity("HES.Core.Entities.DeviceTask", b =>
                {
                    b.HasOne("HES.Core.Entities.DeviceAccount", "DeviceAccount")
                        .WithMany()
                        .HasForeignKey("DeviceAccountId");
                });

            modelBuilder.Entity("HES.Core.Entities.Employee", b =>
                {
                    b.HasOne("HES.Core.Entities.Department", "Department")
                        .WithMany()
                        .HasForeignKey("DepartmentId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("HES.Core.Entities.Position", "Position")
                        .WithMany()
                        .HasForeignKey("PositionId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("HES.Core.Entities.ProximityDevice", b =>
                {
                    b.HasOne("HES.Core.Entities.Device", "Device")
                        .WithMany()
                        .HasForeignKey("DeviceId");

                    b.HasOne("HES.Core.Entities.Workstation", "Workstation")
                        .WithMany("ProximityDevices")
                        .HasForeignKey("WorkstationId");
                });

            modelBuilder.Entity("HES.Core.Entities.Workstation", b =>
                {
                    b.HasOne("HES.Core.Entities.Department", "Department")
                        .WithMany()
                        .HasForeignKey("DepartmentId");
                });

            modelBuilder.Entity("HES.Core.Entities.WorkstationEvent", b =>
                {
                    b.HasOne("HES.Core.Entities.Department", "Department")
                        .WithMany()
                        .HasForeignKey("DepartmentId");

                    b.HasOne("HES.Core.Entities.DeviceAccount", "DeviceAccount")
                        .WithMany()
                        .HasForeignKey("DeviceAccountId");

                    b.HasOne("HES.Core.Entities.Device", "Device")
                        .WithMany()
                        .HasForeignKey("DeviceId");

                    b.HasOne("HES.Core.Entities.Employee", "Employee")
                        .WithMany()
                        .HasForeignKey("EmployeeId");

                    b.HasOne("HES.Core.Entities.Workstation", "Workstation")
                        .WithMany()
                        .HasForeignKey("WorkstationId");
                });

            modelBuilder.Entity("HES.Core.Entities.WorkstationSession", b =>
                {
                    b.HasOne("HES.Core.Entities.Department", "Department")
                        .WithMany()
                        .HasForeignKey("DepartmentId");

                    b.HasOne("HES.Core.Entities.DeviceAccount", "DeviceAccount")
                        .WithMany()
                        .HasForeignKey("DeviceAccountId");

                    b.HasOne("HES.Core.Entities.Device", "Device")
                        .WithMany()
                        .HasForeignKey("DeviceId");

                    b.HasOne("HES.Core.Entities.Employee", "Employee")
                        .WithMany()
                        .HasForeignKey("EmployeeId");

                    b.HasOne("HES.Core.Entities.Workstation", "Workstation")
                        .WithMany()
                        .HasForeignKey("WorkstationId");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole")
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("HES.Core.Entities.ApplicationUser")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("HES.Core.Entities.ApplicationUser")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole")
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("HES.Core.Entities.ApplicationUser")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("HES.Core.Entities.ApplicationUser")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
