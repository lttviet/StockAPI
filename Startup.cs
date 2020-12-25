using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using StockBE.Services;

namespace StockBE
{
  public class Startup
  {
    private readonly string LocalCorsPolicy = "LocalCorsPolicy";

    // This method gets called by the runtime. Use this method to add services to the container.
    // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    public void ConfigureServices(IServiceCollection services)
    {
      services.AddCors(options =>
      {
        options.AddPolicy(LocalCorsPolicy, builder =>
        {
          builder.WithOrigins("http://localhost:4200")
            .AllowAnyMethod()
            .AllowAnyHeader();
        });
      });

      services.AddHostedService<QuoteService>();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
      }

      app.UseRouting();
      app.UseCors(LocalCorsPolicy);

      app.UseEndpoints(endpoints =>
      {
        endpoints.MapGet("/", async context =>
          {
            await context.Response.WriteAsync("Hello World!");
          });
        endpoints.MapGet("/done", async context =>
          {
            await context.Response.WriteAsync("done");
          });
      });
    }
  }
}
