using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Library.API.Services;
using Library.API.Entities;
using Microsoft.EntityFrameworkCore;
using Library.API.Helpers;

namespace Library.API
{
    public class Startup
    {
        public static IConfigurationRoot Configuration;

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appSettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appSettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            // register the DbContext on the container, getting the connection string from
            // appSettings (note: use this during development; in a production environment,
            // it's better to store the connection string in an environment variable)
            var connectionString = Configuration["connectionStrings:libraryDBConnectionString"];
            services.AddDbContext<LibraryContext>(o => o.UseSqlServer(connectionString));

            // register the repository
            services.AddScoped<ILibraryRepository, LibraryRepository>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, 
            ILoggerFactory loggerFactory, LibraryContext libraryContext)
        {
            loggerFactory.AddConsole();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler();
            }

            // to use automapper, first download automapper nuget package and then
            // here in Configure function in Startup.cs add mapping properties
            // from source to destination
            // for automapping, it requires property name in the source and destination
            // to be the same so they can be matched and then be able to mapped
            AutoMapper.Mapper.Initialize(
                cfg => { cfg.CreateMap<Entities.Author, Models.AuthorDto>()
                    .ForMember(dest => dest.Name, opt => opt.MapFrom(source => $"{source.FirstName} {source.LastName}")) // projection
                    .ForMember(dest => dest.Age, opt => opt.MapFrom(source => source.DateOfBirth.GetCurrentAge()));
            });

            // formember is called projection
            // since properties in source and destination must be the same for the mapping
            // but since we need in dto class current age rather than datofbirth and first+lastname of
            // author, we used formember for mapping without error


            libraryContext.EnsureSeedDataForContext();

            app.UseMvc(); 
        }
    }
}
