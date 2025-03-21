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

namespace CALINE3
{
    /*
     * Algorithms in this module make use of the power-law formulas:
     * 
     *    σy[m] = a * (x[m])^b  (horizontal dispersion parameter)
     *    σz[m] = c * (x[m])^d  (vertical dispersion parameter)
     * 
     *    a, b, c, d - constants dependent on stability class
     * 
     * which are analytical approximation of the plot of experimental data.
     * These formulas are assumed to give results (σy, σz) in [m] for the distance
     * argument (x) in [m] although such a relationship cannot be derived from
     * the formulas! This may be the source of dimensional inconsistencies!
     *
     * The time averaging and surface roughness corrections applied here - both
     * dimensionally unclear - give rise to further issues.
     *
     * Therefore I had to "hack" in places - mainly in the constructor - to make
     * the algorithm dimensionally consistent and getting still the same results
     * as the original CALINE3.
     *
     * I hope I did it right.
     * 
     */

    /// <summary>
    /// Gaussian Plume.
    /// </summary>
    public class Plume
    {
        #region Constants
        private static readonly Microgram_Meter3 ZERO_CONCENTRATION = new(0.0);
        private static readonly Meter_Sec ZERO_VELOCITY = new(0.0);
        private static readonly Meter ZERO_POSITION = new(0.0);
        private static readonly Meter MAX_MIXH = new(1000.0);
        #endregion

        #region Power-curve coefficients & Gaussian plume dispersion parameters
        /// <summary>
        /// Coefficient "a" in the power-curve formula: &#963;y[m] = a * (distance[m])^b.
        /// </summary>
        private readonly double PY1;

        /// <summary>
        /// Coefficient "b" in the power-curve formula: &#963;y[m] = a * (distance[m])^b.
        /// </summary>
        private readonly double PY2;

        /// <summary>
        /// Coefficient "c" in power-curve formula: &#963;z[m] = c * (distance[m])^d.
        /// </summary>
        private readonly double PZ1;

        /// <summary>
        /// Coefficient "d" in the power-curve formula: &#963;z[m] = c * (distance[m])^d.
        /// </summary>
        private readonly double PZ2;
        #endregion

        #region Environmental conditions
        /// <summary><see cref="Job"/> site factors.</summary>
        private readonly Job _site;

        /// <summary><see cref="Meteo"/> conditions.</summary>
        private readonly Meteo _meteo;

        /// <summary>Link (pollutant source) parameters.</summary>
        private readonly Link _link;

        /// <summary>Wind flow geometry.</summary>
        private readonly WindFlow _flow;
        #endregion

        #region Constructor
        /// <summary>
        /// <see cref="Plume"/> constructor.
        /// </summary>
        /// <param name="site">Site (location) factors.</param>
        /// <param name="meteo">Meteo conditions.</param>
        /// <param name="link">Link (pollutant source) parameters.</param>
        public Plume(Job site, Meteo meteo, Link link)
        {
            _site = site;
            _meteo = meteo;
            _link = link;
            _flow = new(meteo, link);

            /***************************************
             * 
             * σy(1[m])     = a * (1[m])^b = a = a * e^(b*ln(1[m])     = _met.AY1 * _site.RFAC_3CM_02 * _site.AFAC_3MIN_02;
             * σy(10000[m]) = a * (10000[m])^b = a * e^(b*ln(10000[m]) = _met.AY2 * _site.RFAC_3CM_007 * _site.AFAC_3MIN_02;
             * 
             * b = (ln(σy(10000[m]) - ln(σy(1[m]))) / (ln(10000[m]) - ln(1[m])
             * 
             */

            // σy(1[m]) = a-coefficient [m]
            PY1 = _meteo.AY1 * _site.RFAC_3CM_02 * _site.AFAC_3MIN_02;

            // σy(10000[m])
            double PY10 = _meteo.AY2 * _site.RFAC_3CM_007 * _site.AFAC_3MIN_02;

            // b-coefficient [dimensionless]
            PY2 = Log(PY10 / PY1) / Log(Link.MAX_LENGTH / Link.MIN_LENGTH);

            /***************************************
             * 
             * Mixing zone residence time [s]:
             * 
             */

            Second TR = link.DSTR * link.W2 / meteo.U;

            /***************************************
             * 
             * To relate SGZI (σz initial vertical dispersion parameter) and
             * TR (mixing zone residence time) the following equation:
             * 
             *      SGZI[m] = 1.8[?] + 0.11[?] * TR[s]
             * 
             * has been derived from the General Motors Data Base.
             * 
             * NOTE THE MISSING UNITS [?].
             * 
             * I guess it should be like this:
             */

            Meter SGZI = (Meter)1.8 + (Meter_Sec)0.11 * TR;

            // SGZI need to be adjusted for the averaging time (but it is considered
            // to be independent of surface roughness and atmospheric stability class).
            // It is always defined as occurring at a distance W2 downwind from the
            // element centerpoint i.e. it equals σz(W2[m]):
            Meter SGZ1 = SGZI * _site.AFAC_30MIN_02;

            // σz(10000[m]) adjusted for roughness and averaging time:
            Meter SZ10 = (Meter)_meteo.AZ * _site.RFAC_10CM_007 * _site.AFAC_3MIN_02;

            /***************************************
             * 
             * σz(W2[m])    = c * (W2[m])^d    = c * e^(d∙ln(W2[m]))
             * σz(10000[m]) = c * (10000[m])^d = c * e^(d∙ln(10000[m]))
             * 
             * d = (ln(σz(10000)) - ln(σz(W2))) / (ln(10000) - ln(W2))
             * 
             * ln(σz(W2)/c(W2)) = ln(σz(W2)) - ln(c(W2)) = d * ln(W2)
             * ln(σz(10000)/c(10000)) = ln(σz(10000)) - ln(c(10000)) = d * ln(10000)
             * 
             * ln(c) = (ln(c(10000)) - ln(c(W2))) / 2
             *       = (ln(σz(10000)) + ln(σz(W2)) - d * (ln(10000) + ln(W2))) / 2
             *       = (ln(σz(10000) * σz(W2)) - d*(ln(10000 * W2))) / 2
             * 
             * c = sqrt(σz(10000) * σz(W2)) / pow(sqrt(10000 * W2), d)
             * 
             */

            // d-coefficient [dimesionless]
            PZ2 = Log(SZ10 / SGZ1) / Log(Link.MAX_LENGTH / _link.W2);

            // c-coefficient [m]
            PZ1 = Sqrt(SZ10 * SGZ1) / Pow(Sqrt(Link.MAX_LENGTH * _link.W2), PZ2);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Pollutant concentration [µg/m3] at the <paramref name="receptor"/> location.
        /// </summary>
        /// <param name="receptor">receptor</param>
        /// <returns></returns>
        public Microgram_Meter3 ConcentrationAt(Receptor receptor)
        {
            // D - distance (perpendicular to the link)
            // L - offset (parallel to the link, relative to its start position)
            // Z - level (adjusted for the link type)
            (Meter D, Meter L, Meter Z) = _link.TransformReceptorCoordinates(receptor);

            // Assuming 0 at the orthogonal projection point
            // of the receptor on the link line:
            Meter DWL = -(_link.LL + L);
            Meter UWL = -L;

            // Mass Concentration
            Microgram_Meter3 C = ZERO_CONCENTRATION;

            // Add up the concentrations from the UPWIND elements:
            for (Meter elemEnd = ZERO_POSITION, EL = _link.WL; elemEnd < UWL; EL *= _flow.BASE)
            {
                // Next element
                Meter elemStart = elemEnd;
                elemEnd += EL;
                // Any element reached?
                if (elemEnd > DWL)
                {
                    LinkElement elem = new(_link, _flow,
                        ED1: (elemStart < DWL) ? DWL : elemStart,
                        ED2: (elemEnd > UWL) ? UWL : elemEnd
                    );
                    C += ConcentrationFrom(elem, D, Z);
                }
            }

            // Add up the concentrations from the DOWNWIND elements:
            for (Meter elemStart = ZERO_POSITION, EL = _link.WL; elemStart > DWL; EL *= _flow.BASE)
            {
                // Next element
                Meter elemEnd = elemStart;
                elemStart -= EL;
                // Any element reached?
                if (elemStart < UWL)
                {
                    LinkElement elem = new(_link, _flow,
                        ED1: (elemStart < DWL) ? DWL : elemStart,
                        ED2: (elemEnd > UWL) ? UWL : elemEnd
                    );
                    C += ConcentrationFrom(elem, D, Z);
                }
            }

            return C;
        }

        /// <summary>
        /// Incremental concentration [µg/m3] from the <paramref name="element"/>
        /// at the distance <paramref name="D"/> and at the level <paramref name="Z"/>.
        /// </summary>
        /// <param name="element">link element</param>
        /// <param name="D">receptor-link distance [m]</param>
        /// <param name="Z">receptor level (adjusted to the <see cref="Link"/> type) [m]</param>
        /// <returns></returns>
        private Microgram_Meter3 ConcentrationFrom(LinkElement element, Meter D, Meter Z)
        {
            // Get element profile:
            //    QE  - central subelement lineal strength [µg/(m*s)]
            //    YE  - plume centerline offset [m]
            //    FET - element fetch [m]
            if (!element.GetProfile(D, out Microgram_MeterSec QE, out Meter YE, out Meter FET))
            {
                return ZERO_CONCENTRATION;  // Element does NOT contribute.
            }

            // σy [m] - horizontal standard deviation of the emission distribution:
            Meter SGY = PY1 * Pow(FET, PY2);

            // σz [m] - vertical standard deviation of the emission distribution:
            Meter SGZ = PZ1 * Pow(FET, PZ2);

            // Vertical diffusivity estimate [m2/s]:
            Meter2_Sec KZ = SGZ * SGZ / (2.0 * FET / _meteo.U);

            Microgram_Meter3 FACT = 
                element.SourceStrength(QE, SGY, YE) / (Sqrt(2.0 * System.Math.PI) * SGZ * _meteo.U);

            // Adjust for depressed section wind speed
            FACT *= _link.DepressedSectionFactor(D);

            // Deposition correction
            double FAC3 = DepositionFactor(SGZ, KZ, Z, _link.H, _site.V1);
            if (double.IsNaN(FAC3))
            {
                return ZERO_CONCENTRATION;
            }
            else
            {
                // Settling correction
                FACT *= SettlingFactor(SGZ, KZ, Z, _link.H, _site.VS);

                // Incremental concentration from the element
                double FAC5 = GaussianFactor(SGZ, Z, _link.H, _meteo.MIXH);
                return FACT * (FAC5 - FAC3);
            }
        }

        private static double DepositionFactor(Meter SGZ, Meter2_Sec KZ, Meter Z, Meter H, Meter_Sec V1)
        {
            double FAC3 = 0.0;
            if (V1 != ZERO_VELOCITY)
            {
                double ARG = (V1 * SGZ / KZ + (Z + H) / SGZ) / Sqrt(2.0);
                if (ARG > 5.0)
                {
                    FAC3 = double.NaN;
                }
                else
                {
                    FAC3 =
                        Sqrt(2.0 * System.Math.PI) * V1 * SGZ
                        * Exp(V1 * (Z + H) / KZ + 0.5 * (V1 * SGZ / KZ) * (V1 * SGZ / KZ))
                        * Statistics.Erf(ARG)
                        / KZ;

                    if (FAC3 > 2.0) FAC3 = 2.0;
                }
            }
            return FAC3;
        }

        private static double SettlingFactor(Meter SGZ, Meter2_Sec KZ, Meter Z, Meter H, Meter_Sec VS)
        {
            return (VS == ZERO_VELOCITY) ? 1.0 :
                Exp(-VS * (Z - H) / (2.0 * KZ) - (VS * SGZ / KZ) * (VS * SGZ / KZ) / 8.0);
        }

        private static double GaussianFactor(Meter SGZ, Meter Z, Meter H, Meter MIXH)
        {
            double FAC5 = 0.0;
            double CNT = 0.0;
            double EXLS = 0.0;
            while (true)
            {
                double ARG1 = -0.5 * ((Z + H + 2.0 * CNT * MIXH) / SGZ) * ((Z + H + 2.0 * CNT * MIXH) / SGZ);
                double EXP1 = (ARG1 < -44.0) ? 0.0 : Exp(ARG1);

                double ARG2 = -0.5 * ((Z - H + 2.0 * CNT * MIXH) / SGZ) * ((Z - H + 2.0 * CNT * MIXH) / SGZ);
                double EXP2 = (ARG2 < -44.0) ? 0.0 : Exp(ARG2);

                FAC5 += EXP1 + EXP2;

                if (MIXH >= MAX_MIXH)
                    break;

                if (((EXP1 + EXP2 + EXLS) == 0.0) && (CNT <= 0.0))
                    break;

                if (CNT <= 0.0)
                {
                    CNT = Abs(CNT) + 1.0;
                    EXLS = 0.0;
                }
                else
                {
                    CNT = -1.0 * CNT;
                    EXLS = EXP1 + EXP2;
                }
            }
            return FAC5;
        }
        #endregion
    }
}
