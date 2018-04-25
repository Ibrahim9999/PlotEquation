using System;
using System.Collections.Generic;
using Rhino;
using Rhino.Geometry;
using NCalc;

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
        NONE = 0, CARTESIAN, SPHERICAL, CYLINDRICAL, CONICAL
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
    public struct RhinoObjects
    {
        List<Point3d> points;
        List<Curve> lines;
        List<Curve> curves;
        List<Surface> triangles;
        List<Surface> quads;
        List<Surface> surfaces;

    }

    public struct Variables
    {
        public VariablesUsed used;
        public List<string> names;
    };

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
        /// These lists contain the variables that can be used in equations.
        /// </summary>
        /// <remarks>
        /// The order the variables are in these lists matter. For instance,
        /// when considering an equation in the form of z(x,y), x an y are the
        /// independent variables used, and correspond to the first and second
        /// elements of the cartesianVars list. Therefore, the value stored to
        /// varables.used would be ONE_TWO.
        /// </remarks>
        public readonly List<string> cartesianVars = new List<string>()
        {
            "x", "y", "z", "w"
        };
        public readonly List<string> sphericalVars = new List<string>()
        {
            "theta", "r", "phi", "s"
        };
        public readonly List<string> cylindricalVars = new List<string>()
        {
            "theta", "r", "z", "s"
        };

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
        protected Variables variables;

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
        /// Tests to see if an Expression has no errors.
        /// </summary>
        public static bool ValidExpression(Expression e)
        {
            if (e.HasErrors())
            {
                RhinoApp.WriteLine(e.Error);
                return false;
            }

            return true;
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
            return variables.used == VariablesUsed.IMPLICIT_CURVE || variables.used == VariablesUsed.IMPLICIT_SURFACE;
        }

        /// <summary>
        /// Checks whether equation is parametric.
        /// </summary>
        public bool IsParametric()
        {
            return variables.used == VariablesUsed.PARAMETRIC_CURVE || variables.used == VariablesUsed.PARAMETRIC_SURFACE;
        }

        /// <summary>
        /// Determines the equation type, variables, and dimension.
        /// </summary>
        protected abstract bool DetermineEquationType();
    }

    /// <summary>
    /// Standard Equation class.
    /// </summary>
    public sealed class StandardEquation : Equation
    {
        /// <summary>
        /// Custructs a StandardEquation from a string that represents an
        /// expression, and an integer that represents the dimension.
        /// </summary>
        public StandardEquation(string expression, int dimension)
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
        /// Determines the equation type and variables.
        /// </summary>
        protected override bool DetermineEquationType()
        {
            // List of variables that pertains to equation type
            List<string> vars = new List<string>();

            // Determines what kind of equation the expression is
            if (ValidVariables(cartesianVars))
            {
                equationType = EquationType.CARTESIAN;
                vars = cartesianVars;
            }
            else if (ValidVariables(sphericalVars))
            {
                equationType = EquationType.SPHERICAL;
                vars = sphericalVars;
            }
            else if (ValidVariables(cylindricalVars))
            {
                equationType = EquationType.CYLINDRICAL;
                vars = cylindricalVars;
            }
            else if (expression.Length == 0)
                return false;
            else
                return false;

            // Bools for simplifying code, so I don't have to do
            // expression.Contains() all the time
            List<bool> exists = new List<bool>();
            bool equalsExists = expression.Contains("=");

            string simplifiedEq = expression.Substring(expression.IndexOf('=') + 1);

            for (int i = 0; i < dimension; i++)
                exists.Add(expression.Contains(vars[i]));

            // Figuring out which variables are used
            switch (dimension)
            {
                case 2:
                    if (((exists[0] || ContainsRightVariables(new List<string>() { vars[1] })) && (expression.StartsWith(vars[1] + "=", StringComparison.Ordinal) || expression.StartsWith("f(" + vars[0] + ")=", StringComparison.Ordinal) || !equalsExists)) && ContainsRightVariables(new List<string>() { vars[1] }))
                    {
                        variables.used = VariablesUsed.ONE;
                        variables.names = vars;
                        simplifiedEq += "+0*" + vars[0];
                    }
                    else if (((exists[1] || ContainsRightVariables(new List<string>() { vars[0] })) && (expression.StartsWith(vars[0] + "=", StringComparison.Ordinal) || expression.StartsWith("f(" + vars[1] + ")=", StringComparison.Ordinal) || !equalsExists)) && ContainsRightVariables(new List<string>() { vars[0] }))
                    {
                        variables.used = VariablesUsed.TWO;
                        variables.names = vars;
                        simplifiedEq += "+0*" + vars[1];
                    }

                    break;
                default:
                    if (((exists[0] || exists[1] || (exists[3] && Is4D()) || ContainsRightVariables(new List<string>() { vars[0], vars[1], vars[3] })) && (expression.StartsWith(vars[2] + "=", StringComparison.Ordinal) || expression.StartsWith("f(" + vars[0] + "," + vars[1] + ")=", StringComparison.Ordinal) || !equalsExists)) && ContainsRightVariables(new List<string>() { vars[0], vars[1], vars[3] }))
                    {
                        variables.used = VariablesUsed.ONE_TWO;
                        variables.names = vars;
                        simplifiedEq += "+0*" + vars[0] + "*" + vars[1] + "*" + vars[3];
                    }
                    else if (((exists[1] || exists[2] || (exists[3] && Is4D()) || ContainsRightVariables(new List<string>() { vars[1], vars[2], vars[3] })) && (expression.StartsWith(vars[0] + "=", StringComparison.Ordinal) || expression.StartsWith("f(" + vars[1] + "," + vars[2] + ")=", StringComparison.Ordinal) || !equalsExists)) && ContainsRightVariables(new List<string>() { vars[1], vars[2], vars[3] }))
                    {
                        variables.used = VariablesUsed.TWO_THREE;
                        variables.names = vars;
                        simplifiedEq += "+0*" + vars[1] + "*" + vars[2] + "*" + vars[3];
                    }
                    else if (((exists[2] || exists[0] || (exists[3] && Is4D()) || ContainsRightVariables(new List<string>() { vars[2], vars[0], vars[3] })) && (expression.StartsWith(vars[1] + "=", StringComparison.Ordinal) || expression.StartsWith("f(" + vars[2] + "," + vars[0] + ")=", StringComparison.Ordinal) || !equalsExists)) && ContainsRightVariables(new List<string>() { vars[2], vars[0], vars[3] }))
                    {
                        variables.used = VariablesUsed.ONE_THREE;
                        variables.names = vars;
                        simplifiedEq += "+0*" + vars[2] + "*" + vars[0] + "*" + vars[3];
                    }

                    break;
            }

            expression = simplifiedEq;

            return true;
        }

        private bool ValidVariables(List<string> vars)
        {
            return ContainsRightVariables(vars) || !ContainsWrongVariables(vars);
        }

        private bool ContainsRightVariables(List<string> vars)
        {
            foreach (string v in vars)
                if (expression.Contains(v))
                    return true;

            return false;
        }

        private bool ContainsWrongVariables(List<string> vars)
        {
            string eq = expression;

            if (eq.Length == 0)
                return true;

            eq = eq.Substring(eq.IndexOf('=') + 1);

            RemoveMathFunctions(ref eq);

            foreach (string v in vars)
                if (eq.Contains(v))
                    eq = eq.Replace(v, "");
            
            return CheckForBadVar(eq.ToCharArray());
        }

        /// <summary>
        /// Removes the substrings that are mathematical functions from
        /// equation; used for variable checking.
        /// </summary>
        private bool CheckForBadVar(char[] eq)
        {
            foreach (char c in eq)
                if (c >= 'a' && c <= 'z')
                    return true;

            return false;
        }
        
        /// <summary>
        /// Removes the substrings that are mathematical functions from
        /// equation; used for variable checking.
        /// </summary>
        private static void RemoveMathFunctions(ref string eq)
        {
            eq = eq.Replace("ieeeremainder(", "");
            eq = eq.Replace("remainder(", "");
            eq = eq.Replace("truncate(", "");
            eq = eq.Replace("randdec(", "");
            eq = eq.Replace("randint(", "");
            eq = eq.Replace("ceiling(", "");
            eq = eq.Replace("random(", "");
            eq = eq.Replace("round(", "");
            eq = eq.Replace("floor(", "");
            eq = eq.Replace("log10(", "");
            eq = eq.Replace("asin(", "");
            eq = eq.Replace("acos(", "");
            eq = eq.Replace("atan(", "");
            eq = eq.Replace("sinh(", "");
            eq = eq.Replace("cosh(", "");
            eq = eq.Replace("tanh(", "");
            eq = eq.Replace("csch(", "");
            eq = eq.Replace("sech(", "");
            eq = eq.Replace("coth(", "");
            eq = eq.Replace("sinc(", "");
            eq = eq.Replace("sign(", "");
            eq = eq.Replace("sqrt(", "");
            eq = eq.Replace("rand", "");
            eq = eq.Replace("abs(", "");
            eq = eq.Replace("pow(", "");
            eq = eq.Replace("min(", "");
            eq = eq.Replace("max(", "");
            eq = eq.Replace("exp(", "");
            eq = eq.Replace("log(", "");
            eq = eq.Replace("sin(", "");
            eq = eq.Replace("cos(", "");
            eq = eq.Replace("tan(", "");
            eq = eq.Replace("csc(", "");
            eq = eq.Replace("sec(", "");
            eq = eq.Replace("cot(", "");
            eq = eq.Replace("ln(", "");
            eq = eq.Replace("pi", "");
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
            if (DetermineEquationType() && ValidExpression(new Expression(expression)))
            {

            }
            else
                RhinoApp.WriteLine("Failed to generate equation. Please input a non-empty equation with valid variables.");
        }
    }
}
