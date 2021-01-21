﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Thinktecture.Relay.Server.Persistence.EntityFrameworkCore;

namespace Thinktecture.Relay.Server.Persistence.EntityFrameworkCore.PostgreSql.Migrations.ConfigurationDb
{
    [DbContext(typeof(RelayDbContext))]
    partial class RelayServerConfigurationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .HasAnnotation("ProductVersion", "3.1.7")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("Thinktecture.Relay.Server.Persistence.Models.ClientSecret", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("Created")
                        .HasColumnType("timestamp without time zone");

                    b.Property<DateTime?>("Expiration")
                        .HasColumnType("timestamp without time zone");

                    b.Property<Guid>("TenantId")
                        .HasColumnType("uuid");

                    b.Property<string>("Value")
                        .IsRequired()
                        .HasColumnType("character varying(4000)")
                        .HasMaxLength(4000);

                    b.HasKey("Id");

                    b.HasIndex("TenantId");

                    b.ToTable("ClientSecrets");
                });

            modelBuilder.Entity("Thinktecture.Relay.Server.Persistence.Models.Config", b =>
                {
                    b.Property<Guid>("TenantId")
                        .HasColumnType("uuid");

                    b.Property<bool?>("EnableTracing")
                        .HasColumnType("boolean");

                    b.Property<TimeSpan?>("KeepAliveInterval")
                        .HasColumnType("interval");

                    b.Property<TimeSpan?>("ReconnectMaximumDelay")
                        .HasColumnType("interval");

                    b.Property<TimeSpan?>("ReconnectMinimumDelay")
                        .HasColumnType("interval");

                    b.HasKey("TenantId");

                    b.ToTable("Configs");
                });

            modelBuilder.Entity("Thinktecture.Relay.Server.Persistence.Models.Connection", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("character varying(100)")
                        .HasMaxLength(100);

                    b.Property<DateTimeOffset>("ConnectTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTimeOffset?>("DisconnectTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTimeOffset?>("LastActivityTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("OriginId")
                        .HasColumnType("uuid");

                    b.Property<string>("RemoteIpAddress")
                        .HasColumnType("text");

                    b.Property<Guid>("TenantId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("OriginId");

                    b.HasIndex("TenantId");

                    b.ToTable("Connections");
                });

            modelBuilder.Entity("Thinktecture.Relay.Server.Persistence.Models.Origin", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid");

                    b.Property<DateTimeOffset>("LastSeenTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTimeOffset?>("ShutdownTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTimeOffset>("StartupTime")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.ToTable("Origins");
                });

            modelBuilder.Entity("Thinktecture.Relay.Server.Persistence.Models.Request", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<bool>("Aborted")
                        .HasColumnType("boolean");

                    b.Property<bool>("Errored")
                        .HasColumnType("boolean");

                    b.Property<bool>("Expired")
                        .HasColumnType("boolean");

                    b.Property<bool>("Failed")
                        .HasColumnType("boolean");

                    b.Property<string>("HttpMethod")
                        .IsRequired()
                        .HasColumnType("character varying(10)")
                        .HasMaxLength(10);

                    b.Property<int?>("HttpStatusCode")
                        .HasColumnType("integer");

                    b.Property<long>("RequestBodySize")
                        .HasColumnType("bigint");

                    b.Property<DateTimeOffset>("RequestDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<long>("RequestDuration")
                        .HasColumnType("bigint");

                    b.Property<Guid>("RequestId")
                        .HasColumnType("uuid");

                    b.Property<string>("RequestUrl")
                        .IsRequired()
                        .HasColumnType("character varying(1000)")
                        .HasMaxLength(1000);

                    b.Property<long?>("ResponseBodySize")
                        .HasColumnType("bigint");

                    b.Property<string>("Target")
                        .IsRequired()
                        .HasColumnType("character varying(100)")
                        .HasMaxLength(100);

                    b.Property<Guid>("TenantId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("TenantId");

                    b.ToTable("Requests");
                });

            modelBuilder.Entity("Thinktecture.Relay.Server.Persistence.Models.Tenant", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Description")
                        .HasColumnType("character varying(1000)")
                        .HasMaxLength(1000);

                    b.Property<string>("DisplayName")
                        .HasColumnType("character varying(200)")
                        .HasMaxLength(200);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("character varying(100)")
                        .HasMaxLength(100);

                    b.Property<string>("NormalizedName")
                        .IsRequired()
                        .HasColumnType("character varying(100)")
                        .HasMaxLength(100);

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.HasIndex("NormalizedName")
                        .IsUnique();

                    b.ToTable("Tenants");
                });

            modelBuilder.Entity("Thinktecture.Relay.Server.Persistence.Models.ClientSecret", b =>
                {
                    b.HasOne("Thinktecture.Relay.Server.Persistence.Models.Tenant", null)
                        .WithMany("ClientSecrets")
                        .HasForeignKey("TenantId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Thinktecture.Relay.Server.Persistence.Models.Config", b =>
                {
                    b.HasOne("Thinktecture.Relay.Server.Persistence.Models.Tenant", null)
                        .WithOne("Config")
                        .HasForeignKey("Thinktecture.Relay.Server.Persistence.Models.Config", "TenantId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Thinktecture.Relay.Server.Persistence.Models.Connection", b =>
                {
                    b.HasOne("Thinktecture.Relay.Server.Persistence.Models.Origin", null)
                        .WithMany("Connections")
                        .HasForeignKey("OriginId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Thinktecture.Relay.Server.Persistence.Models.Tenant", null)
                        .WithMany("Connections")
                        .HasForeignKey("TenantId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Thinktecture.Relay.Server.Persistence.Models.Request", b =>
                {
                    b.HasOne("Thinktecture.Relay.Server.Persistence.Models.Tenant", null)
                        .WithMany("Requests")
                        .HasForeignKey("TenantId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
