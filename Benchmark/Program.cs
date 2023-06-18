using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using CALINE3;
using System.Numerics;
using System.Reflection.Metadata;

namespace Benchmark
{
    public class Sample
    {
        // The following (unnecessary) Params declaration(s) makes
        // BenchmarkDotNet reporting the DIMENSIONAL_ANALYSIS symbol status:
#if DIMENSIONAL_ANALYSIS
        [Params("On")]
        public string DimensionalAnalysis { get; set; } = "On";
#else
        [Params("Off")]
        public string DimensionalAnalysis { get; set; } = "Off";
#endif

        private const string INPUT_DATA = @"EXAMPLE FOUR                             60.100.   0.   0.12        1.
RECP. 1                  -350.       30.       1.8
RECP. 2                     0.       30.       1.8
RECP. 3                   750.      100.       1.8
RECP. 4                   850.       30.       1.8
RECP. 5                  -850.     -100.       1.8
RECP. 6                  -550.     -100.       1.8
RECP. 7                  -350.     -100.       1.8
RECP. 8                    50.     -100.       1.8
RECP. 9                   450.     -100.       1.8
RECP. 10                  800.     -100.       1.8
RECP. 11                 -550.       25.       1.8
RECP. 12                 -550.       25.       6.1
URBAN LOCATION: MULTIPLE LINKS, ETC.      6  4
LINK A              AG   500.     0.  3000.     0.   9700. 30.  0. 23.
LINK B              DP   500.     0.  1000.   100.   1200.150. -2. 13.
LINK C              AG -3000.     0.   500.     0.  10900. 30.  0. 23.
LINK D              AG -3000.   -75.  3000.   -75.   9300. 30.  0. 23.
LINK E              BR  -500.   200.  -500.  -300.   4000. 50. 6.1 27.
LINK F              BR  -100.   200.  -100.  -200.   5000. 50. 6.1 27.
 1.  0.6 1000.12.0
 1. 90.6 1000. 7.0
 1.180.6 1000. 5.0
 1.270.6 1000. 6.7
";
        private readonly Job job;

        public Sample()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture =
                System.Globalization.CultureInfo.InvariantCulture;

            using (System.IO.StringReader input = new(INPUT_DATA))
            {
                JobReader rdr = new(input);
                job = rdr.Read()!;
            }
        }

        [Benchmark]
        public void CALINE3()
        {
            foreach (var meteo in job.Meteos)
            {
                // Mass concentration matrix
                Microgram_Meter3[][] MC = new Microgram_Meter3[job.Links.Count][];

                foreach (var link in job.Links)
                {
                    MC[link.ORDINAL] = new Microgram_Meter3[job.Receptors.Count];

                    // Gaussian plume calculator
                    Plume plume = new(job, meteo, link);

                    foreach (var receptor in job.Receptors)
                    {
                        MC[link.ORDINAL][receptor.ORDINAL] = plume.ConcentrationAt(receptor);
                    }
                }
            }
        }
    }
    public class Program
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<Sample>();
        }
    }
}

/*
 * SAMPLE RESULTS (DIMESIONAL_ANALYSIS = OFF) ***********************************
 * 
 * BenchmarkDotNet=v0.13.5, OS=Windows 11 (10.0.22621.1848/22H2/2022Update/SunValley2)
 * 11th Gen Intel Core i7-1165G7 2.80GHz, 1 CPU, 8 logical and 4 physical cores
 * .NET SDK=7.0.304
 *   [Host]     : .NET 7.0.7 (7.0.723.27404), X64 RyuJIT AVX2
 *   DefaultJob : .NET 7.0.7 (7.0.723.27404), X64 RyuJIT AVX2
 * 
 * |  Method | DimensionalAnalysis |     Mean |     Error |    StdDev |
 * |-------- |-------------------- |---------:|----------:|----------:|
 * | CALINE3 |                 Off | 1.109 ms | 0.0118 ms | 0.0110 ms |
 * 
 * Outliers
 *   Sample.CALINE3: Default -> 1 outlier  was  detected (1.08 ms)
 * 
 * Legends
 *   DimensionalAnalysis : Value of the 'DimensionalAnalysis' parameter
 *   Mean                : Arithmetic mean of all measurements
 *   Error               : Half of 99.9% confidence interval
 *   StdDev              : Standard deviation of all measurements
 *   1 ms                : 1 Millisecond (0.001 sec)
 * 
 * Run time: 00:00:14 (14.2 sec), executed benchmarks: 1
 * Global total time: 00:00:19 (19.69 sec), executed benchmarks: 1
 * 
 * 
 * 
 * SAMPLE RESULTS (DIMESIONAL_ANALYSIS = ON) ***********************************
 * 
 * BenchmarkDotNet=v0.13.5, OS=Windows 11 (10.0.22621.1848/22H2/2022Update/SunValley2)
 * 11th Gen Intel Core i7-1165G7 2.80GHz, 1 CPU, 8 logical and 4 physical cores
 * .NET SDK=7.0.304
 *   [Host]     : .NET 7.0.7 (7.0.723.27404), X64 RyuJIT AVX2
 *   DefaultJob : .NET 7.0.7 (7.0.723.27404), X64 RyuJIT AVX2
 * 
 * 
 * |  Method | DimensionalAnalysis |     Mean |     Error |    StdDev |
 * |-------- |-------------------- |---------:|----------:|----------:|
 * | CALINE3 |                  On | 1.244 ms | 0.0208 ms | 0.0194 ms |
 * 
 * Legends
 *   DimensionalAnalysis : Value of the 'DimensionalAnalysis' parameter
 *   Mean                : Arithmetic mean of all measurements
 *   Error               : Half of 99.9% confidence interval
 *   StdDev              : Standard deviation of all measurements
 *   1 ms                : 1 Millisecond (0.001 sec)
 * 
 * Run time: 00:00:17 (17.15 sec), executed benchmarks: 1
 * Global total time: 00:00:22 (22.22 sec), executed benchmarks: 1
 * 
 * PERFORMANCE RATIO ***********************************************************
 *
 *   1.244 ms (dimesional analysis ON)
 *   ---------------------------------- ≈ 1,12
 *   1.109 ms (dimesional analysis OFF)
 *
 * *****************************************************************************
 * 
 */
