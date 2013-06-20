using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace WorkerRole1
{
    /// <summary>
    /// Read COGOW datafiles and convert to Azure Table Entities in json format
    /// </summary>
    public class WorkerRole : RoleEntryPoint
    {
        class AzureWindRoseEntity : TableEntity
        {
            public AzureWindRoseEntity(string time, string latlon)
            {
                this.PartitionKey = time;
                this.RowKey = latlon;
            }

            public AzureWindRoseEntity()
            {
            }

            public string Json { get; set; }
        }

        class DirectionAndValues
        {
            public string Dir;
            public double[] St;
        }
        class WindRoseEntity
        {
            public double[] LL;
            public string T;
            public List<DirectionAndValues> Rose;
        }


        // http://www.cioss.coas.oregonstate.edu/cogow/0101/txt/0.75S_99.75E.txt
        private string[] times = { "0101", "0116", "0201", "0216", "0301", "0316", "0401", "0416", "0501", "0516", "0601", "0616", "0701", "0716", "0801", "0816", "0901", "0916", "1001", "1016", "1101", "1116", "1201", "1216" };
        private string rootPath = @"http://www.cioss.coas.oregonstate.edu/cogow/";

        public override void Run()
        {
            Trace.TraceInformation("WorkerRole1 entry point called", "Information");

            //CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
            //    CloudConfigurationManager.GetSetting("StorageConnectionString"));

            // with connection in app.config, this works!!!
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                CloudConfigurationManager.GetSetting("StorageConnectionString"));

            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Create the table if it doesn't exist.
            CloudTable table = tableClient.GetTableReference("windrose");
            table.CreateIfNotExists();

            foreach (var time in times)
            {
                for (double y = 79.75; y >= -79.75; y -= 0.5)
                //for (double y = -0.25; y >= -79.75; y -= 0.5)
                {
                    int countAtThisLat = 0;
                    int maxLon = 360*2;
                    Parallel.For(0, maxLon, i =>
                    {
                        double x = (i * 0.5) + 0.25;
                        bool southern = y < 0;
                        string timeString = string.Format(@"{0}/txt/", time);
                        string filename = string.Format("{0:0.00;0.00}{1}_{2:0.00}E.txt", y,
                                                        southern ? "S" : "N", x);
                        string fullpath = rootPath + timeString + filename;
                        WebClient wc = new WebClient();
                        try
                        {
                            var blob = wc.DownloadString(fullpath);
                            string[] lines = blob.Split('\n');

                            var latlonLine = lines.Skip(1).Take(1).Single();
                            var latlonLineSplit = latlonLine.Split(' ');
                            string s;
                            s = latlonLineSplit[1];
                            bool latN = (s[s.Length - 1] == 'N');
                            s = s.Substring(0, s.Length - 1);
                            double lat = double.Parse(s)*(latN ? 1 : -1);
                            s = latlonLineSplit[3];
                            s = s.Substring(0, s.Length - 1);
                            double lon = double.Parse(s);
                            if (lon > 180)
                            {
                                lon -= 360;
                            }

                            double[] ll = {lat, lon};
                            string llS = string.Format(@"{0:+000.00;-000.00}_{1:+000.00;-000.00}", lat, lon);

                            var wantedLines = lines.Skip(5).Take(16);
                            List<DirectionAndValues> newLines = new List<DirectionAndValues>(16);
                            foreach (var line in wantedLines)
                            {
                                var segmentedLine = Regex.Split(line, @"\W+\D+");

                                DirectionAndValues d = new DirectionAndValues();
                                newLines.Add(d);
                                d.Dir = segmentedLine[0];
                                d.St = new double[6];
                                for (int g = 0; g < 6; g++)
                                {
                                    d.St[g] = double.Parse(segmentedLine[g + 1]);
                                }
                            }

                            WindRoseEntity rose = new WindRoseEntity();
                            rose.T = time;
                            rose.LL = ll;
                            rose.Rose = newLines;
                            string json = JsonConvert.SerializeObject(rose);

                            AzureWindRoseEntity azureWindRose = new AzureWindRoseEntity(time, llS);
                            azureWindRose.Json = json;

                            TableOperation insertOrReplaceOperation = TableOperation.InsertOrReplace(azureWindRose);

                            // Execute the insertOrReplace operation.
                            table.Execute(insertOrReplaceOperation);

                            Interlocked.Increment(ref countAtThisLat);
                           // Debug.WriteLine(filename);
                        }
                        catch (Exception ex)
                        {
                            string s = ex.Message + " " + filename;
                            // Debug.WriteLine(s);
                            Trace.TraceInformation("Failed", "s");
                        }
                    });
                    string s1 = string.Format("TOTAL AT {0} = {1}", y, countAtThisLat);
                    Debug.WriteLine(s1);
                    Trace.TraceInformation(s1);
                }
               
                Trace.TraceInformation("FINISHED date " + time, "Information");

            }  // for each time period

            Trace.TraceInformation("FINISHED ALL", "Information");

        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections 
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

            return base.OnStart();
        }
    }
}
