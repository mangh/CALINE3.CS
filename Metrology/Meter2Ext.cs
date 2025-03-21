namespace Metrology
{
    public partial struct Meter2
    {
        #region Math (external)
        public static Meter Sqrt(Meter2 area) => new(System.Math.Sqrt(area.Value));
        #endregion
    }
}
