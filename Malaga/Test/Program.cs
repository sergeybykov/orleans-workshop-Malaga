using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interface;
using Orleans;
using Orleans.Runtime;
using Orleans.Runtime.Configuration;
using Orleans.Runtime.Host;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var siloConfig = ClusterConfiguration.LocalhostPrimarySilo();
            siloConfig.Defaults.DefaultTraceLevel = Severity.Warning;
            var silo = new SiloHost("Test Silo", siloConfig);
            silo.InitializeOrleansSilo();
            silo.StartOrleansSilo();

            Console.WriteLine("Silo started.");

            var clientConfig = ClientConfiguration.LocalhostSilo();
            var client = new ClientBuilder().UseConfiguration(clientConfig).Build();
            client.Connect().Wait();

            Console.WriteLine("Client connected.");

            Test(client).Wait();

            Console.WriteLine("Press Enter to close.");
            Console.ReadLine();
        }

        public static async Task Test(IClusterClient client)
        {
            var mark = client.GetGrain<IUser>("mark@fb.com");
            await mark.SetName("Mark");
            await mark.SetStatus("Share your life with me!");

            var jack = client.GetGrain<IUser>("jack@twitter.com");
            await jack.SetName("Jack");
            await jack.SetStatus("Tweet me!");

            var props = await mark.GetProperties();
            Console.WriteLine($"Mark: {props}");
            props = await jack.GetProperties();
            Console.WriteLine($"Jack: {props}");
        }
    }
}
