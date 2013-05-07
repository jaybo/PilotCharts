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

        private static string file0P25 = @"Out.0.25.txt";
        private static string file0P5 = @"Out.0.5.txt";
        private static string file1P0 = @"Out.1.0.txt";
        private static string file2P0 = @"Out.2.0.txt";
        private static string file4P0 = @"Out.4.0.txt";
        private static string file8P0 = @"Out.8.0.txt";
        private static string file16P0 = @"Out.16.0.txt";
        private static string noData = "-9999";

        private static void WritePoint(StreamWriter sw, string lat, string lon, double a)
        {
            string s = string.Format("POINT_SYMBOL=Arrow@ANGLE={0}", (int)a);
            sw.WriteLine(s);
            s = string.Format("{0}, {1}", lat, (double.Parse(lon)));
            sw.WriteLine(s);
        }

        private static bool CloseMod(double a, double b)
        {
            var a1 = Math.Floor(a);
            var b1 = Math.Floor(b);
            var aFrac = a - a1;
            var bFrac = b - b1;
            var iMatch = b < 1 ? a >= 1 : (int) a%(int) b == 0;               // integer portion matches
            var fMatch = Math.Abs(aFrac - bFrac) < .001;                    // fractional portion matches
            ;
            if (b < 1.0)
            {
                return fMatch;
            }
            else
            {
                return (fMatch) && iMatch;
            }
        }

        static void Main(string[] args)
        {
            var dir = Directory.GetCurrentDirectory();

            var U = File.ReadAllLines(Path.Combine(dir, backPath, UFile));
            var V = File.ReadAllLines(Path.Combine(dir, backPath, VFile));

            var o0P25 = new StreamWriter(Path.Combine(dir, backPath, file0P25), false);
            var o0P5 = new StreamWriter(Path.Combine(dir, backPath, file0P5), false);
            var o1P0 = new StreamWriter(Path.Combine(dir, backPath, file1P0), false);
            var o2P0 = new StreamWriter(Path.Combine(dir, backPath, file2P0), false);
            var o4P0 = new StreamWriter(Path.Combine(dir, backPath, file4P0), false);
            var o8P0 = new StreamWriter(Path.Combine(dir, backPath, file8P0), false);
            var o16P0 = new StreamWriter(Path.Combine(dir, backPath, file16P0), false);
                
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
                var slat = rNum.Match(match).Value;
                var lat = double.Parse(slat);
                Debug.WriteLine(slat);

                var lonsCount = lons.Count();
                for (var x = 1; x < lonsCount; x++)
                {
                    if (eU[x].Equals(noData)) continue;
                    if (eV[x].Equals(noData)) continue;
                    var slon = lons[x];
                    var lon = double.Parse(slon);
                    var u = eU[x];
                    var v = eV[x];
                    var a = 180 + Math.Atan2(double.Parse(u), double.Parse(v))*180.0/Math.PI;

                    if (lat == 0.375) {int j = 1;}

                    var bLon0P25 = CloseMod(lon, .375);
                    var bLon0P5 = CloseMod(lon, .875) || bLon0P25;
                    var bLon1P0 = CloseMod(lon, 1.375);
                    var bLon2P0 = CloseMod(lon, 2.375);
                    var bLon4P0 = CloseMod(lon, 4.375);
                    var bLon8P0 = CloseMod(lon, 8.375);
                    var bLon16P0 = CloseMod(lon, 16.375);

                    // to keep even spacing of samples, below the equator, use different values
                    var bLat0P25 = CloseMod(lat, lat > 0 ? .375 : .125);
                    var bLat0P5 = CloseMod(lat, lat > 0 ? .875 : .625) || bLat0P25;
                    var bLat1P0 = CloseMod(lat, lat > 0 ? 1.375 : 1.125);
                    var bLat2P0 = CloseMod(lat, lat > 0 ? 2.375 : 2.125);
                    var bLat4P0 = CloseMod(lat, lat > 0 ? 4.375 : 4.125);
                    var bLat8P0 = CloseMod(lat, lat > 0 ? 8.375 : 8.125);
                    var bLat16P0 = CloseMod(lat, lat > 0 ? 16.375 : 16.125);

                    // Debug.WriteLine(lon.ToString() + " " + b1.ToString() + b2.ToString() + b3.ToString());
                    WritePoint(o0P25, slat, slon, a);
                    if (bLon0P5 && bLat0P5) WritePoint(o0P5, slat, slon, a);
                    if (bLon1P0 && bLat1P0) WritePoint(o1P0, slat, slon, a);
                    if (bLon2P0 && bLat2P0) WritePoint(o2P0, slat, slon, a);
                    if (bLon4P0 && bLat4P0) WritePoint(o4P0, slat, slon, a);
                    if (bLon8P0 && bLat8P0) WritePoint(o8P0, slat, slon, a);
                    if (bLon16P0 && bLat16P0) WritePoint(o16P0, slat, slon, a);

                }
            }

            o0P25.Flush();
            o0P25.Close();
            o0P5.Flush();
            o0P5.Close();
            o1P0.Flush();
            o1P0.Close();
            o2P0.Flush();
            o2P0.Close();
            o4P0.Flush();
            o4P0.Close();
            o8P0.Flush();
            o8P0.Close();
            o16P0.Flush();
            o16P0.Close();
        }
    }
}
