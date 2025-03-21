namespace Metrology
{
    public partial struct Meter
    {
        #region Math (external)
        /// <summary>
        /// Exponentiation used in power-law formulas for dispersion parameters.
        /// </summary>
        /// <param name="x">Length or distance [m].</param>
        /// <param name="b">Exponent [dimensionless].</param>
        /// <returns>
        /// Length (<paramref name="x"/>) raised to the power of <paramref name="b"/>
        /// (i.e. <paramref name="x"/>^<paramref name="b"/>), but the result is expressed in [m]!
        /// </returns>
        /// <remarks>NOTE:<br/>
        /// The function is intended to remedy dimensional inconsistency in<br/>
        /// power-law formulas of the form:
        /// <code>
        /// σ[m] = a * (x[m])^b  // a, b - constants</code>
        /// This formula is an analytical approximation of the plot of experimental data.<br/>
        /// It is assumed to return the result in [m] for the distance argument "<paramref name="x"/>" in [m],<br/>
        /// although such a dimensional relationship cannot be derived from<br/> the formula (unless b = 1)!
        /// </remarks>
        public static Meter Pow(Meter x, double b) => new(System.Math.Pow(x.Value, b));

        /// <summary>
        /// Arctangent for a point (x, y) with coordinates given in <see cref="Meter"/>s.
        /// </summary>
        /// <param name="y">The y coordinate of a point (x, y).</param>
        /// <param name="x">The x coordinate of a point (x, y).</param>
        /// <returns>The angle (in radians) whose tangent equals <c>y/x</c>.</returns>
        public static Radian Atan2(Meter y, Meter x) => new(System.Math.Atan2(y.m_value, x.Value));
        #endregion
    }
}
