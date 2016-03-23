using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nucleus;
using System.Reflection;
using System.IO;
using Shouldly;
using System.Diagnostics;

namespace Nucleus.Tests
{
    partial class Program
    {
        static Connection cx;
        static string file;
        static bool isNew;

        static void Main(string[] args)
        {
            file = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"nucleus.db");
            isNew = !File.Exists(file);

            using (cx = new Connection(file))
            {
                if (isNew)
                {
                    PopulateQuery();
                    PopulateDictionaryQuery();
                    PopulateDynamicQuery();
                }

                TestQuery();
                TestDictionaryQuery();
                TestDynamicQuery();
                TestParallelModifications();
            }

            Benchmark();
            Console.ReadKey();
        }

        static void TestQuery()
        {
            using (Query<DateTime> dic = cx.Query<DateTime>("timestamps"))
            {
                dic.ShouldSatisfyAllConditions(
                    () => dic.Count.ShouldBeGreaterThan(0),
                    () => dic[0].ShouldBeLessThan(DateTime.Now),
                    () => dic[0].ShouldBeGreaterThan(DateTime.MinValue)
                );
            }
        }

        static void TestDictionaryQuery()
        {
            using (DictionaryQuery<string> dic = cx.DictionaryQuery<string>("assembly"))
            {
                dic.ShouldSatisfyAllConditions(
                    () => dic.ShouldContainKey("done"),
                    () => dic.ShouldContainKey("Name"),
                    () => dic["Name"].ShouldBe(Assembly.GetExecutingAssembly().FullName)
                );
            }
        }

        static void TestDynamicQuery()
        {
            using (DynamicDictionaryQuery dic = cx.DictionaryQuery("dynam"))
            {
                dic.ShouldSatisfyAllConditions(
                    () => dic.Contains<string>("random").ShouldBeTrue(),
                    () => dic.Get<string>("random").ShouldNotBeNullOrWhiteSpace(),
                    () => dic.Contains<DateTime>("random").ShouldBeTrue()
                );
            }
        }

        static void TestParallelModifications()
        {
            using (DictionaryQuery<string> dic = cx.DictionaryQuery<string>("parallel"))
            {
                dic.Add("hello", "world");
                dic["hello"].ShouldBe("world");

                using (DictionaryQuery<string> pdic = cx.DictionaryQuery<string>("parallel"))
                {
                    pdic["hello"] = "you";
                }

                dic["hello"].ShouldBe("you");
                dic.Clear();
            }
        }

        #region Populate
        static void PopulateQuery()
        {
            using (Query<DateTime> dic = cx.Query<DateTime>("timestamps"))
            {
                Debug.WriteLine("Adding creation date to 'timestamps'.");

                dic.Add(DateTime.Now);
            }
        }

        static void PopulateDictionaryQuery()
        {
            using (DictionaryQuery<string> dic = cx.DictionaryQuery<string>("assembly"))
            {
                Debug.WriteLine("Adding assembly informations to 'assembly'.");

                Assembly a = Assembly.GetExecutingAssembly();
                dic.Add("Name", a.FullName);
                dic.Add("Version", a.GetName().Version.ToString());
                dic.Add("done", "");
            }
        }

        static void PopulateDynamicQuery()
        {
            using (DynamicDictionaryQuery dic = cx.DictionaryQuery("dynam"))
            {
                Debug.WriteLine("Adding random values to 'dynam'.");

                dic.Set<string>("random", Guid.NewGuid().ToString());
                dic.Set<DateTime>("random", DateTime.MinValue.AddMinutes(new Random().Next(1000000)));
            }
        }
        #endregion
    }
}
