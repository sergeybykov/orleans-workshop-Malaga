﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
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

            var ok = await mark.AddFriend(jack);
            if(ok)
                Console.WriteLine("Mark added Jack as a friend.");

            var sw = Stopwatch.StartNew();

            for (int i = 1; i <= 100; i++)
            {
                var user = client.GetGrain<IUser>($"user{i}@outlook.com");
                await user.SetName($"User #{i}");
                await user.SetStatus((i % 3 == 0) ? "Sad" : "Happy");
                await ((i % 2 == 0) ? mark : jack).AddFriend(user);
                //var p = await user.GetProperties();
                //Console.WriteLine($"{p}");
            }

            sw.Stop();

            Console.WriteLine($"Serial elapsed: {sw.ElapsedMilliseconds}");

            sw.Restart();
            var tasks = new List<Task>();
            for (int j = 101; j <= 200; j++)
            {
                var user = client.GetGrain<IUser>($"user{j}@outlook.com");
                tasks.Add(user.SetName($"User #{j}"));
                tasks.Add(user.SetStatus((j % 3 == 0) ? "Sad" : "Happy"));
                tasks.Add(((j % 2 == 0) ? mark : jack).AddFriend(user));
                //var p = await user.GetProperties();
            }

            await Task.WhenAll(tasks);

            sw.Stop();


            Console.WriteLine($"Parallel elapsed: {sw.ElapsedMilliseconds}");
        }
    }
}
