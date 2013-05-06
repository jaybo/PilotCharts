using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WindAsciiToGlobalMapperScript
{
    class Program
    {
        private static string UFile = @"\DeepZoom\JanuaryU.txt";
        private static string VFile = @"\DeepZoom\JanuaryV.txt";
        private static string outFile = @"\DeepZoom\Out.gms";
        private static string noData = "-9999";

        private static void WritePoint(StreamWriter sw, string lat, string lon, double a)
        {
            string s = string.Format(@"Symbol=WindArrow@Angle=%d", (int) a);
            sw.WriteLine(s);
            s = string.Format(@"%s, %s", lat, lon);
            sw.WriteLine(s);
        }

        static void Main(string[] args)
        {
            var U = File.ReadAllLines(UFile);
            var V = File.ReadAllLines(VFile);
            var output = new StreamWriter(outFile,false);
            output.WriteLine("GLOBAL_MAPPER_SCRIPT VERSION=1.00");
                
            var lons = U[1].Split(',');
            var du = U.Where(d => d.Contains("latitude")).ToList();
            var dv = V.Where(d => d.Contains("latitude")).ToList();

            Regex r = new Regex(@"latitude=.*\]");
            Regex rNum = new Regex(@"-?[0-9]*\.[0-9]*");

            var lines = du.Count();
            for (int y = 2; y < lines; y++)
            {
                var lineU = du[y].Replace(" ", "");
                var lineV = dv[y].Replace(" ", ""); 
                var eU = lineU.Split(',');
                var eV = lineV.Split(',');
                var match = r.Match(lineU).Value;
                var lat = rNum.Match(match).Value;
                Debug.WriteLine(lat);

                var lonsCount = lons.Count();
                for (var x = 1; x < lonsCount; x++)
                {
                    if (eU[x].Equals(noData)) continue;
                    if (eV[x].Equals(noData)) continue;
                    var lon = lons[x];
                    var u = eU[x];
                    var v = eV[x];
                    var a = Math.Atan2(double.Parse(v), double.Parse(u))*Math.PI/2;
                    WritePoint(output, lat, lon, a);
                }
            }

            output.Flush();
            output.Close();
        }
    }
}
