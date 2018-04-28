using System;
using System.Collections.Generic;
using Rhino;
using Rhino.Geometry;
using NCalc;
using System.Linq;

namespace PlotEquation
{
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
    /// Base mathematical object class.
    /// </summary>
    public abstract class Plot
    {
        /// <summary>
        /// Contains all the base types of 3D objects Plot can use.
        /// </summary>
        public class Objects
        {
            /// <summary>
            /// Point object with three components.
            /// </summary>
            public struct Point
            {
                public double X;
                public double Y;
                public double Z;

                public Point(double X, double Y)
                {
                    this.X = X;
                    this.Y = Y;
                    Z = 0;
                }
                public Point(double X, double Y, double Z)
                {
                    this.X = X;
                    this.Y = Y;
                    this.Z = Z;
                }

                public override string ToString()
                {
                    return (System.String.Format("({0}, {1}, {2})", X, Y, Z));
                }

                public override int GetHashCode()
                {
                    return X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode();
                }

                public override bool Equals(object obj)
                {
                    return this == (Point)obj;
                }

                public static bool operator ==(Point a, Point b)
                {
                    return a.X.Equals(b.X) && a.Y.Equals(b.Y) && a.Z.Equals(b.Z);
                }

                public static bool operator !=(Point a, Point b)
                {
                    return !(a == b);
                }

                public static Point operator +(Point a, Point b)
                {
                    return new Point(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
                }

                public static Point operator *(double d, Point p)
                {
                    return new Point(d * p.X, d * p.Y, d * p.Z);
                }
            }
        
            /// <summary>
            /// 2D list of points in 3D space.
            /// </summary>
            public struct Grid
            {
                public List<List<Point>> points;

                public Grid(List<List<Point>> points)
                {
                    this.points = points;
                }

                public List<Point> this[int index]
                {
                    get { return points[index]; }

                    set { points[index] = value; }
                }

                /// <summary>
                /// Creates a Grid from a Wirefram object.
                /// </summary>
                public void FromWireframe(Wireframe wireframe)
                {
                    points = new List<List<Point>>();
                    var polylines = (wireframe.uCurves.Count == 0) ? wireframe.vCurves : wireframe.uCurves;

                    foreach (List<Line> polyline in polylines)
                    {
                        var p = new List<Point>();

                        p.Add(polyline[0].start);

                        foreach (Line line in polyline)
                            p.Add(line.end);

                        points.Add(p);
                    }
                }
            }

            /// <summary>
            /// An object that represents the line between two points.
            /// </summary>
            public struct Line
            {
                public Point start;
                public Point end;

                public Line(Point a, Point b)
                {
                    start = a;
                    end = b;
                }

                public override string ToString()
                {
                    return string.Format("[Line Start={0}, End={1}]", start, end);
                }

                public override int GetHashCode()
                {
                    return start.GetHashCode() ^ end.GetHashCode();
                }

                public override bool Equals(object obj)
                {
                    return this == (Line)obj;
                }

                public static bool operator ==(Line a, Line b)
                {
                    return (a.start.Equals(b.start) && a.end.Equals(b.end));
                }

                public static bool operator !=(Line a, Line b)
                {
                    return !(a == b);
                }

                /// <summary>
                /// Finds the midpoint of the line.
                /// </summary>
                public Point Midpoint()
                {
                    return new Point((start.X + end.X) / 2, (start.Y + end.Y) / 2, (start.Z + end.Z) / 2);
                }
            }

            /// <summary>
            /// A 2D grid of lines in 3D space.
            /// </summary>
            public struct Wireframe
            {
                public List<List<Line>> uCurves;
                public List<List<Line>> vCurves;

                public Wireframe(List<List<Line>> u, List<List<Line>> v)
                {
                    uCurves = u;
                    vCurves = v;
                }

                public List<Point> ToPoints()
                {
                    var polylines = (uCurves.Count == 0) ? vCurves : uCurves;
                    var points = new List<Point>();

                    points.Add(polylines[0][0].start);

                    foreach (List<Line> polyline in polylines)
                        foreach (Line line in polyline)
                            points.Add(line.end);

                    return points;
                }

                public List<Line> ToLines()
                {
                    var lines = new List<Line>();

                    foreach (List<Line> polyline in uCurves)
                        lines.AddRange(polyline);
                    foreach (List<Line> polyline in vCurves)
                        lines.AddRange(polyline);

                    return lines;
                }

                public void MakeUFromV()
                {
                    uCurves = new List<List<Line>>();

                    for (int i = 0; i < vCurves[0].Count; i++)
                    {
                        var polyline = new List<Line>();

                        for (int e = 0; e < vCurves.Count; e++)
                            polyline.Add(vCurves[e][i]);

                        uCurves.Add(polyline);
                    }
                }

                public void MakeVFromU()
                {
                    vCurves = new List<List<Line>>();

                    for (int i = 0; i < uCurves[0].Count; i++)
                    {
                        var polyline = new List<Line>();

                        for (int e = 0; e < uCurves.Count; e++)
                            polyline.Add(uCurves[e][i]);

                        vCurves.Add(polyline);
                    }
                }
            }

            /// <summary>
            /// A collection of 3D points that represent verticies of a triangle.
            /// </summary>
            public struct Triangle
            {
                public Point[] verticies;

                public Triangle(Point a, Point b, Point c)
                {
                    verticies = new Point[3];

                    verticies[0] = a;
                    verticies[1] = b;
                    verticies[2] = c;
                }

                public Point this[int index]
                {
                    get { return verticies[index]; }

                    set { verticies[index] = value; }
                }
            }

            /// <summary>
            /// A collection of 3D points that represent verticies of a quad.
            /// </summary>
            public struct Quad
            {
                public Point[] verticies;

                public Quad(Point a, Point b, Point c, Point d)
                {
                    verticies = new Point[4];

                    verticies[0] = a;
                    verticies[1] = b;
                    verticies[2] = c;
                    verticies[3] = d;
                }

                public Point this[int index]
                {
                    get { return verticies[index]; }

                    set { verticies[index] = value; }
                }
            }

            /// <summary>
            /// A collection of Triangles that represent a mesh.
            /// </summary>
            public struct TriangleMesh
            {
                public List<Triangle> triangles;

                public TriangleMesh(List<Triangle> mesh)
                {
                    triangles = mesh;
                }

                public Triangle this[int index]
                {
                    get { return triangles[index]; }

                    set { triangles[index] = value; }
                }

                public int Count
                {
                    get { return triangles.Count; }
                }

                public void MakeFromWireframe(Wireframe wireframe)
                {
                    triangles = new List<Triangle>();
                    var polylines = (wireframe.uCurves.Count == 0) ? wireframe.vCurves : wireframe.uCurves;

                    for (int i = 1; i < polylines.Count; i++)
                        for (int e = 1; e < polylines[i].Count; e++)
                        {
                            triangles.Add(new Triangle(polylines[i - 1][e - 1].end, polylines[i][e - 1].end, polylines[i][e].end));
                            triangles.Add(new Triangle(polylines[i - 1][e - 1].end, polylines[i][e].end, polylines[i - 1][e].end));
                        }
                }

                public void MakeFromQuadMesh(QuadMesh mesh)
                {
                    triangles = new List<Triangle>();

                    for (int i = 1; i < mesh.Count; i++)
                    {
                        triangles.Add(new Triangle(mesh[i][0], mesh[i][1], mesh[i][2]));
                        triangles.Add(new Triangle(mesh[i][0], mesh[i][3], mesh[i][2]));
                    }

                }
            }

            /// <summary>
            /// A collection of Quads that represent a mesh.
            /// </summary>
            public struct QuadMesh
            {
                public List<Quad> quads;

                public QuadMesh(List<Quad> mesh)
                {
                    quads = mesh;
                }

                public Quad this[int index]
                {
                    get { return quads[index]; }

                    set { quads[index] = value; }
                }

                public int Count
                {
                    get { return quads.Count; }
                }

                public void MakeFromWireframe(Wireframe wireframe)
                {
                    quads = new List<Quad>();
                    var polylines = (wireframe.uCurves.Count == 0) ? wireframe.vCurves : wireframe.uCurves;

                    for (int i = 1; i < polylines.Count; i++)
                        for (int e = 1; e < polylines[i].Count; e++)
                            quads.Add(new Quad(polylines[i - 1][e - 1].end, polylines[i][e - 1].end, polylines[i][e].end, polylines[i - 1][e].end));
                }

                public void MakeFromTriangleMesh(TriangleMesh mesh)
                {
                    quads = new List<Quad>();

                    for (int i = 0; i < mesh.Count; i += 2)
                        quads.Add(new Quad(mesh[i][0], mesh[i][1], mesh[i][2], mesh[i + 1][1]));
                }
            }
        }

        /// <summary>
        /// Contains all the types of mathematical objects that can be plotted.
        /// </summary>
        public enum Type
        {
            NONE = 0, STANDARD_EQUATION, PARAMETRIC_EQUATION, MANDELBROT, STRANGE_ATTRACTOR,
        }
        
        /// <summary>
        /// Used for converting Plot objects to Rhino objects.
        /// </summary>
        public struct RhinoObjects
        {
            public List<Point3d> points;
            public List<List<Point3d>> grid;
            public List<Curve> lines;
            public List<Curve> curves;
            public List<List<Curve>> wireframe;
            public List<Surface> triangles;
            public List<Surface> quads;
            public List<Surface> surfaces;
        }
        
        // CONVERT THEM TO RHINO OBJECTS

        /// <summary>
        /// Bool dictating whether plot generation is safe or not.
        /// </summary>
        protected bool success;

        /// <summary>
        /// Represents the kind of mathematical object the Plot is.
        /// </summary>
        protected Type plotType;

        /// <summary>
        /// Any sliders are added here; used to see change over time.
        /// </summary>
        protected Dictionary<string, double> sliders = new Dictionary<string, double>();

        /// <summary>
        /// Returns the mathematical object type of the Plot.
        /// </summary>
        protected Type ObjectType => plotType;

        /// <summary>
        /// Returns whether plot can safely be generated.
        /// </summary>
        public bool Successful()
        {
            return success;
        }

        /// <summary>
        /// Checks Plot to see whether it is an equation or not.
        /// </summary>
        public bool IsEquation()
        {
            return plotType == Type.STANDARD_EQUATION || plotType == Type.PARAMETRIC_EQUATION;
        }

        /// <summary>
        /// Checks Plot to see whether it is a standard equation or not.
        /// </summary>
        public bool IsStandardEquation()
        {
            return plotType == Type.STANDARD_EQUATION;
        }

        /// <summary>
        /// Checks Plot to see whether it is a parametric equation or not.
        /// </summary>
        public bool IsParametricEquation()
        {
            return plotType == Type.PARAMETRIC_EQUATION;
        }

        /// <summary>
        /// Checks Plot to see whether it is a mandelbrot fractal or not.
        /// </summary>
        public bool IsMandelbrot()
        {
            return plotType == Type.MANDELBROT;
        }

        /// <summary>
        /// Checks Plot to see whether it is a strange attractor or not.
        /// </summary>
        public bool IsStrangeAttractor()
        {
            return plotType == Type.STRANGE_ATTRACTOR;
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
        /// Contains all type of coordinate systems the plugin can plot.
        /// </summary>
        new public enum Type
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
        /// Wraps points back to each other in a loop.
        /// </summary>
        public bool wrapPoints;
        /// <summary>
        /// Wraps curves back to each other in a loop.
        public bool wrapCurves;

        /// <summary>
        /// List of variables with their corresponding bounds.
        /// </summary>
        protected Dictionary<string, Bounds> vars = new Dictionary<string, Bounds>();

        /// <summary>
        /// These lists contain the cartesian variables that can be used in equations.
        /// </summary>
        /// <remarks>
        /// The order the variables are in these lists matter. For instance,
        /// when considering an equation in the form of z(x,y), x and y are the
        /// independent variables used, and correspond to the first and second
        /// elements of the cartesianVars list. Therefore, the value stored to
        /// varables.used would be ONE_TWO.
        /// </remarks>
        public readonly List<string> cartesianVars = new List<string>()
        {
            "x", "y", "z", "w"
        };
        /// <summary>
        /// These lists contain the spherical variables that can be used in equations.
        /// </summary>
        /// <remarks>
        /// The order the variables are in these lists matter. For instance,
        /// when considering an equation in the form of r(theta,phi), theta and phi are the
        /// independent variables used, and correspond to the first and third
        /// elements of the sphericalVars list. Therefore, the value stored to
        /// varables.used would be ONE_THREE.
        /// </remarks>
        public readonly List<string> sphericalVars = new List<string>()
        {
            "theta", "r", "phi", "s"
        };
        /// <summary>
        /// These lists contain the cylindrical variables that can be used in equations.
        /// </summary>
        /// <remarks>
        /// The order the variables are in these lists matter. For instance,
        /// when considering an equation in the form of theta(r,z), r and z are the
        /// independent variables used, and correspond to the second and third
        /// elements of the sphericalVars list. Therefore, the value stored to
        /// varables.used would be TWO_THREE.
        /// </remarks>
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
        protected Type equationType;
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
        protected abstract bool DetermineEquationType(List<Bounds> bounds);
    }

    /// <summary>
    /// Standard Equation class.
    /// </summary>
    public sealed class StandardEquation : Equation
    {
        /// <summary>
        /// Custructs a StandardEquation from a string that represents an
        /// expression, and a List of Bounds that represents the bounds for each
        /// variable. The equation dimension is then found by the number of
        /// elements in the list.
        /// </summary>
        public StandardEquation(string expression, List<Bounds> bounds)
        {
            plotType = Plot.Type.STANDARD_EQUATION;
            this.expression = expression;
            dimension = bounds.Count + 1;

            if (!DetermineEquationType(bounds))
            {
                success = false;
                RhinoApp.WriteLine("Failed to create equation object.");
            }
            else
                success = true;
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
        protected override bool DetermineEquationType(List<Bounds> bounds)
        {
            // List of variables that pertains to equation type
            List<string> vars = new List<string>();

            // Determines what kind of equation the expression is
            if (ValidVariables(cartesianVars))
            {
                equationType = Type.CARTESIAN;
                vars = cartesianVars;
            }
            else if (ValidVariables(sphericalVars))
            {
                equationType = Type.SPHERICAL;
                vars = sphericalVars;
            }
            else if (ValidVariables(cylindricalVars))
            {
                equationType = Type.CYLINDRICAL;
                vars = cylindricalVars;
            }
            else if (expression.Length == 0)
            {
                RhinoApp.WriteLine("The expression is empty.");
                return false;
            }
            else
            {
                RhinoApp.WriteLine("Unknown error.");
                return false;
            }

            // Bools for simplifying code, so I don't have to do
            // expression.Contains() all the time
            List<bool> exists = new List<bool>();
            bool equalsExists = expression.Contains("=");

            // Removes equals sign from expression
            string simplifiedEq = expression.Substring(expression.IndexOf('=') + 1) + "+0*";
            
            for (int i = 0; i < dimension; i++)
                exists.Add(expression.Contains(vars[i]));

            // Uses different methods for testing expression depending on
            // dimension. Figuring out which variables are used.
            switch (dimension)
            {
                case 2:
                    vars.RemoveAt(vars.Count - 1);

                    if (((exists[0] || ContainsRightVariables(new List<string>() { vars[1] })) && (expression.StartsWith(vars[1] + "=", StringComparison.Ordinal) || expression.StartsWith("f(" + vars[0] + ")=", StringComparison.Ordinal) || !equalsExists)) && ContainsRightVariables(new List<string>() { vars[1] }))
                    {
                        variablesUsed = VariablesUsed.ONE;
                        simplifiedEq += vars[0];
                    }
                    else if (((exists[1] || ContainsRightVariables(new List<string>() { vars[0] })) && (expression.StartsWith(vars[0] + "=", StringComparison.Ordinal) || expression.StartsWith("f(" + vars[1] + ")=", StringComparison.Ordinal) || !equalsExists)) && ContainsRightVariables(new List<string>() { vars[0] }))
                    {
                        variablesUsed = VariablesUsed.TWO;
                        simplifiedEq += vars[1];
                    }

                    break;
                case 3:
                    //4D implementation
                    /*if (((exists[0] || exists[1] || (exists[3] && Is4D()) || ContainsRightVariables(new List<string>() { vars[0], vars[1], vars[3] })) && (expression.StartsWith(vars[2] + "=", StringComparison.Ordinal) || expression.StartsWith("f(" + vars[0] + "," + vars[1] + ")=", StringComparison.Ordinal) || !equalsExists)) && ContainsRightVariables(new List<string>() { vars[0], vars[1], vars[3] }))
                    {
                        variablesUsed = VariablesUsed.ONE_TWO;
                        simplifiedEq += vars[0] + "*" + vars[1] + "*" + vars[3];
                    }*/
                    if (((exists[0] || exists[1] || ContainsRightVariables(new List<string>() { vars[0], vars[1] })) && (expression.StartsWith(vars[2] + "=", StringComparison.Ordinal) || expression.StartsWith("f(" + vars[0] + "," + vars[1] + ")=", StringComparison.Ordinal) || !equalsExists)) && ContainsRightVariables(new List<string>() { vars[0], vars[1] }))
                    {
                        variablesUsed = VariablesUsed.ONE_TWO;
                        simplifiedEq += vars[0] + "*" + vars[1];
                    }
                    else if (((exists[1] || exists[2] || ContainsRightVariables(new List<string>() { vars[1], vars[2] })) && (expression.StartsWith(vars[0] + "=", StringComparison.Ordinal) || expression.StartsWith("f(" + vars[1] + "," + vars[2] + ")=", StringComparison.Ordinal) || !equalsExists)) && ContainsRightVariables(new List<string>() { vars[1], vars[2] }))
                    {
                        variablesUsed = VariablesUsed.TWO_THREE;
                        simplifiedEq += vars[1] + "*" + vars[2];
                    }
                    else if (((exists[2] || exists[0] || ContainsRightVariables(new List<string>() { vars[2], vars[0] })) && (expression.StartsWith(vars[1] + "=", StringComparison.Ordinal) || expression.StartsWith("f(" + vars[2] + "," + vars[0] + ")=", StringComparison.Ordinal) || !equalsExists)) && ContainsRightVariables(new List<string>() { vars[2], vars[0] }))
                    {
                        variablesUsed = VariablesUsed.ONE_THREE;
                        simplifiedEq += vars[2] + "*" + vars[0];
                    }

                    break;
                default:
                    RhinoApp.WriteLine("Dimension outside scope of program. Accepted values are 2, 3, and 4.");
                    return false;
            }
            
            expression = simplifiedEq;

            for (int i = 0; i < dimension; i++)
                this.vars.Add(vars[i], bounds[i]);

            return ValidExpression(new Expression(expression));
        }

        /// <summary>
        /// Checks to see if the expression is valid.
        /// </summary>
        private bool ValidVariables(List<string> vars)
        {
            return ContainsRightVariables(vars) || !ContainsWrongVariables(vars);
        }

        /// <summary>
        /// Looks to see if valid vairables exist in expression.
        /// </summary>
        private bool ContainsRightVariables(List<string> vars)
        {
            foreach (string v in vars)
                if (expression.Contains(v))
                    return true;

            return false;
        }

        /// <summary>
        /// Checks for letters that shouldn't be there.
        /// </summary>
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

            foreach (char c in eq.ToCharArray())
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
            return dimension + "D " + equationType + " Equation:\n\t\"" + expression + "\"\n";
        }
        
        /// <summary>
        /// Creates objects.
        /// </summary>
        public override void Generate()
        {
            if (success)
            {

            }
            else
                RhinoApp.WriteLine("Unable to generate equation.");
        }
    }
}
