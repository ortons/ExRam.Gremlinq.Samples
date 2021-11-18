﻿using ExRam.Gremlinq.Core.AspNet;
using ExRam.Gremlinq.Samples.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ExRam.Gremlinq.Samples.AspNet {
    public class Startup {
        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {
          //  services.AddGremlinq(setup => setup.UseCosmosDb<Vertex, Edge>(x => x.PartitionKey)).AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            app.UseRouting().UseAuthorization().UseEndpoints(endpoints => {
                endpoints.MapControllers();
            });
        }
    }
}
