using System;
using Microsoft.Extensions.Configuration;
using Thinktecture.Relay.Docker.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class IServiceCollectionExtensions
    {

        public static PostgreSqlConfig AddRelayPostgreSqlConfig(this IServiceCollection services, IConfiguration configuration)
        {
			  	if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            var postgreSqlConfig = new PostgreSqlConfig();
            var postgreSection = configuration.GetSection(PostgreSqlConfig.SECTION_NAME);
            if (postgreSection == null)
            {
                throw new ArgumentOutOfRangeException("PostgreSql Config not present");
            }
            postgreSection.Bind(postgreSqlConfig);
            services.AddSingleton(postgreSqlConfig);
            return postgreSqlConfig;
        }

        public static RabbitMqConfig AddRelayRabbitMqConfig(this IServiceCollection services, IConfiguration configuration)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            var rabbitMqConfig = new RabbitMqConfig();
            var rabbitMqSection = configuration.GetSection(RabbitMqConfig.SECTION_NAME);
            if (rabbitMqSection == null)
            {
                throw new ArgumentOutOfRangeException("RabbitMQ Config not present");
            }
            rabbitMqSection.Bind(rabbitMqConfig);
            services.AddSingleton(rabbitMqConfig);
            return rabbitMqConfig;
        }


    }
}
