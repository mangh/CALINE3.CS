/*******************************************************************************

    Units of Measurement for C# applications

    Copyright (C) Marek Aniola

    This program is provided to you under the terms of the license
    as published at https://github.com/mangh/metrology


********************************************************************************/

namespace Metrology
{
    /// <summary>
    /// Math methods supporting the Metrology (units of measurement) library.
    /// </summary>
    public static class Math
    {
        #region Math methods that take arguments or return value of Unit of Measurement type.
        public static double Sin(Radian angle) => System.Math.Sin(angle.Value);
        public static double Cos(Radian angle) => System.Math.Cos(angle.Value);
        public static Radian Atan(double x) => new(System.Math.Atan(x));
        public static Degree Abs(Degree d) => new(System.Math.Abs(d.Value));
        public static Meter Abs(Meter d) => new(System.Math.Abs(d.Value));
        public static Meter Sqrt(Meter2 area) => new(System.Math.Sqrt(area.Value));
        public static PPM Round(PPM ppm, int digits) => new(System.Math.Round(ppm.Value, digits));

        /// <summary>
        /// Exponentiation used in power-law formulas for dispersion parameters.
        /// </summary>
        /// <param name="x">Length or distance [m].</param>
        /// <param name="b">Exponent [dimensionless].</param>
        /// <returns>
        /// Length (<paramref name="x"/>) raised to the power of <paramref name="b"/>
        /// (i.e. <paramref name="x"/>^<paramref name="b"/>), but the result is expressed in [m]!
        /// </returns>
        /// <remarks>NOTE: The function is intended to remedy dimensional inconsistency<br/>
        /// in power-law formulas of the form:
        /// <code>
        /// σ[m] = a * (x[m])^b  // a, b - constants</code>
        /// This formula is an analytical approximation of the plot of experimental data.<br/>
        /// It is assumed to return a result in [m] for the distance argument (<paramref name="x"/>) in [m],<br/>
        /// although such a dimensional relationship cannot be derived from<br/> the formula (unless b = 1)!
        /// </remarks>
        public static Meter Pow(Meter x, double b) => new(System.Math.Pow(x.Value, b));
        #endregion

        #region Standard math methods (not related to units, but still required within the Unit of Measurement namespace).
        public static double Abs(double x) => System.Math.Abs(x);
        public static double Log(double x) => System.Math.Log(x);
        public static double Exp(double x) => System.Math.Exp(x);
        public static double Pow(double x, double y) => System.Math.Pow(x, y);
        public static double Sqrt(double x) => System.Math.Sqrt(x);
        #endregion
    }
}
