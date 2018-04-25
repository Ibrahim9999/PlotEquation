using System;
using System.Linq;
using System.Collections.Generic;
using NCalc;
using Rhino.Geometry;

namespace PlotEquation
{
    public struct Bounds
    {
        public double min;
        public double max;

        public Bounds(double min, double max)
        {
            this.min = min;
            this.max = max;
        }

        public override string ToString()
        {
            return (System.String.Format("({0}, {1})", min, max));
        }

        public override int GetHashCode()
        {
            return min.GetHashCode() ^ max.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return this == (Bounds)obj;
        }

        public static bool operator ==(Bounds a, Bounds b)
        {
            return a.min.Equals(b.min) && a.max.Equals(b.max);
        }

        public static bool operator !=(Bounds a, Bounds b)
        {
            return !(a == b);
        }

        public static bool operator >(Bounds a, Bounds b)
        {
            return (a.max - a.min) > (b.max - b.min);
        }

        public static bool operator <(Bounds a, Bounds b)
        {
            return (a.max - a.min) < (b.max - b.min);
        }

        public static bool operator >=(Bounds a, Bounds b)
        {
            return (a.max - a.min) >= (b.max - b.min);
        }

        public static bool operator <=(Bounds a, Bounds b)
        {
            return (a.max - a.min) <= (b.max - b.min);
        }
    }
}
