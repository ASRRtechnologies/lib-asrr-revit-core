using Autodesk.Revit.DB;
using System;

namespace ASRR.Revit.Core.Exporter.GLTF.Leia.Model
{
    /// <summary>
    ///     From Jeremy Tammik's RvtVa3c exporter:
    ///     https://github.com/va3c/RvtVa3c
    ///     An integer-based 3D point class.
    /// </summary>
    public class PointIntObject : IComparable<PointIntObject>
    {
        public PointIntObject(XYZ p)
        {
            X = p.X;
            Y = p.Y;
            Z = p.Z;
        }

        public double X { get; set; }

        public double Y { get; set; }

        public double Z { get; set; }

        public int CompareTo(PointIntObject a)
        {
            var d = X - a.X;
            if (d == 0)
            {
                d = Y - a.Y;
                if (d == 0) d = Z - a.Z;
            }

            return d == 0 ? 0 : d > 0 ? 1 : -1;
        }
    }
}