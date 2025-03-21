﻿namespace Metrology
{
    public partial struct Radian
    {
        #region Math (external)
        public static double Sin(Radian angle) => System.Math.Sin(angle.Value);
        public static double Cos(Radian angle) => System.Math.Cos(angle.Value);
        public static Radian Atan(double x) => new(System.Math.Atan(x));
        #endregion
    }
}
