﻿using Autofac;
using Autofac.Extensions.DependencyInjection;
using Competencias.Api;
using Competencias.Api.Controllers;
using Competencias.Domain;
using Competencias.Handlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Common;
using Swashbuckle.AspNetCore.Swagger;

public class Startup
{
	public IConfiguration Configuration { get; }
	public IContainer Container { get; private set; }

	public Startup(IHostingEnvironment env)
	{
		var builder = new ConfigurationBuilder()
			.SetBasePath(env.ContentRootPath)
			.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
			.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
			.AddEnvironmentVariables();
		Configuration = builder.Build();
	}

	public void ConfigureServices(IServiceCollection services)
	{
		services.AddMvc().AddControllersAsServices();

		services.AddSwaggerGen(c =>
		{
			c.SwaggerDoc("v1", new Info { Title = "Competencia API", Version = "v1" });
			c.DescribeAllEnumsAsStrings();
		});

		services.AddDbContext<AppDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("AppDatabase")));

		var builder = new ContainerBuilder();

		builder.Populate(services);

		ConfigureContainer(builder);

		Container = builder.Build();

	}

	public void Configure(IApplicationBuilder app, IHostingEnvironment env)
	{
		app.UseMvc();
		app.UseSwagger();
		app.UseSwaggerUI(c =>
		{
			c.SwaggerEndpoint("/swagger/v1/swagger.json", "Competencia API V1");
		});
	}

	public void ConfigureContainer(ContainerBuilder builder)
	{
		builder.RegisterType<CompetenciaController>().PropertiesAutowired();
		builder.RegisterType<UnitOfWork>().As<IUnitOfWork>();

		builder.RegisterAssemblyTypes(typeof(CompetenciaCriadaHandler).Assembly)
			   .AsClosedTypesOf(typeof(IHandler<>));

	}

}