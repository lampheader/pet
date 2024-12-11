using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace hh_db.DB_Models;

public partial class TestVacanciesDbContext : DbContext
{
    public TestVacanciesDbContext()
    {
    }

    public TestVacanciesDbContext(DbContextOptions<TestVacanciesDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Address> Addresses { get; set; }

    public virtual DbSet<ApplicantsLocationRequirement> ApplicantsLocationRequirements { get; set; }

    public virtual DbSet<BaseSalary> BaseSalarys { get; set; }

    public virtual DbSet<DateStatus> DateStatuses { get; set; }

    public virtual DbSet<HiringOrganization> HiringOrganizations { get; set; }

    public virtual DbSet<Identifier> Identifiers { get; set; }

    public virtual DbSet<JobLocation> JobLocations { get; set; }

    public virtual DbSet<Skill> Skills { get; set; }

    public virtual DbSet<Vacancy> Vacancies { get; set; }

    public virtual DbSet<Value> Values { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=localhost;TrustServerCertificate=True;Database=test_vacancies_db;User=sa;Password=113322Qq__;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Address>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("addr_p_key");

            entity.Property(e => e.AddressCountry)
                .HasMaxLength(50)
                .HasColumnName("addressCountry");
            entity.Property(e => e.AddressLocality)
                .HasMaxLength(50)
                .HasColumnName("addressLocality");
            entity.Property(e => e.AddressRegion)
                .HasMaxLength(50)
                .HasColumnName("addressRegion");
            entity.Property(e => e.StreetAddress)
                .HasMaxLength(80)
                .HasColumnName("streetAddress");
            entity.Property(e => e.Type)
                .HasMaxLength(50)
                .HasColumnName("type");
        });

        modelBuilder.Entity<ApplicantsLocationRequirement>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("appLocReq_p_key");

            entity.Property(e => e.Name)
                .HasMaxLength(60)
                .HasColumnName("name");
            entity.Property(e => e.Type)
                .HasMaxLength(50)
                .HasColumnName("type");
        });

        modelBuilder.Entity<BaseSalary>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("BaseSal_p_key");

            entity.Property(e => e.Currency)
                .HasMaxLength(50)
                .HasColumnName("currency");
            entity.Property(e => e.Type)
                .HasMaxLength(50)
                .HasColumnName("type");
            entity.Property(e => e.ValueId).HasColumnName("valueId");

            entity.HasOne(d => d.Value).WithMany(p => p.BaseSalaries)
                .HasForeignKey(d => d.ValueId)
                .HasConstraintName("BaseSal_value_f_key");
        });

        modelBuilder.Entity<DateStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("DateStatus_p_key");

            entity.Property(e => e.DatePosted)
                .HasColumnType("datetime")
                .HasColumnName("datePosted");
            entity.Property(e => e.LastUpdate)
                .HasColumnType("datetime")
                .HasColumnName("lastUpdate");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("null")
                .HasColumnName("status");
            entity.Property(e => e.ValIdThrough)
                .HasColumnType("datetime")
                .HasColumnName("valIdThrough");
        });

        modelBuilder.Entity<HiringOrganization>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("HirOrg_p_key");

            entity.Property(e => e.Name)
                .HasMaxLength(300)
                .HasColumnName("name");
            entity.Property(e => e.Type)
                .HasMaxLength(50)
                .HasColumnName("type");
        });

        modelBuilder.Entity<Identifier>(entity =>
        {
            entity.HasKey(e => e.Value).HasName("Id_p_key");

            entity.Property(e => e.Value)
                .ValueGeneratedNever()
                .HasColumnName("value");
            entity.Property(e => e.Name)
                .HasMaxLength(300)
                .HasColumnName("name");
            entity.Property(e => e.Type)
                .HasMaxLength(50)
                .HasColumnName("type");
        });

        modelBuilder.Entity<JobLocation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("JobLocId_p_key");

            entity.Property(e => e.AddressId)
                .HasDefaultValueSql("(NULL)")
                .HasColumnName("addressId");
            entity.Property(e => e.Type)
                .HasMaxLength(50)
                .HasColumnName("type");

            entity.HasOne(d => d.Address).WithMany(p => p.JobLocations)
                .HasForeignKey(d => d.AddressId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("addId_JobLoc_f_key");
        });

        modelBuilder.Entity<Skill>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Skills_p_key");

            entity.HasIndex(e => e.Name, "skillName_uniq").IsUnique();

            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Vacancy>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Vacancies_p_key");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.BaseSalaryId).HasDefaultValueSql("(NULL)");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.EmploymentType)
                .HasMaxLength(50)
                .HasColumnName("employmentType");
            entity.Property(e => e.Experience)
                .HasMaxLength(20)
                .HasColumnName("experience");
            entity.Property(e => e.Title)
                .HasMaxLength(300)
                .HasColumnName("title");

            entity.HasOne(d => d.ApplicantLocationRequirements).WithMany(p => p.Vacancies)
                .HasForeignKey(d => d.ApplicantLocationRequirementsId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("AppLocReq_Id_f_key");

            entity.HasOne(d => d.BaseSalary).WithMany(p => p.Vacancies)
                .HasForeignKey(d => d.BaseSalaryId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("BasSalId_val_f_key");

            entity.HasOne(d => d.DateStatus).WithMany(p => p.Vacancies)
                .HasForeignKey(d => d.DateStatusId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("DateStId_f_key");

            entity.HasOne(d => d.HiringOrganization).WithMany(p => p.Vacancies)
                .HasForeignKey(d => d.HiringOrganizationId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("HirOrgId_f_key");

            entity.HasOne(d => d.IdNavigation).WithOne(p => p.Vacancy)
                .HasForeignKey<Vacancy>(d => d.Id)
                .HasConstraintName("Id_f_key");

            entity.HasOne(d => d.JobLocation).WithMany(p => p.Vacancies)
                .HasForeignKey(d => d.JobLocationId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("JobLocId_f_key");

            entity.HasMany(d => d.Skills).WithMany(p => p.Vacancies)
                .UsingEntity<Dictionary<string, object>>(
                    "VacancySkill",
                    r => r.HasOne<Skill>().WithMany()
                        .HasForeignKey("SkillId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("Skill_f_key"),
                    l => l.HasOne<Vacancy>().WithMany()
                        .HasForeignKey("VacancyId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("Vac_f_key"),
                    j =>
                    {
                        j.HasKey("VacancyId", "SkillId").HasName("VacancySkills_p_key");
                        j.ToTable("VacancySkills");
                    });
        });

        modelBuilder.Entity<Value>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Value_p_key");

            entity.ToTable("Value");

            entity.Property(e => e.MaxValue)
                .HasDefaultValueSql("(NULL)")
                .HasColumnName("maxValue");
            entity.Property(e => e.MinValue)
                .HasDefaultValueSql("(NULL)")
                .HasColumnName("minValue");
            entity.Property(e => e.Type)
                .HasMaxLength(50)
                .HasColumnName("type");
            entity.Property(e => e.UnitText)
                .HasMaxLength(50)
                .HasColumnName("unitText");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
