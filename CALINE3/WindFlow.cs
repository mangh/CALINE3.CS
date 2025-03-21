namespace CALINE3
{
    /// <summary>
    /// Geometry of the wind flow (relative to the <see cref="Link"/>).
    /// <list type="bullet">
    /// <item><description>BASE - element growth factor,</description></item>
    /// <item><description>PHI - the angle between the wind and the link,</description></item>
    /// <item><description>TETA - the normalized angle between the wind and the link (PHI % 90),</description></item>
    /// </list>
    /// </summary>
    public readonly struct WindFlow
    {
        #region Constants
        private static readonly Degree DEG_20 = new(20.0);
        private static readonly Degree DEG_50 = new(50.0);
        private static readonly Degree DEG_70 = new(70.0);
        private static readonly Degree DEG_90 = new(90.0);
        private static readonly Degree DEG_180 = new(180.0);
        private static readonly Degree DEG_270 = new(270.0);
        private static readonly Degree DEG_360 = new(360.0);
        #endregion

        #region Properties
        /// <summary>
        /// The angle [rad] between the wind direction and the direction of the link.
        /// </summary>
        public readonly Radian PHI;

        /// <summary>
        /// The normalized angle [rad] between the wind and the roadway (that is PHI % 90 [rad]).
        /// </summary>
        public readonly Radian TETA;

        /// <summary>
        /// <see cref="LinkElement"/> growth factor [dimensionless].
        /// </summary>
        public readonly double BASE;
        #endregion

        #region Constructor(s)
        /// <summary>
        /// <see cref="WindFlow"/> constructor.
        /// </summary>
        /// <param name="meteo"><see cref="Meteo"/> conditions.</param>
        /// <param name="link"><see cref="Link"/> parameters.</param>
        public WindFlow(Meteo meteo, Link link)
        {
            Degree phi = meteo.BRG - link.LBRG;

            Degree teta = Abs(phi);
            if (teta >= DEG_270) teta = DEG_360 - teta;
            else if (teta >= DEG_180) teta -= DEG_180;
            else if (teta > DEG_90) teta = DEG_180 - teta;

#if DIMENSIONAL_ANALYSIS
            PHI = Radian.From(phi);
            TETA = Radian.From(teta);
#else
            PHI = Metrology.Radian.FromDegree(phi);
            TETA = Metrology.Radian.FromDegree(teta);
#endif
            // Set element growth base
            BASE =
                (teta < DEG_20) ? 1.1 :
                (teta < DEG_50) ? 1.5 :
                (teta < DEG_70) ? 2.0 : 4.0;
        }
        #endregion
    }
}
