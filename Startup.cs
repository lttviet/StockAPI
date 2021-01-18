using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

using StockBE.Hubs;
using StockBE.Services;
using StockBE.DataAccess;

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
            .AllowAnyHeader()
            .AllowCredentials();
        });
      });

      services.AddSwaggerGen(c =>
      {
        c.SwaggerDoc(
          "v1",
          new OpenApiInfo { Title = "Broker API", Version = "v1" }
        );
      });

      services.AddControllers();
      services.AddSingleton<BrokerDataAccess>();
      services.AddSingleton<QuoteClient>();
      services.AddHostedService<BrokerSubscriptionWorker>();
      services.AddHostedService<QuoteSubscriptionWorker>();
      services.AddSignalR();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
      }

      app.UseSwagger();
      app.UseSwaggerUI(c =>
      {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Broker");
      });
      app.UseHttpsRedirection();
      app.UseCors(LocalCorsPolicy);
      app.UseRouting();

      app.UseEndpoints(endpoints =>
      {
        endpoints.MapControllers();
        endpoints.MapHub<BrokerHub>("/brokerhub");
        endpoints.MapHub<QuoteHub>("/quotehub");
      });
    }
  }
}
