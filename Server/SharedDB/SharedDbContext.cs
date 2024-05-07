using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharedDB
{
    public class SharedDbContext : DbContext
    {
        public DbSet<TokenDb> Tokens { get; set; }
        public DbSet<ServerDb> Servers { get; set; }
        public SharedDbContext()
        {
            
        }
        public SharedDbContext(DbContextOptions<SharedDbContext> options) : base(options)
        {

        }
        public string ConnectionString { get; set; } = @"Data Source=(localdb)\ProjectModels;Initial Catalog=SharedDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;";

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            if(options.IsConfigured == false)
            {
                options
                //.UseLoggerFactory(_logger)
                .UseSqlServer(ConnectionString);
            }
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<TokenDb>()
                .HasIndex(t => t.AccountDbId)
                .IsUnique();

            builder.Entity<ServerDb>()
                .HasIndex(s => s.Name)
                .IsUnique();
        }
    }
}
