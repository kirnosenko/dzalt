using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Web;

namespace DZALT.Web
{
	public class Program
	{
		public static void Main(string[] args)
		{
			LogManager.Setup().LoadConfigurationFromAppSettings();
			CreateWebHostBuilder(args).Build().Run();
		}

		public static IHostBuilder CreateWebHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.UseServiceProviderFactory(f => new AutofacServiceProviderFactory())
				.ConfigureWebHostDefaults(webHostBuilder =>
				{
					webHostBuilder.ConfigureAppConfiguration((hostingContext, config) =>
					{
						config
							.SetBasePath(hostingContext.HostingEnvironment.ContentRootPath)
							.AddJsonFile("appsettings.json", true, true)
							.AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", true, true)
							.AddEnvironmentVariables();
					});

					webHostBuilder
						.UseStartup<Startup>()
						.ConfigureLogging(logging => { logging.ClearProviders(); })
						.UseNLog();
				});
	}
}