using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interface;
using Orleans;
using Orleans.Runtime;
using Orleans.Runtime.Configuration;
using Orleans.Runtime.Host;
using OrleansDashboard;

namespace Test
{
    class Program
    {
        static string connectionString = "UseDevelopmentStorage=true";

        static void Main(string[] args)
        {
            var clusterId = Guid.NewGuid().ToString();
            StartAzureTableSilo(1, clusterId);
            //StartAzureTableSilo(2, clusterId);
            var client = StartAzureTableClient(clusterId);

            Test(client).Wait();

            Console.WriteLine("Press Enter to close.");
            Console.ReadLine();
        }

        public static void StartAzureTableSilo(int index, string clusterId)
        {
            var siloConfig = ClusterConfiguration.LocalhostPrimarySilo(11110+index, 29999+index);
            siloConfig.Globals.LivenessType = GlobalConfiguration.LivenessProviderType.AzureTable;
            siloConfig.Globals.ReminderServiceType = GlobalConfiguration.ReminderServiceProviderType.AzureTable;
            siloConfig.Globals.DataConnectionString = connectionString;
            siloConfig.Globals.DeploymentId = clusterId;
            siloConfig.AddAzureTableStorageProvider("Storage", connectionString);
            if(index == 1)
                siloConfig.Globals.RegisterDashboard();
            siloConfig.Defaults.DefaultTraceLevel = Severity.Warning;
            var silo = new SiloHost("Test Silo", siloConfig);
            silo.InitializeOrleansSilo();
            silo.StartOrleansSilo();

            Console.WriteLine($"Silo {index} started.");
        }

        public static IClusterClient StartAzureTableClient(string clusterId)
        {
            var clientConfig = ClientConfiguration.LocalhostSilo();
            clientConfig.GatewayProvider = ClientConfiguration.GatewayProviderType.AzureTable;
            clientConfig.DataConnectionString = connectionString;
            clientConfig.DeploymentId = clusterId;

            var client = new ClientBuilder().UseConfiguration(clientConfig).Build();
            client.Connect().Wait();

            Console.WriteLine("Client connected.");
            return client;
        }

        public static IClusterClient InitializeLocalSiloAndClient()
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
            return client;
        }

        public static async Task Test(IClusterClient client)
        {
            var mark = client.GetGrain<IUser>("mark@fb.com");
            var jack = client.GetGrain<IUser>("jack@twitter.com");

            //await PopulateUsers(client, mark, jack);

            RequestContext.Set("requestId", "xyz");
            var props = await mark.GetProperties();
            RequestContext.Set("requestId", null);

            Console.WriteLine($"Mark: {props}");
            props = await jack.GetProperties();
            Console.WriteLine($"Jack: {props}");

        }

        public static async Task PopulateUsers(IClusterClient client, IUser mark, IUser jack)
        {
            await mark.SetName("Mark");
            await mark.SetStatus("Share your life with me!");

            await jack.SetName("Jack");
            await jack.SetStatus("Tweet me!");

            await mark.AddFriend(jack);

            for (int i = 1; i <= 10; i++)
            {
                var user = client.GetGrain<IUser>($"user{i}@outlook.com");
                await user.SetName($"User #{i}");
                await user.SetStatus((i % 3 == 0) ? "Sad" : "Happy");
                await ((i % 2 == 0) ? mark : jack).AddFriend(user);
            }

            var tasks = new List<Task>();
            for (int j = 101; j <= 20; j++)
            {
                var user = client.GetGrain<IUser>($"user{j}@outlook.com");
                tasks.Add(user.SetName($"User #{j}"));
                tasks.Add(user.SetStatus((j % 3 == 0) ? "Sad" : "Happy"));
                tasks.Add(((j % 2 == 0) ? mark : jack).AddFriend(user));
            }

            await Task.WhenAll(tasks);
        }
    }
}
