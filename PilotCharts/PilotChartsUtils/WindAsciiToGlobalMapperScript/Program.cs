using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WindAsciiToGlobalMapperScript
{
    internal class Program
    {
        private static List<string> monthNames = DateTimeFormatInfo.CurrentInfo.MonthNames.Take(12).ToList();

        private static string backPath = @"..\..\..\..\";
        private static string UFile = @"U.txt";
        private static string VFile = @"V.txt";
        private static string WindFile = @"Wind.txt";

        private static string file0P25 = @".0.25.txt";
        private static string file0P5 = @".0.5.txt";
        private static string file1P0 = @".1.0.txt";
        private static string file2P0 = @".2.0.txt";
        private static string file4P0 = @".4.0.txt";
        private static string file8P0 = @".8.0.txt";
        private static string file16P0 = @".16.0.txt";
        private static string noData = "-9999";

        private static void WritePoint(StreamWriter sw, string lat, string lon, double a, double scale)
        {
            string s = string.Format("POINT_SYMBOL=Arrow@ANGLE={0}@SCALE={1}", (int)a, scale);
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

            foreach (var month in monthNames)
            {
               // if (month != "January") continue;

                var U = File.ReadAllLines(Path.Combine(dir, backPath, month + UFile));
                var V = File.ReadAllLines(Path.Combine(dir, backPath, month + VFile));
                var Wind = File.ReadAllLines(Path.Combine(dir, backPath, month + WindFile));

                var o0P25 = new StreamWriter(Path.Combine(dir, backPath, "out." + month + file0P25), false);
                var o0P5 = new StreamWriter(Path.Combine(dir, backPath, "out." + month + file0P5), false);
                var o1P0 = new StreamWriter(Path.Combine(dir, backPath, "out." + month + file1P0), false);
                var o2P0 = new StreamWriter(Path.Combine(dir, backPath, "out." + month + file2P0), false);
                var o4P0 = new StreamWriter(Path.Combine(dir, backPath, "out." + month + file4P0), false);
                var o8P0 = new StreamWriter(Path.Combine(dir, backPath, "out." + month + file8P0), false);
                var o16P0 = new StreamWriter(Path.Combine(dir, backPath, "out." + month + file16P0), false);

                var lons = U[1].Split(',');
                var du = U.Where(d => d.Contains("latitude")).ToList();
                var dv = V.Where(d => d.Contains("latitude")).ToList();
                var dWind = Wind.Where(d => d.Contains("latitude")).ToList();

                Regex r = new Regex(@"latitude=.*\]");
                Regex rNum = new Regex(@"-?[0-9]*\.[0-9]*");

                var lines = du.Count();
                for (int y = 2; y < lines; y++)
                {
                    var lineU = du[y].Replace(" ", "");
                    var lineV = dv[y].Replace(" ", "");
                    var lineWind = dWind[y].Replace(" ", "");
                    var eU = lineU.Split(',');
                    var eV = lineV.Split(',');
                    var eWind = lineWind.Split(',');
                    var match = r.Match(lineU).Value;
                    var slat = rNum.Match(match).Value;
                    var lat = double.Parse(slat);
                    int dlat = (int)Math.Floor(lat);
                    Debug.WriteLine(slat);

                    var lonsCount = lons.Count();
                    for (var x = 1; x < lonsCount; x++)
                    {
                        if (eU[x].Equals(noData)) continue;
                        if (eV[x].Equals(noData)) continue;
                        var slon = lons[x];
                        var lon = double.Parse(slon);
                        int dlon = (int) lon;
                        var u = eU[x];
                        var v = eV[x];
                        var a = 180 + Math.Atan2(double.Parse(u), double.Parse(v))*180.0/Math.PI;
                        var scale = Math.Max( double.Parse(eWind[x])/7, 0.8);

                        if (lat == -0.375)
                        {
                            int j = 1;
                        }
                        bool southern = dlat < 0;

                        var bLon0P25 = slon.EndsWith(".375");
                        var bLon0P5 = slon.EndsWith(".875") || bLon0P25;
                        var bLon1P0 =  bLon0P25;
                        var bLon2P0 = (dlon % 2 == 0) && bLon0P25;
                        var bLon4P0 = (dlon % 4 == 0) && bLon0P25;
                        var bLon8P0 = (dlon % 8 == 0) && bLon0P25;
                        var bLon16P0 = (dlon % 16 == 0) && bLon0P25;

                        
                        // to keep even spacing of samples, below the equator, use different values
                        var bLat0P25 = slat.EndsWith(southern ? ".875" : ".375");
                        var bLat0P5 = slat.EndsWith(southern ? ".375" : ".875") || bLat0P25;
                        var bLat1P0 = bLat0P25;
                        var bLat2P0 = (dlat % 2 == (southern ? -1 : 0)) && bLat0P25 && ((lat < -1) || (lat > 0));
                        var bLat4P0 = (dlat % 4 == (southern ? -1 : 0)) && bLat0P25 && ((lat < -1) || (lat > 0));
                        var bLat8P0 = (dlat % 8 == (southern ? -1 : 0)) && bLat0P25 && ((lat < -1) || (lat > 0));
                        var bLat16P0 = (dlat % 16 == (southern ? -1 : 0)) && bLat0P25 && ((lat < -1) || (lat > 0));

                        //var bLat2P0 = (dlat % 2 == 0) && bLat0P25 && ((dlat < -1) || (dlat > 0));
                        //var bLat4P0 = (dlat % 4 == 0) && bLat0P25 && ((dlat < -1) || (dlat > 0));
                        //var bLat8P0 = (dlat % 8 ==0) && bLat0P25 && ((dlat < -1) || (dlat > 0));
                        //var bLat16P0 = (dlat % 16 == 0) && bLat0P25 && ((dlat < -1) || (dlat > 0));

                        // Debug.WriteLine(lon.ToString() + " " + b1.ToString() + b2.ToString() + b3.ToString());
                        WritePoint(o0P25, slat, slon, a, scale);
                        if (bLon0P5 && bLat0P5) WritePoint(o0P5, slat, slon, a, scale);
                        if (bLon1P0 && bLat1P0) WritePoint(o1P0, slat, slon, a, scale);
                        if (bLon2P0 && bLat2P0) WritePoint(o2P0, slat, slon, a, scale);
                        if (bLon4P0 && bLat4P0) WritePoint(o4P0, slat, slon, a, scale);
                        if (bLon8P0 && bLat8P0) WritePoint(o8P0, slat, slon, a, scale);
                        if (bLon16P0 && bLat16P0) WritePoint(o16P0, slat, slon, a, scale);
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
            }  // for each month
        }
    }
}
