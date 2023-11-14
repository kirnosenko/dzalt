using System;
using System.IO;
using System.Linq;
using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace DZALT.Web.Infrastructure
{
	public static class Extensions
	{
		public static IServiceCollection AddCustomApiVersioning(this IServiceCollection services)
		{
			services
				.AddApiVersioning(options =>
				{
					// reporting api versions will return the headers
					// "api-supported-versions" and "api-deprecated-versions"
					options.ReportApiVersions = true;
				})
				.AddApiExplorer(options =>
				{
					// add the versioned api explorer, which also adds IApiVersionDescriptionProvider service
					// note: the specified format code will format the version as "'v'major[.minor][-status]"
					options.GroupNameFormat = "'v'VVV";

					// note: this option is only necessary when versioning by url segment. the SubstitutionFormat
					// can also be used to control the format of the API version in route templates
					options.SubstituteApiVersionInUrl = true;
				});

			return services;
		}

		public static IServiceCollection AddCustomSwagger(
			this IServiceCollection services,
			string title,
			string version)
		{
			services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc(version, new OpenApiInfo { Title = title, Version = version });

				var filesXml = Directory
					.GetFiles(AppContext.BaseDirectory)
					.Where(x => x.Contains(".xml"));
				foreach (var fileXml in filesXml)
				{
					var fullPath = Path.Combine(AppContext.BaseDirectory, fileXml);
					c.IncludeXmlComments(fullPath);
				}
			});

			return services;
		}

		public static IApplicationBuilder UseCustomSwaggerUi(
			this IApplicationBuilder app,
			IApiVersionDescriptionProvider provider,
			IConfiguration configuration)
		{
			app.UseSwaggerUI(
				options =>
				{
					// build a swagger endpoint for each discovered API version
					foreach (var description in provider.ApiVersionDescriptions)
					{
						string swaggerJsonBasePath = string.IsNullOrWhiteSpace(options.RoutePrefix) ? "." : "..";
						options.SwaggerEndpoint(
							$"{swaggerJsonBasePath}/swagger/{description.GroupName}/swagger.json",
							description.GroupName.ToUpperInvariant());
					}
				});

			return app;
		}
	}
}
