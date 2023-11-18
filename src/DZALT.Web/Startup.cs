using System;
using Asp.Versioning.ApiExplorer;
using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DZALT.Entities.Persistent.Postgres;
using DZALT.Web.Infrastructure;
using DZALT.Entities.Tracing;

namespace DZALT.Web
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		public string ConnectionString { get; private set; }

		public void ConfigureServices(IServiceCollection services)
		{
			services.AddControllers();
			services.AddMediatR(c =>
			{
				c.RegisterServicesFromAssembly(typeof(IDataStore).Assembly);
			});

			services.AddCustomApiVersioning();
			services.AddCustomSwagger("DZALT API", "v1");
		}

		public void Configure(
			IApplicationBuilder app,
			IApiVersionDescriptionProvider provider)
		{
			app.UseMiddleware<TimerMiddleware>();

            app.UseDefaultFiles();
			app.UseStaticFiles();
            
			app.UseSwagger();
			app.UseCustomSwaggerUi(provider, Configuration);

			app.UseRouting();
			app.UseEndpoints(end => end.MapControllers());
		}

		public void ConfigureContainer(ContainerBuilder builder)
		{
			builder
				.Register<PostgreSqlDataStore.PostgreSqlDataStoreConfig>(c => new PostgreSqlDataStore.PostgreSqlDataStoreConfig()
				{
					Name = "dzalt-test",
					Address = Environment.GetEnvironmentVariable(EnvironmentVariables.DZALT_PG_ADDRESS) ?? string.Empty,
					Port = Environment.GetEnvironmentVariable(EnvironmentVariables.DZALT_PG_PORT) ?? string.Empty,
					User = Environment.GetEnvironmentVariable(EnvironmentVariables.DZALT_PG_USER) ?? string.Empty,
					Password = Environment.GetEnvironmentVariable(EnvironmentVariables.DZALT_PG_PASSWORD) ?? string.Empty,
				})
				.InstancePerLifetimeScope();

			builder
				.Register<PostgreSqlDataStore>(c => new PostgreSqlDataStore(
					c.Resolve<PostgreSqlDataStore.PostgreSqlDataStoreConfig>()))
				.As<IDataStore>()
				.InstancePerLifetimeScope();

			builder
				.Register(c => c.Resolve<IDataStore>().OpenSession())
				.As<ISession>()
				.As<IRepository>()
				.InstancePerLifetimeScope();

			builder
				.RegisterType<DirTracer>()
				.As<IDirTracer>()
				.InstancePerLifetimeScope();
			builder
				.RegisterType<FileTracer>()
				.As<IFileTracer>()
				.InstancePerLifetimeScope();
			builder
				.RegisterType<LineTracer>()
				.As<ILineTracer>()
				.InstancePerLifetimeScope();
		}
	}
}
