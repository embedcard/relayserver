﻿using System.Data.Entity;
using Thinktecture.Relay.Server.Repository.DbModels;

namespace Thinktecture.Relay.Server.Repository
{
    internal class RelayContext : DbContext
    {
        public DbSet<DbLink> Links { get; set; }
        public DbSet<DbRequestLogEntry> RequestLogEntries { get; set; }
        public DbSet<DbUser> Users { get; set; }
        public DbSet<DbTraceConfiguration> TraceConfigurations { get; set; }
        public DbSet<DbActiveConnection> ActiveConnections{ get; set; }

        public RelayContext()
        {
            Configuration.AutoDetectChangesEnabled = false;
            Configuration.LazyLoadingEnabled = false;
            Configuration.ProxyCreationEnabled = false;
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // add composite key for active connections table
            modelBuilder.Entity<DbActiveConnection>()
                .HasKey(t => new {t.LinkId, t.ConnectionId, t.OriginId});
        }
    }
}
