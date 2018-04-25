using System;
using System.Collections.Generic;
using Rhino.Geometry;

namespace PlotEquation
{
    /// <summary>
    /// Contains all the types of mathematical objects that can be plotted.
    /// </summary>
    public enum PlotType
    {
        NONE = 0, STANDARD_EQUATION, PARAMETRIC_EQUATION, MANDELBROT, STRANGE_ATTRACTOR,
    }

    /// <summary>
    /// Contains all type of coordinate systems the plugin can plot.
    /// </summary>
    public enum EquationType
    {
        NONE = 0, CARTESIAN, POLAR, CYLINDRICAL, CONICAL
    }

    /// <summary>
    /// Used to differentiate which variables are used in an equation
    /// </summary>
    public enum VariablesUsed
    {
        NONE = 0, ONE, TWO, PARAMETRIC_CURVE, IMPLICIT_CURVE, ONE_TWO, TWO_THREE, ONE_THREE, PARAMETRIC_SURFACE, IMPLICIT_SURFACE,
    }

    /// <summary>
    /// Contains the different ways mathematical objects can be graphed.
    /// </summary>
    /// <remarks>
    /// Curves is not listed here because all objects are generated from a
    /// group of points, which are grouped into lines, and curves are made
    /// from a group of lines.
    /// </remarks>
    public enum DisplayType
    {
        NONE = 0, POINTS, LINES, TRIANGLES, QUADS, SURFACE
    }

    /// <summary>
    /// Used for converting Plot objects to Rhino objects.
    /// </summary>
    struct RhinoObjects
    {
        List<Point3d> points;
        List<Curve> lines;
        List<Curve> curves;
        List<Surface> triangles;
        List<Surface> quads;
        List<Surface> surfaces;

    }

    /// <summary>
    /// Base mathematical object class.
    /// </summary>
    public abstract class Plot
    {
        /// <summary>
        /// Represents the kind of mathematical object the Plot is.
        /// </summary>
        protected PlotType plotType;

        /// <summary>
        /// Any sliders are added here; used to see change over time.
        /// </summary>
        protected Dictionary<string, double> sliders = new Dictionary<string, double>();

        /// <summary>
        /// Returns the mathematical object type of the Plot.
        /// </summary>
        protected PlotType ObjectType => plotType;

        /// <summary>
        /// Checks Plot to see whether it is an equation or not.
        /// </summary>
        public bool IsEquation()
        {
            return plotType == PlotType.STANDARD_EQUATION || plotType == PlotType.PARAMETRIC_EQUATION;
        }

        /// <summary>
        /// Checks Plot to see whether it is a standard equation or not.
        /// </summary>
        public bool IsStandardEquation()
        {
            return plotType == PlotType.STANDARD_EQUATION;
        }

        /// <summary>
        /// Checks Plot to see whether it is a parametric equation or not.
        /// </summary>
        public bool IsParametricEquation()
        {
            return plotType == PlotType.PARAMETRIC_EQUATION;
        }

        /// <summary>
        /// Checks Plot to see whether it is a mandelbrot fractal or not.
        /// </summary>
        public bool IsMandelbrot()
        {
            return plotType == PlotType.MANDELBROT;
        }

        /// <summary>
        /// Checks Plot to see whether it is a strange attractor or not.
        /// </summary>
        public bool IsStrangeAttractor()
        {
            return plotType == PlotType.STRANGE_ATTRACTOR;
        }

        /// <summary>
        /// Adds a slider to the list, which comprises of a variable and value.
        /// </summary>
        public void AddSlider(string variable, double value)
        {
            sliders.Add(variable, value);
        }

        /// <summary>
        /// Removes slider from list based on variable name.
        /// </summary>
        public void RemoveSlider(string variable)
        {
            sliders.Remove(variable);
        }

        /// <summary>
        /// Changes a slider value.
        /// </summary>
        public void ChangeSlider(string variable, double value)
        {
            sliders[variable] = value;
        }

        /// <summary>
        /// Updates a slider with an increment.
        /// </summary>
        public void UpdateSlider(string variable, double increment)
        {
            sliders[variable] += increment;
        }

        /// <summary>
        /// Outputs Plot informtion.
        /// </summary>
        public abstract override string ToString();

        /// <summary>
        /// Creates objects.
        /// </summary>
        public abstract void Generate();
    }

    /// <summary>
    /// Base equation class.
    /// </summary>
    public abstract class Equation : Plot
    {
        /// <summary>
        /// Wraps points back to each other in a loop.
        /// </summary>
        public bool wrapPoints;
        /// <summary>
        /// Wraps curves back to each other in a loop.
        public bool wrapCurves;

        /// <summary>
        /// List of variables with their corresponding bounds.
        /// </summary>
        private Dictionary<string, Bounds> vars = new Dictionary<string, Bounds>();

        /// <summary>
        /// Represents the number of points per curve.
        /// </summary>
        protected int pointsPerCurve;
        /// <summary>
        /// Represents the number of curves per surface.
        /// </summary>
        protected int curvesPerSurface;

        /// <summary>
        /// Represents the equation type.
        /// </summary>
        protected EquationType equationType;
        /// <summary>
        /// Represents the variables used in the equation.
        /// </summary>
        protected VariablesUsed variablesUsed;

        /// <summary>
        /// Represents the dimension of the equation.
        /// </summary>
        protected int dimension;

        /// <summary>
        /// Returns the dimension of the equation.
        /// </summary>
        public int Dimension => dimension;

        /// <summary>
        /// Returns the number of points per curve.
        /// </summary>
        public int PointsPerCurve => pointsPerCurve;
        /// <summary>
        /// Sets new value for the number of points per curve.
        /// </summary>
        public void SetPointsPerCurve(int pointsPerCurve)
        {
            this.pointsPerCurve = pointsPerCurve;
        }

        /// <summary>
        /// Returns the number of curves per surface.
        /// </summary>
        public int CurvesPerSurface => curvesPerSurface;
        /// <summary>
        /// Sets new value for the number of curves per surface.
        /// </summary>
        public void SetCurvesPerSurface(int curvesPerSurface)
        {
            this.curvesPerSurface = curvesPerSurface;
        }

        /// <summary>
        /// Checks whether equation is 2 dimensional.
        /// </summary>
        public bool Is2D()
        {
            return dimension == 2;
        }

        /// <summary>
        /// Checks whether equation is 3 dimensional.
        /// </summary>
        public bool Is3D()
        {
            return dimension == 3;
        }

        /// <summary>
        /// Checks whether equation is 4 dimensional.
        /// </summary>
        public bool Is4D()
        {
            return dimension == 4;
        }

        /// <summary>
        /// Checks whether equation is implicit.
        /// </summary>
        public bool IsImplicit()
        {
            return variablesUsed == VariablesUsed.IMPLICIT_CURVE || variablesUsed == VariablesUsed.IMPLICIT_SURFACE;
        }

        /// <summary>
        /// Checks whether equation is parametric.
        /// </summary>
        public bool IsParametric()
        {
            return variablesUsed == VariablesUsed.PARAMETRIC_CURVE || variablesUsed == VariablesUsed.PARAMETRIC_SURFACE;
        }

        /// <summary>
        /// Determines the equation type, variables, and dimension.
        /// </summary>
        protected abstract void DetermineEquationType();
    }

    /// <summary>
    /// Standard Equation class.
    /// </summary>
    public sealed class StandardEquation : Equation
    {
        /// <summary>
        /// Custructs a StandardEquation from a string that represents an expression.
        /// </summary>
        public StandardEquation(string expression)
        {
            plotType = PlotType.STANDARD_EQUATION;
            this.expression = expression;
        }

        /// <summary>
        /// String that represents the equation used.
        /// </summary>
        private string expression;

        /// <summary>
        /// Returns the expression as a string.
        /// </summary>
        public string Expression => expression;

        /// <summary>
        /// Determines the equation type, variables, and dimension.
        /// </summary>
        protected override void DetermineEquationType()
        {

        }

        /// <summary>
        /// Outputs StandardEquation informtion.
        /// </summary>
        public override string ToString()
        {
            return equationType + " Equation:\n\t\"" + expression + "\"\n";
        }
        
        /// <summary>
        /// Creates objects.
        /// </summary>
        public override void Generate()
        {
            throw new NotImplementedException();
        }
    }
}
