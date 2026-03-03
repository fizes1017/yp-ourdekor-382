using System;
using MetalStructuresAPI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MetalStructuresAPI.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("MetalStructuresAPI.Models.Calculation", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateOnly>("CalculatedAt")
                        .HasColumnType("date")
                        .HasColumnName("calculated_at");

                    b.Property<decimal>("TotalAmount")
                        .HasColumnType("decimal(10,2)")
                        .HasColumnName("total_amount");

                    b.HasKey("Id");

                    b.ToTable("calculations", (string)null);
                });

            modelBuilder.Entity("MetalStructuresAPI.Models.CalculationItem", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("CalculationId")
                        .HasColumnType("integer")
                        .HasColumnName("calculation_id");

                    b.Property<int>("MaterialId")
                        .HasColumnType("integer")
                        .HasColumnName("material_id");

                    b.Property<decimal>("Quantity")
                        .HasColumnType("decimal(10,3)")
                        .HasColumnName("quantity");

                    b.Property<decimal>("TotalPrice")
                        .HasColumnType("decimal(10,2)")
                        .HasColumnName("total_price");

                    b.Property<decimal>("UnitPrice")
                        .HasColumnType("decimal(10,2)")
                        .HasColumnName("unit_price");

                    b.HasKey("Id");

                    b.HasIndex("CalculationId");

                    b.HasIndex("MaterialId");

                    b.ToTable("calculation_items", (string)null);
                });

            modelBuilder.Entity("MetalStructuresAPI.Models.Material", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Article")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)")
                        .HasColumnName("article");

                    b.Property<DateOnly>("CreatedAt")
                        .HasColumnType("date")
                        .HasColumnName("created_at");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("name");

                    b.Property<decimal>("Price")
                        .HasColumnType("decimal(10,2)")
                        .HasColumnName("price");

                    b.Property<string>("Unit")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("character varying(20)")
                        .HasColumnName("unit");

                    b.HasKey("Id");

                    b.HasIndex("Article")
                        .IsUnique();

                    b.ToTable("materials", (string)null);
                });

            modelBuilder.Entity("MetalStructuresAPI.Models.Manager", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp without time zone")
                        .HasColumnName("created_at");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("email");

                    b.Property<string>("FullName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("full_name");

                    b.Property<string>("PasswordHash")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)")
                        .HasColumnName("password_hash");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("timestamp without time zone")
                        .HasColumnName("updated_at");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("username");

                    b.HasKey("Id");

                    b.HasIndex("Email")
                        .IsUnique();

                    b.HasIndex("Username")
                        .IsUnique();

                    b.ToTable("managers", (string)null);
                });

            modelBuilder.Entity("MetalStructuresAPI.Models.CalculationItem", b =>
                {
                    b.HasOne("MetalStructuresAPI.Models.Calculation", "Calculation")
                        .WithMany("CalculationItems")
                        .HasForeignKey("CalculationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MetalStructuresAPI.Models.Material", "Material")
                        .WithMany()
                        .HasForeignKey("MaterialId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Calculation");

                    b.Navigation("Material");
                });

            modelBuilder.Entity("MetalStructuresAPI.Models.Calculation", b =>
                {
                    b.Navigation("CalculationItems");
                });
#pragma warning restore 612, 618
        }
    }
}





