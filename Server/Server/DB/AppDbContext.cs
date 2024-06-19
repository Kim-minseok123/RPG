using Google.Protobuf.WellKnownTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Server.Data;
using Server.Game;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace Server.DB
{
	public class AppDbContext : DbContext
	{
		public DbSet<AccountDb> Accounts { get; set; }
		public DbSet<PlayerDb> Players { get; set; }
		public DbSet<ItemDb> Items { get; set; }
		public DbSet<SkillDb> Skills { get; set; }
		public DbSet<QuickSlotDb> QuickSlots { get; set; }

		static readonly ILoggerFactory _logger = LoggerFactory.Create(builder => { builder.AddConsole(); });

		string _connectionString = @"Data Source=(localdb)\ProjectModels;Initial Catalog=GameDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;";
        
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options
            //.UseLoggerFactory(_logger)
                .UseSqlServer(ConfigManager.Config == null ? _connectionString : ConfigManager.Config.connectionString);
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
			builder.Entity<AccountDb>()
				.HasIndex(a => a.AccountLoginId)
				.IsUnique();

			builder.Entity<PlayerDb>()
				.HasIndex(p => p.PlayerName)
				.IsUnique();

            builder.Entity<SkillDb>()
            .HasAlternateKey(s => new { s.PlayerDbId, s.TemplateId });

            builder.Entity<QuickSlotDb>()
            .HasAlternateKey(s => new { s.PlayerDbId, s.Slot });
        }
	}
}
