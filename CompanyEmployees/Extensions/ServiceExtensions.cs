﻿using Contracts;
using LoggerService;
using Repository;
using Service.Contracts;
using Service;
using Microsoft.EntityFrameworkCore;
using Service.DataShaping;
using Shared.DataTransferObjects;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc;

namespace CompanyEmployees.Extensions;

public static class ServiceExtensions
{
    public static void ConfigureCors(this IServiceCollection services) => 
        services.AddCors(options =>
        { 
            options.AddPolicy("CorsPolicy", builder => 
                    builder.AllowAnyOrigin()
                            .AllowAnyMethod()
                            .AllowAnyHeader()
                            .WithExposedHeaders("X-Pagination")); 
        });

    public static void ConfigureIISIntegration(this IServiceCollection services) => 
        services.Configure<IISOptions>(options => { });

    public static void ConfigureLoggerService(this IServiceCollection services) => 
        services.AddSingleton<ILoggerManager, LoggerManager>();

    public static void ConfigureRepositoryManager(this IServiceCollection services) => 
        services.AddScoped<IRepositoryManager, RepositoryManager>();

    public static void ConfigureServiceManager(this IServiceCollection services)
    {
        services.AddScoped<IDataShaper<EmployeeDto>, DataShaper<EmployeeDto>>();
        services.AddScoped<IServiceManager, ServiceManager>();
    }


    public static void ConfigureSqlContext(this IServiceCollection services, IConfiguration configuration) => 
        services.AddDbContext<RepositoryContext>(opts => 
                opts.UseSqlServer(configuration.GetConnectionString("sqlConnection")));

    public static IMvcBuilder AddCustomCSVFormatter(this IMvcBuilder builder) => 
        builder.AddMvcOptions(config => config.OutputFormatters.Add(new CsvOutputFormatter()));

    public static void AddCustomMediaTypes(this IServiceCollection services) 
    { 
        services.Configure<MvcOptions>(config => 
        { 
            var systemTextJsonOutputFormatter = config.OutputFormatters.OfType<SystemTextJsonOutputFormatter>()?.FirstOrDefault(); 
            if (systemTextJsonOutputFormatter != null) 
            { 
                systemTextJsonOutputFormatter.SupportedMediaTypes.Add("application/vnd.ramesh.hateoas+json"); 
            } 
            var xmlOutputFormatter = config.OutputFormatters.OfType<XmlDataContractSerializerOutputFormatter>()?.FirstOrDefault(); 
            if (xmlOutputFormatter != null) 
            { 
                xmlOutputFormatter.SupportedMediaTypes.Add("application/vnd.ramesh.hateoas+xml"); 
            } 
        });
    }
}
