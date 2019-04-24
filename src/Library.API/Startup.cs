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
using Microsoft.AspNetCore.Diagnostics;
using Swashbuckle.AspNetCore.Swagger;
using Microsoft.AspNetCore.Mvc.Formatters;
using NLog.Extensions.Logging;


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
            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "Ece's API", Version = "v1" });
            });

            services.AddMvc(setupAction =>
            {
                setupAction.ReturnHttpNotAcceptable = true; // return 406 not acceptable if the format is not right (e.g: json instead of xml)
                setupAction.OutputFormatters.Add(new XmlDataContractSerializerOutputFormatter()); // accept header as xml format
                setupAction.InputFormatters.Add((new XmlDataContractSerializerInputFormatter())); // content-type header as xml
            });

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

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), 
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Ece's API V1");
                c.RoutePrefix = string.Empty; // by adding this, swagger is shows up as soon as we type http://localhost:<port>/sss
            });

            loggerFactory.AddConsole();
            loggerFactory.AddDebug(LogLevel.Information);
            //loggerFactory.AddProvider(new NLogLoggerProvider());
            loggerFactory.AddNLog();
           
            // middleware
            if (env.IsDevelopment())
            {
                // use stacktrace to see error in dev env
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // use exception handler to see error in production env
                app.UseExceptionHandler(appBuilder => {
                    appBuilder.Run(async context =>
                    {
                        var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();
                        if (exceptionHandlerFeature != null)
                        {
                            // catches invalid patch error
                            var logger = loggerFactory.CreateLogger("Global Exception Logger");
                            logger.LogError(500, exceptionHandlerFeature.Error, exceptionHandlerFeature.Error.Message);
                        }

                        context.Response.StatusCode = 500; // server error
                        await context.Response.WriteAsync("An unexpected error happened. Try again later");
                    });
                });
            }

            // to use automapper, first download automapper nuget package and then
            // here in Configure function in Startup.cs add mapping properties
            // from source to destination
            // for automapping, it requires property name in the source and destination
            // to be the same so they can be matched and then be able to mapped
            AutoMapper.Mapper.Initialize(
                cfg => { cfg.CreateMap<Author, Models.AuthorDto>()
                    .ForMember(dest => dest.Name, opt => opt.MapFrom(source => $"{source.FirstName} {source.LastName}")) // projection
                    .ForMember(dest => dest.Age, opt => opt.MapFrom(source => source.DateOfBirth.GetCurrentAge()));

                    cfg.CreateMap<Book, Models.BookDto>();

                    cfg.CreateMap<Models.AuthorForCreationDto, Author>();
                    cfg.CreateMap<Models.BookForCreationDto, Book>();
                    cfg.CreateMap<Models.BookForUpdateDto, Book>();
                    cfg.CreateMap<Book, Models.BookForUpdateDto>();
                });

            // formember is called projection
            // since properties in source and destination must be the same for the mapping
            // but since we need in dto class current age rather than dateofbirth and first+lastname of
            // author, we used formember for mapping without error


            libraryContext.EnsureSeedDataForContext();

            app.UseMvc(); 
        }
    }
}
