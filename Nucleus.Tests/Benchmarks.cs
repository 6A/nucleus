using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Raven.Server;
using Raven.Client;
using Raven.Client.Embedded;

namespace Nucleus.Tests
{
    partial class Program
    {
        public class DataDocumentStore
        {
            private static IDocumentStore instance;

            public static IDocumentStore Instance
            {
                get
                {
                    if (instance == null)
                        throw new InvalidOperationException(
                          "IDocumentStore has not been initialized.");
                    return instance;
                }
            }

            public static IDocumentStore Initialize()
            {
                instance = new EmbeddableDocumentStore { DataDirectory = @"D:\Raven", UseEmbeddedHttpServer = true };
                instance.Conventions.IdentityPartsSeparator = "-";
                instance.Initialize();
                return instance;
            }
        }

        public class User
        {
            public string Username { get; set; }
            public string Password { get; set; }

            public User()
            {
                Username = Guid.NewGuid().ToString();
                Password = Guid.NewGuid().ToString();
            }
        }

        static void Benchmark()
        {
            long nucleusInit, ravendbInit, nucleusWrite, ravendbWrite, nucleusRead, ravendbRead;

            Stopwatch sw = new Stopwatch();

            sw.Start();
            var nucleus = new Connection(@"D:\nucleus.db");
            sw.Stop();
            nucleusInit = sw.ElapsedMilliseconds;

            sw.Restart();
            DataDocumentStore.Initialize();
            sw.Stop();
            ravendbInit = sw.ElapsedMilliseconds;

            sw.Restart();
            var nquery = nucleus.Query<User>("users");
            for (int i = 0; i < 1000; i++)
            {
                nquery.Add(new User());
            }
            nquery.Save();
            sw.Stop();
            nucleusWrite = sw.ElapsedMilliseconds;

            sw.Restart();
            var rquery = DataDocumentStore.Instance.OpenSession();
            for (int i = 0; i < 1000; i++)
            {
                rquery.Store(new User());
            }
            rquery.SaveChanges();
            sw.Stop();
            ravendbWrite = sw.ElapsedMilliseconds;

            sw.Restart();
            List<User> nusers = nquery.Take(256).ToList();
            nquery.Dispose();
            sw.Stop();
            nucleusRead = sw.ElapsedMilliseconds;

            sw.Restart();
            List<User> rusers = rquery.Query<User>().Take(256).ToList();
            DataDocumentStore.Instance.Dispose();
            sw.Stop();
            ravendbRead = sw.ElapsedMilliseconds;

            Console.WriteLine("Nucleus initialized in {0} milliseconds.", nucleusInit);
            Console.WriteLine("RavenDB initialized in {0} milliseconds.", ravendbInit);
            Console.WriteLine("Nucleus added 1000 users in {0} milliseconds.", nucleusWrite);
            Console.WriteLine("RavenDB added 1000 users in {0} milliseconds.", ravendbWrite);
            Console.WriteLine("Nucleus read {1} users in {0} milliseconds.", nucleusRead, nusers.Count);
            Console.WriteLine("RavenDB read {1} users in {0} milliseconds.", ravendbRead, rusers.Count);

            rusers.Clear();
            nusers.Clear();
        }
    }
}
