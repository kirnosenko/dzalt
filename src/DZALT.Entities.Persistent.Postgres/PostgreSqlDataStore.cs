using System;
using Microsoft.EntityFrameworkCore;

namespace DZALT.Entities.Persistent.Postgres
{
	public class PostgreSqlDataStore : NamedDataStore
	{
		public class PostgreSqlDataStoreConfig
		{
			public string Name { get; set; }
			public string Address { get; set; }
			public string Port { get; set; }
			public string User { get; set; }
			public string Password { get; set; }
		}

		private readonly string address;
		private readonly string credentials;
		
		public PostgreSqlDataStore(PostgreSqlDataStoreConfig config)
			: base(config.Name)
		{
			if (string.IsNullOrEmpty(config.Port))
			{
				config.Port = "5432";
			}
			this.address = $"Server={config.Address};Port={config.Port}";
			this.credentials = $"User ID={config.User};Password={config.Password}";
		}

		protected override void Configure(DbContextOptionsBuilder options)
		{
			var cs = $"{credentials};{address};Database={name};Integrated Security=true;Pooling=true;";

			options.UseNpgsql(cs);
			AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
		}
	}
}
