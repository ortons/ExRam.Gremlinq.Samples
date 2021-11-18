//#define GremlinServer
//#define CosmosDB

#define AWSNeptune
//#define JanusGraph

// Put this into static scope to access the default GremlinQuerySource as "g". 
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ExRam.Gremlinq.Core;
using ExRam.Gremlinq.Providers.WebSocket;
using ExRam.Gremlinq.Samples.Shared;
using Microsoft.Extensions.Logging;
using static ExRam.Gremlinq.Core.GremlinQuerySource;

namespace ExRam.Gremlinq.Samples {
    public class Program {
        private static Uri CreateUri(string hostname, int port, bool enableSsl) {
            var scheme = enableSsl ? "wss" : "ws";
            return new Uri($"{scheme}://{hostname}:{port}");
        }

        private static async Task Main() {
            
            
            
            var gremlinQuerySource = CreateDbConnection();


            //
            // List<Task> tasks = new List<Task>();
            //
            // for (int i = 0; i < 100; i++) {
            //     var task = Task.Factory.StartNew(() => {
            //         var id =  IdGenerator.GetId();
            //         Console.WriteLine(id);
            //     });
            //     tasks.Add(task);
            // }
            //
            //
            // Task.WaitAll(tasks.ToArray());
            //

            bool justQuery = true;
            
            
            if (justQuery)
                await new Logic(gremlinQuerySource, Console.Out).RunJustQueries();
            else
                await new Logic(gremlinQuerySource, Console.Out).Run();
            
        }

        
        
        private static IGremlinQuerySource CreateDbConnection() {
            var dbUri = CreateUri("nxt-sanity-test.cluster-ca8s0hjhsaye.eu-west-1.neptune.amazonaws.com", 8182, true);


            var gremlinQuerySource = g.ConfigureEnvironment(env => env //We call ConfigureEnvironment twice so that the logger is set on the environment from now on.
                .UseLogger(LoggerFactory.Create(builder => builder.AddFilter(__ => true).AddConsole()).CreateLogger("Queries"))).ConfigureEnvironment(env => env
                .UseModel(GraphModel.FromBaseTypes<Vertex, Edge>(lookup => lookup.IncludeAssembliesOfBaseTypes())
                    //For CosmosDB, we exclude the 'PartitionKey' property from being included in updates.
#if CosmosDB
                        .ConfigureProperties(model => model.ConfigureElement<Vertex>(conf => conf.IgnoreOnUpdate(x => x.PartitionKey)))
#endif
                )
                //Disable query logging for a noise free console output.
                //Enable logging by setting the verbosity to anything but None.
                .ConfigureOptions(options => options.SetValue(WebSocketGremlinqOptions.QueryLogLogLevel, LogLevel.None))

#if GremlinServer
                    .UseGremlinServer(builder => builder
                        .AtLocalhost()));
#elif AWSNeptune
                .UseNeptune(builder => builder.At(dbUri)));
#elif CosmosDB
                    .UseCosmosDb(builder => builder
                        .At(new Uri("wss://your_gremlin_endpoint.gremlin.cosmos.azure.com:443/"), "your database name", "your graph name")
                        .AuthenticateBy("your auth key")
                        .ConfigureWebSocket(_ => _
                            .ConfigureGremlinClient(client => client
                                .ObserveResultStatusAttributes((requestMessage, statusAttributes) =>
                                {
                                    //Uncomment to log request charges for CosmosDB.
                                    //if (statusAttributes.TryGetValue("x-ms-total-request-charge", out var requestCharge))
                                    //    env.Logger.LogInformation($"Query {requestMessage.RequestId} had a RU charge of {requestCharge}.");
                                })))));
#elif JanusGraph
                    .UseJanusGraph(builder => builder
                        .AtLocalhost()));
#endif

            return gremlinQuerySource;
        }
    }
}
