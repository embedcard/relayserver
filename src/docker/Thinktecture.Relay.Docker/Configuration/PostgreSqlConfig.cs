using System;

namespace Thinktecture.Relay.Docker.Configuration
{
	public class PostgreSqlConfig
	{
		public const string SECTION_NAME = "PostgreSql";
		public String Host { get; set; }
		public String Database { get; set; }
		public String Username { get; set; }
		public String Password { get; set; }
		public String ConnectionString => $"host={Host};database={Database};username={Username};password={Password}";
	}
}
