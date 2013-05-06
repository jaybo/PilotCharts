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
        private static string backPath = @"..\..\..\..\";
        private static string UFile = @"JanuaryU.txt";
        private static string VFile = @"JanuaryV.txt";
        private static string outFile = @"Out.txt";
        private static string noData = "-9999";

        private static void WritePoint(StreamWriter sw, string lat, string lon, double a)
        {
            string s = string.Format("POINT_SYMBOL=Arrow@ANGLE={0}", (int)a);
            sw.WriteLine(s);
            s = string.Format("{0}, {1}", lat, (double.Parse(lon)));
            sw.WriteLine(s);
        }

        static void Main(string[] args)
        {
            var dir = Directory.GetCurrentDirectory();

            var U = File.ReadAllLines(Path.Combine(dir, backPath, UFile));

            var V = File.ReadAllLines(Path.Combine(dir, backPath, VFile));
            var output = new StreamWriter(Path.Combine(dir, backPath, outFile), false);
                
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
                    var a = 180 + Math.Atan2(double.Parse(u), double.Parse(v))*180.0/Math.PI;
                    WritePoint(output, lat, lon, a);
                }
            }

            output.Flush();
            output.Close();
        }
    }
}
