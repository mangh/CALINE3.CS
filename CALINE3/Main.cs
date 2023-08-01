/*******************************************************************************

    Units of Measurement for C# applications applied to
    the CALINE3 Model algorithm.

    For more information on CALINE3 and its status see:
    • https://www.epa.gov/scram/air-quality-dispersion-modeling-alternative-models#caline3
    • https://www.epa.gov/scram/2017-appendix-w-final-rule.

    Copyright (C) mangh

    This program is provided to you under the terms of the license 
    as published at https://github.com/mangh/metrology.

********************************************************************************/
using System;
using System.Diagnostics;

using static System.Console;

namespace CALINE3
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length < 1)
            {
                WriteLine("Missing or invalid command line arguments");
                WriteLine($"Usage: {typeof(Program).Assembly.GetName().Name} /path/to/input.data");
                return 1;
            }

            System.Threading.Thread.CurrentThread.CurrentCulture = 
                System.Globalization.CultureInfo.InvariantCulture;

            TimeSpan totalElapsedTime = new(0L);

            using (System.IO.StreamReader input = new(args[0]))
            {
                JobReader rdr = new(input);

                Job? site;
                while ((site = rdr.Read()) is not null)
                {
                    TimeSpan jobElapsedTime = new(0L);

                    foreach (var meteo in site.Meteos)
                    {
                        long startTime = Stopwatch.GetTimestamp();

                        // Mass concentration matrix
                        Microgram_Meter3[][] MC = new Microgram_Meter3[site.Links.Count][];

                        foreach (var link in site.Links)
                        {
                            MC[link.ORDINAL] = new Microgram_Meter3[site.Receptors.Count];

                            // Gaussian plume calculator
                            Plume plume = new(site, meteo, link);

                            foreach (var receptor in site.Receptors)
                            {
                                MC[link.ORDINAL][receptor.ORDINAL] = plume.ConcentrationAt(receptor);
                            }
                        }

                        jobElapsedTime += Stopwatch.GetElapsedTime(startTime);

                        Report.Output(site, meteo, MC);
                    }
                    totalElapsedTime += jobElapsedTime;

                    WriteLine();
                    WriteLine($"Job computation time (excl. I/O): {jobElapsedTime.TotalMicroseconds:F0} µs :: {site.JOB} :: {site.RUN}");
                    WriteLine();
                }
            }

            WriteLine($"Total computation time (excl. I/O): {totalElapsedTime.TotalMicroseconds:F0} µs.");

            return 0;
        }
    }
}
