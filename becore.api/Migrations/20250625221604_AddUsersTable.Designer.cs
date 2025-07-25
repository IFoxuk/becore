﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using becore.api.Scheme;

#nullable disable

namespace becore.api.Migrations
{
    [DbContext(typeof(ApplicationContext))]
    [Migration("20250625221604_AddUsersTable")]
    partial class AddUsersTable
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.6")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("becore.api.Scheme.Packs.AddonPack", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("Created")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Description")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(32)
                        .HasColumnType("character varying(32)");

                    b.Property<int>("PackType")
                        .HasColumnType("integer");

                    b.Property<Guid>("PageId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("PageId");

                    b.ToTable("AddonPack");
                });

            modelBuilder.Entity("becore.api.Scheme.Packs.DataPack", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("Created")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Description")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(32)
                        .HasColumnType("character varying(32)");

                    b.Property<int>("PackType")
                        .HasColumnType("integer");

                    b.Property<Guid>("PageId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("PageId");

                    b.ToTable("DataPacks");
                });

            modelBuilder.Entity("becore.api.Scheme.Packs.ResourcesPack", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("Created")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Description")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(32)
                        .HasColumnType("character varying(32)");

                    b.Property<int>("PackType")
                        .HasColumnType("integer");

                    b.Property<Guid>("PageId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("PageId");

                    b.ToTable("ResourcePack");
                });

            modelBuilder.Entity("becore.api.Scheme.Packs.ScriptPack", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("Created")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Description")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(32)
                        .HasColumnType("character varying(32)");

                    b.Property<int>("PackType")
                        .HasColumnType("integer");

                    b.Property<Guid>("PageId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("PageId");

                    b.ToTable("ScriptPack");
                });

            modelBuilder.Entity("becore.api.Scheme.Page", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Description")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(32)
                        .HasColumnType("character varying(32)");

                    b.Property<string>("QuadIcon")
                        .HasColumnType("text");

                    b.Property<string>("WideIcon")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Page");
                });

            modelBuilder.Entity("becore.api.Scheme.PageTag", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("PageId")
                        .HasColumnType("uuid");

                    b.Property<string>("TagName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.HasKey("Id");

                    b.HasIndex("PageId");

                    b.ToTable("PageTag");
                });

            modelBuilder.Entity("becore.api.Scheme.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<bool>("IsActive")
                        .HasColumnType("boolean");

                    b.Property<DateTime?>("LastLoginAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("PasswordHash")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("becore.api.Scheme.Packs.AddonPack", b =>
                {
                    b.HasOne("becore.api.Scheme.Page", "Page")
                        .WithMany()
                        .HasForeignKey("PageId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Page");
                });

            modelBuilder.Entity("becore.api.Scheme.Packs.DataPack", b =>
                {
                    b.HasOne("becore.api.Scheme.Page", "Page")
                        .WithMany()
                        .HasForeignKey("PageId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Page");
                });

            modelBuilder.Entity("becore.api.Scheme.Packs.ResourcesPack", b =>
                {
                    b.HasOne("becore.api.Scheme.Page", "Page")
                        .WithMany()
                        .HasForeignKey("PageId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Page");
                });

            modelBuilder.Entity("becore.api.Scheme.Packs.ScriptPack", b =>
                {
                    b.HasOne("becore.api.Scheme.Page", "Page")
                        .WithMany()
                        .HasForeignKey("PageId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Page");
                });

            modelBuilder.Entity("becore.api.Scheme.PageTag", b =>
                {
                    b.HasOne("becore.api.Scheme.Page", "Page")
                        .WithMany("PageTags")
                        .HasForeignKey("PageId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Page");
                });

            modelBuilder.Entity("becore.api.Scheme.Page", b =>
                {
                    b.Navigation("PageTags");
                });
#pragma warning restore 612, 618
        }
    }
}
