using System;
using System.Collections.Generic;
using Rhino;
using Rhino.Geometry;
using NCalc;
using System.Linq;

namespace PlotEquation
{
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
        /// List of variables that's used in equation.
        /// </summary>
        protected List<string> vars = new List<string>();

        /// <summary>
        /// List of the bounds to their corresponding variables.
        /// </summary>
        protected List<Bounds> bounds = new List<Bounds>();

        /// <summary>
        /// List of the bounds to coordinate values to restrict values that are
        /// too large.
        /// </summary>
        protected Dictionary<string, Bounds> maxValues = new Dictionary<string, Bounds>
        {
            { "X" , new Bounds(Double.MinValue, Double.MaxValue) },
            { "Y" , new Bounds(Double.MinValue, Double.MaxValue) },
            { "Z" , new Bounds(Double.MinValue, Double.MaxValue) }
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
        /// Returns the list with the vars used in equation.
        /// </summary>
        public List<string> Vars => vars;
        /// <summary>
        /// Returns the list with the bounds to the vars used in equation.
        /// </summary>
        public List<Bounds> Bounds => bounds;

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
        /// <returns></returns>
        public bool Is2D()
        {
            return dimension == 2;
        }

        /// <summary>
        /// Checks whether equation is 3 dimensional.
        /// </summary>
        /// <returns></returns>
        public bool Is3D()
        {
            return dimension == 3;
        }

        /// <summary>
        /// Checks whether equation is 4 dimensional.
        /// </summary>
        /// <returns></returns>
        public bool Is4D()
        {
            return dimension == 4;
        }

        /// <summary>
        /// Checks whether equation is implicit.
        /// </summary>
        /// <returns></returns>
        public bool IsImplicit()
        {
            return variablesUsed == VariablesUsed.IMPLICIT_CURVE || variablesUsed == VariablesUsed.IMPLICIT_SURFACE;
        }

        /// <summary>
        /// Checks whether equation is parametric.
        /// </summary>
        /// <returns></returns>
        public bool IsParametric()
        {
            return variablesUsed == VariablesUsed.PARAMETRIC_CURVE || variablesUsed == VariablesUsed.PARAMETRIC_SURFACE;
        }

        /// <summary>
        /// Evaluates the result of an equation after plugging in the variable
        /// values.
        /// </summary>
        /// <param name="eq"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        protected Objects.Point ExpressionResult(Expression eq, double x, double y, double z)
        {
            switch (equationType)
            {
                case Type.CARTESIAN:
                    switch (variablesUsed)
                    {
                        case VariablesUsed.ONE:
                        case VariablesUsed.ONE_TWO:
                            return Calculate.CartesianPoint(x, y, z);
                        case VariablesUsed.TWO:
                            return Calculate.CartesianPoint(y, x, z);
                        case VariablesUsed.ONE_THREE:
                            return Calculate.CartesianPoint(x, z, y);
                        case VariablesUsed.TWO_THREE:
                            return Calculate.CartesianPoint(y, z, x);
                    }

                    break;
                case Type.SPHERICAL:
                    switch (variablesUsed)
                    {
                        case VariablesUsed.ONE:
                        case VariablesUsed.ONE_TWO:
                            return Calculate.SphericalPoint(x, y, z);
                        case VariablesUsed.TWO:
                            return Calculate.SphericalPoint(y, x, z);
                        case VariablesUsed.ONE_THREE:
                            return Calculate.SphericalPoint(x, z, y);
                        case VariablesUsed.TWO_THREE:
                            return Calculate.SphericalPoint(y, z, x);
                    }

                    break;
                case Type.CYLINDRICAL:
                    switch (variablesUsed)
                    {
                        case VariablesUsed.ONE:
                        case VariablesUsed.ONE_TWO:
                            return Calculate.CylindricalPoint(x, y, z);
                        case VariablesUsed.TWO:
                            return Calculate.CylindricalPoint(y, x, z);
                        case VariablesUsed.ONE_THREE:
                            return Calculate.CylindricalPoint(x, z, y);
                        case VariablesUsed.TWO_THREE:
                            return Calculate.CylindricalPoint(y, z, x);
                    }

                    break;
            }

            return new Objects.Point(0, 0, 0);
        }

        /// <summary>
        /// Creates all the Rhino objects from Plot Wireframe.
        /// </summary>
        /// <param name="wireframe"></param>
        protected void CreateRhinoObjects(RhinoDoc doc, Objects.Wireframe wireframe)
        {
            RhinoApp.WriteLine("Creating objects...");

            if (dimension == 2)
            {
                // Point list creation
                RhinoApp.Write("\tPoints... ");
                rhinoObjects.points = new List<Point3d>();

                foreach (Objects.Polyline polyline in wireframe.uCurves)
                    foreach (Objects.Point point in polyline.Verticies)
                        rhinoObjects.points.Add(RhinoObjects.PointToRhino(point));

                RhinoApp.WriteLine("Done");

                // Polyline list creation
                RhinoApp.Write("\tPolylines... ");
                rhinoObjects.polylines = new List<Polyline>();

                foreach (Objects.Polyline polyline in wireframe.uCurves)
                {
                    Objects.Polyline pl = new Objects.Polyline();

                    foreach (Objects.Point point in polyline.Verticies)
                        if (point == Objects.Point.NaN && pl.Count > 1)
                        {
                            rhinoObjects.polylines.Add(RhinoObjects.PolylineToRhino(pl));
                            pl = new Objects.Polyline();
                        }
                        else
                            pl.Add(point);

                    if (pl.Count > 1)
                        rhinoObjects.polylines.Add(RhinoObjects.PolylineToRhino(pl));
                }

                RhinoApp.WriteLine("Done");

                // Curve list creation
                RhinoApp.Write("\tCurves... ");
                rhinoObjects.curves = new List<Curve>();

                foreach (Objects.Polyline polyline in wireframe.uCurves)
                {
                    Objects.Polyline pl = new Objects.Polyline();

                    foreach (Objects.Point point in polyline.Verticies)
                        if (point == Objects.Point.NaN && pl.Count > 1)
                        {
                            rhinoObjects.curves.Add(RhinoObjects.CurveToRhino(pl));
                            pl = new Objects.Polyline();
                        }
                        else
                            pl.Add(point);

                    if (pl.Count > 1)
                        rhinoObjects.curves.Add(RhinoObjects.CurveToRhino(pl));
                }

                RhinoApp.WriteLine("Done");
            }
            else if (dimension == 3)
            {
                // Grid creation
                RhinoApp.Write("\tGrid... ");
                rhinoObjects.grid = new List<List<Point3d>>();

                foreach (Objects.Polyline polyline in wireframe.uCurves)
                {
                    List<Point3d> points = new List<Point3d>();

                    foreach (Objects.Point point in polyline.Verticies)
                        points.Add(RhinoObjects.PointToRhino(point));

                    rhinoObjects.grid.Add(points);
                }

                RhinoApp.WriteLine("Done");

                // Lineframe creation
                RhinoApp.Write("\tLineframe... ");
                rhinoObjects.lineframe = RhinoObjects.LineframeToRhino(wireframe);

                RhinoApp.WriteLine("Done");

                // Wireframe creation
                RhinoApp.Write("\tWireframe... ");
                rhinoObjects.wireframe = RhinoObjects.WireframeToRhino(wireframe);

                RhinoApp.WriteLine("Done");

                // Triangle list creation
                RhinoApp.Write("\tTriangles... ");
                rhinoObjects.triangles = Brep.JoinBreps(RhinoObjects.TriangleMeshToRhino(Objects.TriangleMesh.MakeFromWireframe(wireframe)), .00000001).ToList();

                RhinoApp.WriteLine("Done");

                // Quad list creation
                RhinoApp.Write("\tQuads... ");
                rhinoObjects.quads = Brep.JoinBreps(RhinoObjects.QuadMeshToRhino(Objects.QuadMesh.MakeFromWireframe(wireframe)), .00000001).ToList();

                RhinoApp.WriteLine("Done");

                // Surface creation
                RhinoApp.Write("\tSurfaces... ");
                rhinoObjects.surfaces = new List<Surface>
                {
                    Create.SurfaceFromPoints(rhinoObjects.grid, 3, 3, wrapPoints, wrapCurves)
                };
                //@ Fix wireframe lack when NaN appears somewhere
                if (rhinoObjects.surfaces[0] == null)
                    foreach (Brep brep in rhinoObjects.quads)
                        rhinoObjects.surfaces.Add(Create.SurfaceFromPoints(Create.GridFromBrep(brep), 3, 3, wrapPoints, wrapCurves));

                RhinoApp.WriteLine("Done");
            }
        }

        /// <summary>
        /// Tests to see if an Expression has no errors.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
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
        /// Sets all the possible mathematical functions equations can use.
        /// </summary>
        /// <param name="expression"></param>
        public static void SetParameters(ref Expression expression)
        {
            expression.Parameters["pi"] = Math.PI;
            expression.Parameters["e"] = Math.E;

            expression.EvaluateFunction += delegate (string name, FunctionArgs args)
            {
                double d = Double.NaN,
                    e = Double.NaN,
                    f = Double.NaN;

                if (args.Parameters.Length > 0)
                    d = Convert.ToDouble(args.Parameters[0].Evaluate());
                if (args.Parameters.Length > 1)
                    e = Convert.ToDouble(args.Parameters[1].Evaluate());
                if (args.Parameters.Length > 2)
                    f = Convert.ToDouble(args.Parameters[2].Evaluate());

                switch (name)
                {
                    case "abs":
                        args.Result = Math.Abs(d);
                        break;
                    case "pow":
                        args.Result = Math.Pow(d, e);
                        break;
                    case "sqrt":
                        args.Result = Math.Sqrt(d);
                        break;
                    case "round":
                        args.Result = Math.Round(d, Convert.ToInt32(e));
                        break;
                    case "sign":
                        if (d > 0)
                            args.Result = 1;
                        else if (d < 0)
                            args.Result = -1;
                        else
                            args.Result = 0;
                        break;
                    case "min":
                        args.Result = Math.Min(d, e);
                        break;
                    case "max":
                        args.Result = Math.Max(d, e);
                        break;
                    case "ceiling":
                        args.Result = Math.Ceiling(d);
                        break;
                    case "truncate":
                        args.Result = Math.Truncate(d);
                        break;
                    case "exp":
                        args.Result = Math.Exp(d);
                        break;
                    case "floor":
                        args.Result = Math.Floor(d);
                        break;
                    case "remainder":
                    case "ieeeremainder":
                        args.Result = Math.IEEERemainder(d, e);
                        break;
                    case "ln":
                        args.Result = Math.Log(d);
                        break;
                    case "log":
                        if (Double.IsNaN(e))
                            args.Result = Math.Log10(d);
                        else
                            args.Result = Math.Log10(e) / Math.Log10(d);
                        break;
                    case "log10":
                        args.Result = Math.Log10(d);
                        break;
                    case "sin":
                        args.Result = Math.Sin(d % (2 * Math.PI));
                        break;
                    case "cos":
                        args.Result = Math.Cos(d % (2 * Math.PI));
                        break;
                    case "tan":
                        args.Result = Math.Tan(d % (2 * Math.PI));
                        break;
                    case "csc":
                        args.Result = 1 / Math.Sin(d % (2 * Math.PI));
                        break;
                    case "sec":
                        args.Result = 1 / Math.Cos(d % (2 * Math.PI));
                        break;
                    case "cot":
                        args.Result = 1 / Math.Tan(d % (2 * Math.PI));
                        break;
                    case "asin":
                        args.Result = Math.Asin(d % (2 * Math.PI));
                        break;
                    case "acos":
                        args.Result = Math.Acos(d % (2 * Math.PI));
                        break;
                    case "atan":
                        args.Result = Math.Atan(d % (2 * Math.PI));
                        break;
                    case "sinh":
                        args.Result = Math.Sinh(d);
                        break;
                    case "cosh":
                        args.Result = Math.Cosh(d);
                        break;
                    case "tanh":
                        args.Result = Math.Tanh(d);
                        break;
                    case "csch":
                        args.Result = 1 / Math.Sinh(d);
                        break;
                    case "sech":
                        args.Result = 1 / Math.Cosh(d);
                        break;
                    case "coth":
                        args.Result = 1 / Math.Tanh(d);
                        break;
                    case "sinc":
                        if (Equals(d, 0))
                            args.Result = 1;
                        else
                            args.Result = Math.Sin(d % (2 * Math.PI)) / d;
                        break;
                    case "random":
                        var random = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);
                        if (Double.IsNaN(d))
                            d = 1;
                        args.Result = random.NextDouble() * d;
                        break;
                    case "randint":
                        random = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);
                        if (d < e)
                            args.Result = (int)d + (int)((e - d + 1) * random.NextDouble());
                        else
                            args.Result = (int)e + (int)((d - e + 1) * random.NextDouble());
                        break;
                    case "randdec":
                        random = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);
                        if (d < e)
                            args.Result = d + (e - d + 1) * random.NextDouble();
                        else
                            args.Result = e + (d - e + 1) * random.NextDouble();
                        break;
                }
            };
        }

        /// <summary>
        /// Determines the equation type, variables, and dimension.
        /// </summary>
        /// <returns></returns>
        protected abstract bool DetermineEquationType();
    }

    /// <summary>
    /// Standard equation class (one expression line).
    /// </summary>
    public sealed class StandardEquation : Equation
    {
        /// <summary>
        /// Custructs a StandardEquation from a string that represents an
        /// expression, and a List of Bounds that represents the bounds for each
        /// variable. The equation dimension is then found by the number of
        /// elements in the list.
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="bounds"></param>
        public StandardEquation(string expression, List<Bounds> bounds, int pointsPerCurve = 20, int curvesPerSurface = 20)
        {
            plotType = Plot.Type.STANDARD_EQUATION;
            this.expression = expression;
            this.bounds = bounds;
            this.curvesPerSurface = curvesPerSurface;
            this.pointsPerCurve = pointsPerCurve;
            dimension = bounds.Count + 1;

            if (!DetermineEquationType())
            {
                success = false;
                RhinoApp.WriteLine("Failed to create equation object.");
            }
            else
                success = true;

            rhinoObjects.Initialize();
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
        /// <returns></returns>
        protected override bool DetermineEquationType()
        {
            // Tests to make sure iteration values are valid
            if (pointsPerCurve < 2 || pointsPerCurve > 999)
            {
                RhinoApp.WriteLine("Points per curve must be bigger than 1 and less than 999.");
                return false;
            }
            if (curvesPerSurface < 2 || curvesPerSurface > 999)
            {
                RhinoApp.WriteLine("Curves per surface must be bigger than 1 and less than 999.");
                return false;
            }

            // List of variables that pertains to equation type
            List<string> vars = new List<string>();

            // Determines what kind of equation the expression is
            /*
             * theta + phi (2D)
             */
            if (ValidVariables(cartesianVars) && TestEquals(cartesianVars))
            {
                equationType = Type.CARTESIAN;
                vars = cartesianVars;
            }
            else if (ValidVariables(sphericalVars) && TestEquals(sphericalVars))
            {
                equationType = Type.SPHERICAL;
                vars = sphericalVars;
            }
            else if (ValidVariables(cylindricalVars) && TestEquals(cylindricalVars))
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
                RhinoApp.WriteLine("Invalid variables. Make sure variables correspond to either cartesian, spherical, or cylindrical coordinates.");
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
            // dimension and equation type. Figuring out which variables are
            // used.
            switch (dimension)
            {
                case 2:
                    vars.RemoveAt(vars.Count - 1);

                    if ((exists[0] || ContainsRightVariables(new List<string>() { vars[0] })) && ValidEquals(vars, VariablesUsed.ONE) && !ContainsWrongVariables(new List<string>() { vars[0] }))
                    {
                        variablesUsed = VariablesUsed.ONE;
                        simplifiedEq += vars[0];
                    }
                    else if ((exists[1] || ContainsRightVariables(new List<string>() { vars[1] })) && ValidEquals(vars, VariablesUsed.TWO) && !ContainsWrongVariables(new List<string>() { vars[1] }))
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
                    }
                    RhinoApp.WriteLine("\ntheta exists: " + exists[0]);
                    RhinoApp.WriteLine("phi exists: " + exists[2]);
                    RhinoApp.WriteLine("anything other than theta and phi: " + ContainsWrongVariables(new List<string>() { vars[2], vars[0] }, true));
                    RhinoApp.WriteLine("valid equals (one_two): " + ValidEquals(vars, VariablesUsed.ONE_TWO));
                    RhinoApp.WriteLine("valid equals (one_three): " + ValidEquals(vars, VariablesUsed.ONE_THREE));
                    */
                    if (equationType == Equation.Type.SPHERICAL && (exists[0] || ContainsRightVariables(new List<string>() { vars[0] })) && ValidEquals(vars, VariablesUsed.ONE_THREE) && !ContainsWrongVariables(new List<string>() { vars[0] }))
                    {
                        variablesUsed = VariablesUsed.ONE_THREE;
                        simplifiedEq += vars[0] + "*" + vars[2];
                    }
                    else if (equationType == Equation.Type.SPHERICAL && (exists[1] || ContainsRightVariables(new List<string>() { vars[1] })) && ValidEquals(vars, VariablesUsed.TWO_THREE) && !ContainsWrongVariables(new List<string>() { vars[1] }))
                    {
                        variablesUsed = VariablesUsed.TWO_THREE;
                        simplifiedEq += vars[1] + "*" + vars[2];
                    }
                    else if ((exists[0] || exists[1] || !ContainsWrongVariables(new List<string>() { vars[0], vars[1] })) && ValidEquals(vars, VariablesUsed.ONE_TWO) && !ContainsWrongVariables(new List<string>() { vars[0], vars[1] }))
                    {
                        variablesUsed = VariablesUsed.ONE_TWO;
                        simplifiedEq += vars[0] + "*" + vars[1];
                    }
                    else if ((exists[2] || exists[0] || !ContainsWrongVariables(new List<string>() { vars[2], vars[0] })) && ValidEquals(vars, VariablesUsed.ONE_THREE) && !ContainsWrongVariables(new List<string>() { vars[2], vars[0] }))
                    {
                        variablesUsed = VariablesUsed.ONE_THREE;
                        simplifiedEq += vars[2] + "*" + vars[0];
                    }
                    else if ((exists[1] || exists[2] || !ContainsWrongVariables(new List<string>() { vars[1], vars[2] })) && ValidEquals(vars, VariablesUsed.TWO_THREE) && !ContainsWrongVariables(new List<string>() { vars[1], vars[2] }))
                    {
                        variablesUsed = VariablesUsed.TWO_THREE;
                        simplifiedEq += vars[1] + "*" + vars[2];
                    }

                    break;
                default:
                    RhinoApp.WriteLine("Dimension outside scope of program. Accepted values are 2 and 3.");
                    return false;
            }

            RhinoApp.WriteLine("\nExpression:  " + expression);
            RhinoApp.WriteLine("Equation Type:  " + equationType);
            RhinoApp.WriteLine("Variables Used:  " + variablesUsed);
            RhinoApp.Write("Decided vars: ");

            for (int i = 0; i < dimension; i++)
                RhinoApp.Write(vars[i] + ", ");

            RhinoApp.WriteLine();

            if (variablesUsed == VariablesUsed.NONE)
            {
                RhinoApp.WriteLine("\nInvalid variables. Make sure variables correspond to either cartesian, spherical, or cylindrical coordinates.");
                return false;
            }

            expression = simplifiedEq;

            switch (variablesUsed)
            {
                case VariablesUsed.ONE:
                    this.vars.Add(vars[0]);

                    break;
                case VariablesUsed.TWO:
                    this.vars.Add(vars[1]);

                    break;
                case VariablesUsed.ONE_TWO:
                    this.vars.Add(vars[0]);
                    this.vars.Add(vars[1]);

                    break;
                case VariablesUsed.ONE_THREE:
                    this.vars.Add(vars[0]);
                    this.vars.Add(vars[2]);

                    break;
                case VariablesUsed.TWO_THREE:
                    this.vars.Add(vars[1]);
                    this.vars.Add(vars[2]);

                    break;
            }

            return ValidExpression(new Expression(expression));
        }

        /// <summary>
        /// Tests if the equals part of the expression with all of the
        /// VariabledUsed components to see if it is valid.
        /// </summary>
        /// <param name="vars"></param>
        /// <returns></returns>
        private bool TestEquals(List<string> vars)
        {
            return ValidEquals(vars, VariablesUsed.ONE) || ValidEquals(vars, VariablesUsed.TWO) || ValidEquals(vars, VariablesUsed.ONE_TWO) || ValidEquals(vars, VariablesUsed.ONE_THREE) || ValidEquals(vars, VariablesUsed.TWO_THREE);
        }

        /// <summary>
        /// Checks to see if the equals part of the expression is valid.
        /// </summary>
        /// <param name="vars"></param>
        /// <param name="variablesUsed"></param>
        /// <returns></returns>
        private bool ValidEquals(List<string> vars, VariablesUsed variablesUsed)
        {
            if (!expression.Contains('='))
                return true;
            
            switch (variablesUsed)
            {
                case VariablesUsed.ONE:
                    return expression.StartsWith(vars[1] + "=", StringComparison.Ordinal) || expression.StartsWith("f(" + vars[0] + ")=", StringComparison.Ordinal) || expression.StartsWith(vars[1] + "(" + vars[0] + ")=", StringComparison.Ordinal);
                case VariablesUsed.TWO:
                    return expression.StartsWith(vars[0] + "=", StringComparison.Ordinal) || expression.StartsWith("f(" + vars[1] + ")=", StringComparison.Ordinal) || expression.StartsWith(vars[0] + "(" + vars[1] + ")=", StringComparison.Ordinal);
                case VariablesUsed.ONE_TWO:
                    return expression.StartsWith(vars[2] + "=", StringComparison.Ordinal) || expression.StartsWith("f(" + vars[0] + ',' + vars[1] + ")=", StringComparison.Ordinal) || expression.StartsWith("f(" + vars[1] + ',' + vars[0] + ")=", StringComparison.Ordinal) || expression.StartsWith(vars[2] + "(" + vars[0] + ',' + vars[1] + ")=", StringComparison.Ordinal) || expression.StartsWith(vars[2] + "(" + vars[1] + ',' + vars[0] + ")=", StringComparison.Ordinal);
                case VariablesUsed.ONE_THREE:
                    return expression.StartsWith(vars[1] + "=", StringComparison.Ordinal) || expression.StartsWith("f(" + vars[0] + ',' + vars[2] + ")=", StringComparison.Ordinal) || expression.StartsWith("f(" + vars[2] + ',' + vars[0] + ")=", StringComparison.Ordinal) || expression.StartsWith(vars[1] + "(" + vars[0] + ',' + vars[2] + ")=", StringComparison.Ordinal) || expression.StartsWith(vars[1] + "(" + vars[2] + ',' + vars[0] + ")=", StringComparison.Ordinal);
                case VariablesUsed.TWO_THREE:
                    return expression.StartsWith(vars[0] + "=", StringComparison.Ordinal) || expression.StartsWith("f(" + vars[1] + ',' + vars[2] + ")=", StringComparison.Ordinal) || expression.StartsWith("f(" + vars[2] + ',' + vars[1] + ")=", StringComparison.Ordinal) || expression.StartsWith(vars[0] + "(" + vars[1] + ',' + vars[2] + ")=", StringComparison.Ordinal) || expression.StartsWith(vars[0] + "(" + vars[2] + ',' + vars[1] + ")=", StringComparison.Ordinal);
            }

            return false;
        }

        /// <summary>
        /// Checks to see if the expression is valid.
        /// </summary>
        /// <param name="vars"></param>
        /// <returns></returns>
        private bool ValidVariables(List<string> vars)
        {
            return ContainsRightVariables(vars) && !ContainsWrongVariables(vars);
        }

        /// <summary>
        /// Looks to see if valid vairables exist in expression.
        /// </summary>
        /// <param name="vars"></param>
        /// <returns></returns>
        private bool ContainsRightVariables(List<string> vars)
        {
            string eq = expression.Substring(expression.IndexOf('=') + 1);

            if (eq.Length == 0)
                return false;

            RemoveMathFunctions(ref eq);
            
            foreach (string v in vars)
                if (eq.Contains(v))
                    return true;

            return false;
        }

        /// <summary>
        /// Checks for letters that shouldn't be there.
        /// </summary>
        /// <param name="vars"></param>
        /// <returns></returns>
        private bool ContainsWrongVariables(List<string> vars)
        {
            string eq = expression.Substring(expression.IndexOf('=') + 1);

            if (eq.Length == 0)
                return true;

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
        /// <param name="eq"></param>
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
        /// <returns></returns>
        public override string ToString()
        {
            return dimension + "D " + equationType + " Equation:\n\t\"" + expression + "\"\n";
        }

        /// <summary>
        /// Creates equation objects.
        /// </summary>
        public override void Generate(RhinoDoc doc)
        {
            if (success)
            {
                var eq = new Expression(expression);
                SetParameters(ref eq);

                double pointIteration = (bounds[0].max - bounds[0].min) / pointsPerCurve;
                double curveIteration = 1;
                double result = 0;

                if (dimension == 2)
                    bounds.Add(equationType == Type.SPHERICAL ? new Bounds(Math.PI / 2, 2) : new Bounds(0, 0));
                else if (dimension == 3)
                    curveIteration = (bounds[1].max - bounds[1].min) / curvesPerSurface;

                int pointDecimalCount = Math.Max(Calculate.DecimalCount(pointIteration), Calculate.DecimalCount(bounds[0].min));
                int curveDecimalCount = Math.Max(Calculate.DecimalCount(curveIteration), Calculate.DecimalCount(bounds[1].min)); ;

                var wireframe = new Objects.Wireframe();

                for (double varTwo = bounds[1].min; varTwo <= bounds[1].max; varTwo += curveIteration)
                {
                    varTwo = Math.Round(varTwo, curveDecimalCount);

                    if (dimension == 3)
                        eq.Parameters[vars[1]] = varTwo;

                    var curve = new Objects.Polyline();

                    for (double varOne = bounds[0].min; varOne <= bounds[0].max; varOne += pointIteration)
                    {
                        varOne = Math.Round(varOne, pointDecimalCount);
                        eq.Parameters[vars[0]] = varOne;

                        result = Convert.ToDouble(eq.Evaluate());

                        if (dimension == 2 && (Double.IsNaN(result) || Double.IsInfinity(result)))
                            curve.Add(Objects.Point.NaN);
                        else
                        {
                            if (Double.IsPositiveInfinity(result))
                                result = Double.MaxValue;
                            else if (Double.IsNegativeInfinity(result))
                                result = Double.MinValue;
                            else if (Double.IsNaN(result))
                                result = 0;

                            var functionResult = ExpressionResult(eq, varOne, (dimension == 3) ? varTwo : result, (dimension == 3) ? result : varTwo);

                            //@ replace these lines
                            functionResult.X = Math.Min(functionResult.X, maxValues["X"].max);
                            functionResult.X = Math.Max(functionResult.X, maxValues["X"].min);
                            functionResult.Y = Math.Min(functionResult.Y, maxValues["Y"].max);
                            functionResult.Y = Math.Max(functionResult.Y, maxValues["Y"].min);
                            functionResult.Z = Math.Min(functionResult.Z, maxValues["Z"].max);
                            functionResult.Z = Math.Max(functionResult.Z, maxValues["Z"].min);

                            curve.Add(functionResult);
                        }
                    }

                    if (wrapPoints)
                        curve.Add(curve[0]);

                    wireframe.uCurves.Add(curve);
                }

                if (wrapCurves && dimension == 3)
                    wireframe.uCurves.Add(wireframe.uCurves[0]);

                if (dimension == 3)
                    wireframe.MakeVFromU();

                CreateRhinoObjects(doc, wireframe);
            }
            else
                RhinoApp.WriteLine("Unable to generate equation.");
        }
    }

    /// <summary>
    /// Parametric equation class (two/three expression lines).
    /// </summary>
    public sealed class ParametricEquation : Equation
    {
        /// <summary>
        /// Custructs a StandardEquation from a string that represents an
        /// expression, and a List of Bounds that represents the bounds for each
        /// variable. The equation dimension is then found by the number of
        /// elements in the list.
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="bounds"></param>
        public ParametricEquation(string expression, List<Bounds> bounds, int pointsPerCurve = 20, int curvesPerSurface = 20)
        {
            plotType = Plot.Type.STANDARD_EQUATION;
            this.expression = expression;
            this.bounds = bounds;
            this.curvesPerSurface = curvesPerSurface;
            this.pointsPerCurve = pointsPerCurve;
            dimension = bounds.Count + 1;

            if (!DetermineEquationType())
            {
                success = false;
                RhinoApp.WriteLine("Failed to create equation object.");
            }
            else
                success = true;

            rhinoObjects.Initialize();
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
        /// <returns></returns>
        protected override bool DetermineEquationType()
        {
            // Tests to make sure iteration values are valid
            if (pointsPerCurve < 2 || pointsPerCurve > 999)
            {
                RhinoApp.WriteLine("Points per curve must be bigger than 1 and less than 999.");
                return false;
            }
            if (curvesPerSurface < 2 || curvesPerSurface > 999)
            {
                RhinoApp.WriteLine("Curves per surface must be bigger than 1 and less than 999.");
                return false;
            }

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
                RhinoApp.WriteLine("Invalid variables. Make sure variables correspond to either cartesian, spherical, or cylindrical coordinates.");
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

                    if (((exists[0] || ContainsRightVariables(new List<string>() { vars[0] })) && (expression.StartsWith(vars[1] + "=", StringComparison.Ordinal) || expression.StartsWith("f(" + vars[0] + ")=", StringComparison.Ordinal) || expression.StartsWith(vars[1] + "(" + vars[0] + ")=", StringComparison.Ordinal) || !equalsExists)) && ContainsRightVariables(new List<string>() { vars[0] }))
                    {
                        variablesUsed = VariablesUsed.ONE;
                        simplifiedEq += vars[0];
                    }
                    else if (((exists[1] || ContainsRightVariables(new List<string>() { vars[1] })) && (expression.StartsWith(vars[0] + "=", StringComparison.Ordinal) || expression.StartsWith("f(" + vars[1] + ")=", StringComparison.Ordinal) || expression.StartsWith(vars[1] + "(" + vars[1] + ")=", StringComparison.Ordinal) || !equalsExists)) && ContainsRightVariables(new List<string>() { vars[1] }))
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
                    if (((exists[0] || exists[1] || !ContainsWrongVariables(new List<string>() { vars[0], vars[1] })) && (expression.StartsWith(vars[2] + "=", StringComparison.Ordinal) || expression.StartsWith("f(" + vars[0] + "," + vars[1] + ")=", StringComparison.Ordinal) || !equalsExists)) && !ContainsWrongVariables
                        (new List<string>() { vars[0], vars[1] }))
                    {
                        variablesUsed = VariablesUsed.ONE_TWO;
                        simplifiedEq += vars[0] + "*" + vars[1];
                    }
                    else if (((exists[1] || exists[2] || !ContainsWrongVariables(new List<string>() { vars[1], vars[2] })) && (expression.StartsWith(vars[0] + "=", StringComparison.Ordinal) || expression.StartsWith("f(" + vars[1] + "," + vars[2] + ")=", StringComparison.Ordinal) || !equalsExists)) && !ContainsWrongVariables(new List<string>() { vars[1], vars[2] }))
                    {
                        variablesUsed = VariablesUsed.TWO_THREE;
                        simplifiedEq += vars[1] + "*" + vars[2];
                    }
                    else if (((exists[2] || exists[0] || !ContainsWrongVariables(new List<string>() { vars[2], vars[0] })) && (expression.StartsWith(vars[1] + "=", StringComparison.Ordinal) || expression.StartsWith("f(" + vars[2] + "," + vars[0] + ")=", StringComparison.Ordinal) || !equalsExists)) && !ContainsWrongVariables(new List<string>() { vars[2], vars[0] }))
                    {
                        variablesUsed = VariablesUsed.ONE_THREE;
                        simplifiedEq += vars[2] + "*" + vars[0];
                    }

                    break;
                default:
                    RhinoApp.WriteLine("Dimension outside scope of program. Accepted values are 2 and 3.");
                    return false;
            }

            if (variablesUsed == VariablesUsed.NONE)
            {
                RhinoApp.WriteLine("Invalid variables. Make sure variables correspond to either cartesian, spherical, or cylindrical coordinates.");
                return false;
            }

            expression = simplifiedEq;

            switch (variablesUsed)
            {
                case VariablesUsed.ONE:
                    this.vars.Add(vars[0]);

                    break;
                case VariablesUsed.TWO:
                    this.vars.Add(vars[1]);

                    break;
                case VariablesUsed.ONE_TWO:
                    this.vars.Add(vars[0]);
                    this.vars.Add(vars[1]);

                    break;
                case VariablesUsed.ONE_THREE:
                    this.vars.Add(vars[0]);
                    this.vars.Add(vars[2]);

                    break;
                case VariablesUsed.TWO_THREE:
                    this.vars.Add(vars[1]);
                    this.vars.Add(vars[2]);

                    break;
            }

            RhinoApp.WriteLine("\nEquation Type:  " + equationType);
            RhinoApp.WriteLine("Variables Used:  " + variablesUsed);
            RhinoApp.WriteLine("Expression:  " + expression);

            RhinoApp.Write("Vars: ");
            for (int i = 0; i < this.vars.Count; i++)
                RhinoApp.Write(this.vars[i] + ", ");

            return ValidExpression(new Expression(expression));
        }

        /// <summary>
        /// Checks to see if the expression is valid.
        /// </summary>
        /// <param name="vars"></param>
        /// <returns></returns>
        private bool ValidVariables(List<string> vars)
        {
            RhinoApp.WriteLine("\nIs Right: " + ContainsRightVariables(vars));
            RhinoApp.WriteLine("Is Wrong: " + ContainsWrongVariables(vars));

            return /**/ContainsRightVariables(vars) && !ContainsWrongVariables(vars);
        }

        /// <summary>
        /// Looks to see if valid vairables exist in expression.
        /// </summary>
        /// <param name="vars"></param>
        /// <returns></returns>
        private bool ContainsRightVariables(List<string> vars)
        {
            /*
            foreach (string s in vars)
                if (expression.StartsWith(s + "=") || expression.StartsWith("f(" + s + ")="))
                    return true;
            */
            string eq = expression;

            if (eq.Length == 0)
                return false;

            RemoveMathFunctions(ref eq);
            RhinoApp.WriteLine("\tAfter removed math: " + eq);
            foreach (string v in vars)
                if (eq.Contains(v))
                    return true;

            return false;
        }

        /// <summary>
        /// Checks for letters that shouldn't be there.
        /// </summary>
        /// <param name="vars"></param>
        /// <returns></returns>
        private bool ContainsWrongVariables(List<string> vars)
        {
            string eq = expression;

            if (eq.Length == 0)
                return true;

            RemoveMathFunctions(ref eq);
            RhinoApp.WriteLine("\tAfter removed math: " + eq);
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
        /// <param name="eq"></param>
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
        /// <returns></returns>
        public override string ToString()
        {
            return dimension + "D " + equationType + " Equation:\n\t\"" + expression + "\"\n";
        }

        /// <summary>
        /// Creates equation objects.
        /// </summary>
        public override void Generate(RhinoDoc doc)
        {
            if (success)
            {
                var eq = new Expression(expression);
                SetParameters(ref eq);

                double pointIteration = (bounds[0].max - bounds[0].min) / pointsPerCurve;
                double curveIteration = 1;
                double result = 0;

                if (dimension == 2)
                    bounds.Add(equationType == Type.SPHERICAL ? new Bounds(Math.PI / 2, 2) : new Bounds(0, 0));
                else if (dimension == 3)
                    curveIteration = (bounds[1].max - bounds[1].min) / curvesPerSurface;

                int pointDecimalCount = Math.Max(Calculate.DecimalCount(pointIteration), Calculate.DecimalCount(bounds[0].min));
                int curveDecimalCount = Math.Max(Calculate.DecimalCount(curveIteration), Calculate.DecimalCount(bounds[1].min)); ;

                var wireframe = new Objects.Wireframe();

                for (double varTwo = bounds[1].min; varTwo <= bounds[1].max; varTwo += curveIteration)
                {
                    varTwo = Math.Round(varTwo, curveDecimalCount);

                    if (dimension == 3)
                        eq.Parameters[vars[1]] = varTwo;

                    var curve = new Objects.Polyline();

                    for (double varOne = bounds[0].min; varOne <= bounds[0].max; varOne += pointIteration)
                    {
                        varOne = Math.Round(varOne, pointDecimalCount);
                        eq.Parameters[vars[0]] = varOne;

                        result = Convert.ToDouble(eq.Evaluate());

                        if (dimension == 2 && (Double.IsNaN(result) || Double.IsInfinity(result)))
                            curve.Add(Objects.Point.NaN);
                        else
                        {
                            if (Double.IsPositiveInfinity(result))
                                result = Double.MaxValue;
                            else if (Double.IsNegativeInfinity(result))
                                result = Double.MinValue;
                            else if (Double.IsNaN(result))
                                result = 0;

                            var functionResult = ExpressionResult(eq, varOne, (dimension == 3) ? varTwo : result, (dimension == 3) ? result : varTwo);

                            //@ replace these lines
                            functionResult.X = Math.Min(functionResult.X, maxValues["X"].max);
                            functionResult.X = Math.Max(functionResult.X, maxValues["X"].min);
                            functionResult.Y = Math.Min(functionResult.Y, maxValues["Y"].max);
                            functionResult.Y = Math.Max(functionResult.Y, maxValues["Y"].min);
                            functionResult.Z = Math.Min(functionResult.Z, maxValues["Z"].max);
                            functionResult.Z = Math.Max(functionResult.Z, maxValues["Z"].min);

                            curve.Add(functionResult);
                        }
                    }

                    if (wrapPoints)
                        curve.Add(curve[0]);

                    wireframe.uCurves.Add(curve);
                }

                if (wrapCurves && dimension == 3)
                    wireframe.uCurves.Add(wireframe.uCurves[0]);

                if (dimension == 3)
                    wireframe.MakeVFromU();

                CreateRhinoObjects(doc, wireframe);
            }
            else
                RhinoApp.WriteLine("Unable to generate equation.");
        }
    }
}
