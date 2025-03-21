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
        #region Standard math methods (not related to units, but still required within the Unit of Measurement namespace).
        public static double Abs(double x) => System.Math.Abs(x);
        public static double Log(double x) => System.Math.Log(x);
        public static double Exp(double x) => System.Math.Exp(x);
        public static double Pow(double x, double y) => System.Math.Pow(x, y);
        public static double Sqrt(double x) => System.Math.Sqrt(x);
        #endregion
    }
}
