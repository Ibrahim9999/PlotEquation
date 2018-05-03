using System;
using System.Collections.Generic;
using Rhino;
using Rhino.Geometry;
using System.Linq;
//@ Add names to points in rhino
//@ Fix 3D Cylindrical variable parsing
//@ theta returns phi=theta
//@ phi returns theta = phi
//@ error with r=theta
//@ weird surfaces r=cos(phi)^2
//@ add curved quad option
//@ add just number option
namespace PlotEquation
{
    /// <summary>
    /// Base mathematical object class for PlotEquation.
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
            public class Point
            {
                public double X;
                public double Y;
                public double Z;

                public Point()
                {
                    X = 0;
                    Y = 0;
                    Z = 0;
                }
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

                public static Point Origin => new Point(0, 0, 0);
                public static Point NaN => new Point(Double.NaN, Double.NaN, Double.NaN);

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

                public static Point operator -(Point a, Point b)
                {
                    return new Point(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
                }

                public static Point operator *(double d, Point p)
                {
                    return new Point(d * p.X, d * p.Y, d * p.Z);
                }

                public static Point operator /(Point p, double d)
                {
                    return new Point(p.X / d, p.Y / d, p.Z / d);
                }

                public static Point operator /(double d, Point p)
                {
                    return new Point(d / p.X, d / p.Y, d / p.Z);
                }

                public double Magnitude()
                {
                    return Math.Sqrt(X * X + Y * Y + Z * Z);
                }
            }
            
            /// <summary>
            /// 2D list of points in 3D space.
            /// </summary>
            public class Grid
            {
                private List<List<Point>> points;

                public Grid()
                {
                    points = new List<List<Point>>();
                }
                public Grid(int u, int v)
                {
                    points = new List<List<Point>>();

                    Reset(u, v);
                }
                public Grid(List<List<Point>> points)
                {
                    this.points = points;
                }

                public List<Point> this[int index]
                {
                    get { return points[index]; }

                    set { points[index] = value; }
                }

                public Point this[int i, int j]
                {
                    get { return points[i][j]; }

                    set { points[i][j] = value; }
                }

                public int Count
                {
                    get { return points.Count; }
                }

                /// <summary>
                /// Creates a new u x v grid of points.
                /// </summary>
                /// <param name="u"></param>
                /// <param name="v"></param>
                public void Reset(int u, int v)
                {
                    points = new List<List<Point>>();

                    for (int i = 0; i < u; i++)
                    {
                        points.Add(new List<Point>());

                        for (int j = 0; j < v; j++)
                            points[i].Add(Point.Origin);
                    }
                }

                /// <summary>
                /// Changes value of point at index in grid.
                /// </summary>
                public void Change(int i, int j, Point point)
                {
                    points[i][j] = point;
                }
                /// <summary>
                /// Changes value of point at index in grid.
                /// </summary>
                public void Change(int i, int j, int x, int y, int z)
                {
                    points[i][j] = new Point(x, y, z);
                }

                /// <summary>
                /// Creates a Grid from a Wirefram object.
                /// </summary>
                public void FromWireframe(Wireframe wireframe, bool deleteOldPoints = true)
                {
                    if (deleteOldPoints)
                        points = new List<List<Point>>();

                    var polylines = (wireframe.uCurves.Count == 0) ? wireframe.vCurves : wireframe.uCurves;

                    foreach (Polyline polyline in polylines)
                    {
                        var p = new List<Point>();

                        foreach (Point point in polyline.Verticies)
                            p.Add(point);

                        points.Add(p);
                    }
                }
            }

            /// <summary>
            /// An object that represents the line between two points.
            /// </summary>
            public class Line
            {
                public Point start;
                public Point end;

                public Line()
                {
                    start = Point.Origin;
                    end = Point.Origin;
                }
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

                public Point this[int i]
                {
                    get
                    {
                        if (i == 0)
                            return start;
                        if (i == 1)
                            return end;

                        Point difference = end - start;

                        return i * ((difference) / difference.Magnitude());
                    }
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
            /// A list of points that represent verticies in a lines.
            /// </summary>
            public class Polyline
            {
                private List<Point> verticies;

                public Polyline()
                {
                    verticies = new List<Point>();
                }
                public Polyline(List<Point> polyline)
                {
                    verticies = polyline;
                }

                public List<Point> Verticies => verticies;

                public override int GetHashCode()
                {
                    return verticies.GetHashCode();
                }

                public int Count
                {
                    get { return verticies.Count; }
                }
                
                public Point this[int index]
                {
                    get { return verticies[index]; }

                    set { verticies[index] = value; }
                }

                public override bool Equals(object obj)
                {
                    return this == (Polyline)obj;
                }

                public static bool operator ==(Polyline a, Polyline b)
                {
                    if (a.Count != b.Count)
                        return false;

                    for (int i = 0; i < a.Count; i++)
                        if (a[i] != b[i])
                            return false;

                    return true;
                }

                public static bool operator !=(Polyline a, Polyline b)
                {
                    return !(a == b);
                }

                /// <summary>
                /// Converts Polyline to a list of Lines.
                /// </summary>
                public List<Line> ToLines()
                {
                    var lines = new List<Line>();

                    for (int i = 1; i < verticies.Count; i++)
                        lines.Add(new Line(verticies[i - 1], verticies[i]));

                    return lines;
                }

                /// <summary>
                /// Adds a point to verticies.
                /// </summary>
                public void Add(Point point)
                {
                    verticies.Add(point);
                }
                /// <summary>
                /// Adds a line to verticies.
                /// </summary>
                /// <remarks>
                /// If the new line does not connect to the existing polyline,
                /// both the start and end points will be added to verticies.
                /// </remarks>
                public void Add(Line line)
                {
                    if (line.start != verticies.Last())
                        verticies.Add(line.start);

                    verticies.Add(line.end);
                }

                /// <summary>
                /// Changes value of point at index.
                /// </summary>
                public void Change(int index, Point point)
                {
                    verticies[index] = point;
                }
                /// <summary>
                /// Changes value of point at index.
                /// </summary>
                public void Change(int index, int x, int y, int z)
                {
                    verticies[index] = new Point(x, y, z);
                }

                /// <summary>
                /// Removes point from verticies.
                /// </summary>
                public void Remove(Point point)
                {
                    verticies.Remove(point);
                }
                /// <summary>
                /// Removes point from verticies at index.
                /// </summary>
                public void Remove(int index)
                {
                    verticies.RemoveAt(index);
                }
            }

            /// <summary>
            /// A 2D grid of lines in 3D space.
            /// </summary>
            public class Wireframe
            {
                public List<Polyline> uCurves;
                public List<Polyline> vCurves;

                public Wireframe()
                {
                    uCurves = new List<Polyline>();
                    vCurves = new List<Polyline>();
                }
                public Wireframe(List<Polyline> uCurves)
                {
                    this.uCurves = uCurves;
                    vCurves = new List<Polyline>();

                    MakeVFromU();
                }

                /// <summary>
                /// Converts the Wireframe into a list of points.
                /// </summary>
                public List<Point> ToPoints()
                {
                    var polylines = (uCurves.Count == 0) ? vCurves : uCurves;
                    var points = new List<Point>();

                    foreach (Polyline polyline in polylines)
                        foreach (Point point in polyline.Verticies)
                            points.Add(point);

                    return points;
                }

                /// <summary>
                /// Converts the Wireframe into a Grid.
                /// </summary>
                /// <param name="useU"></param>
                /// <returns></returns>
                public Grid ToGrid(bool useU = true)
                {
                    List<Polyline> p = useU ? uCurves : vCurves;
                    Grid g = new Grid();

                    g.Reset(p.Count, p[0].Count);

                    for (int i = 0; i < p.Count; i++)
                        for (int j = 0; j < p[i].Count; j++)
                            g[i][j] = uCurves[i][j];

                    return g;
                }

                /// <summary>
                /// Converts the Wireframe into a list of lines.
                /// </summary>
                public List<Line> ToLines()
                {
                    var lines = new List<Line>();

                    foreach (Polyline polyline in uCurves)
                        lines.AddRange(polyline.ToLines());
                    foreach (Polyline polyline in vCurves)
                        lines.AddRange(polyline.ToLines());

                    return lines;
                }

                /// <summary>
                /// Generates Wireframe from Grid.
                /// </summary>
                public void FromGrid(Grid grid, bool deleteOldCurves = true)
                {
                    if (deleteOldCurves)
                    {
                        uCurves = new List<Polyline>();
                        vCurves = new List<Polyline>();
                    }

                    for (int i = 0; i < grid.Count; i++)
                        for (int j = 0; j < grid[i].Count; j++)
                            uCurves[i].Add(grid[i][j]);

                    MakeVFromU();
                }
                
                /// <summary>
                /// Generates U curves from V curves.
                /// </summary>
                public void MakeUFromV()
                {
                    uCurves = new List<Polyline>();

                    for (int i = 0; i < vCurves[0].Count; i++)
                    {
                        var polyline = new Polyline();

                        for (int e = 0; e < vCurves.Count; e++)
                            polyline.Add(vCurves[e][i]);

                        uCurves.Add(polyline);
                    }
                }

                /// <summary>
                /// Generates V curves from U curves.
                /// </summary>
                public void MakeVFromU()
                {
                    vCurves = new List<Polyline>();

                    for (int i = 0; i < uCurves[0].Count; i++)
                    {
                        var polyline = new Polyline();

                        for (int e = 0; e < uCurves.Count; e++)
                            polyline.Add(uCurves[e][i]);

                        vCurves.Add(polyline);
                    }
                }
            }

            /// <summary>
            /// A collection of 3D points that represent verticies of a triangle.
            /// </summary>
            public class Triangle
            {
                private Point[] verticies;

                public Triangle()
                {
                    verticies = new Point[3];

                    verticies[0] = Point.Origin;
                    verticies[1] = Point.Origin;
                    verticies[2] = Point.Origin;
                }
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

                /// <summary>
                /// Changes vertex position in Triangle at vertexIndex.
                /// </summary>
                public void AdjustVertex(int vertexIndex, Point vertex)
                {
                    verticies[vertexIndex] = vertex;
                }
                /// <summary>
                /// Changes vertex values in Triangle at vertexIndex.
                /// </summary>
                public void AdjustVertex(int vertexIndex, double x, double y, double z)
                {
                    verticies[vertexIndex] = new Point(x, y, z);
                }
            }

            /// <summary>
            /// A collection of 3D points that represent verticies of a quad.
            /// </summary>
            public class Quad
            {
                public Point[] verticies;

                public Quad()
                {
                    verticies = new Point[4];

                    verticies[0] = Point.Origin;
                    verticies[1] = Point.Origin;
                    verticies[2] = Point.Origin;
                    verticies[3] = Point.Origin;

                }
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

            //@ Make trianglemesh and quadmesh more like lists
            /// <summary>
            /// A collection of Triangles that represent a mesh.
            /// </summary>
            public class TriangleMesh
            {
                public List<Triangle> triangles;

                public TriangleMesh()
                {
                    triangles = new List<Triangle>();
                }
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

                public static TriangleMesh MakeFromWireframe(Wireframe wireframe)
                {
                    var mesh = new TriangleMesh();
                    mesh.triangles = new List<Triangle>();
                    var polylines = (wireframe.uCurves.Count == 0) ? wireframe.vCurves : wireframe.uCurves;

                    for (int i = 1; i < polylines.Count; i++)
                        for (int e = 1; e < polylines[i].Count; e++)
                        {
                            mesh.triangles.Add(new Triangle(polylines[i - 1][e - 1], polylines[i][e - 1], polylines[i][e]));
                            mesh.triangles.Add(new Triangle(polylines[i - 1][e - 1], polylines[i][e], polylines[i - 1][e]));
                        }

                    return mesh;
                }

                public static TriangleMesh MakeFromQuadMesh(QuadMesh mesh)
                {
                    TriangleMesh triMesh = new TriangleMesh();
                    triMesh.triangles = new List<Triangle>();

                    for (int i = 1; i < mesh.Count; i++)
                    {
                        triMesh.triangles.Add(new Triangle(mesh[i][0], mesh[i][1], mesh[i][2]));
                        triMesh.triangles.Add(new Triangle(mesh[i][0], mesh[i][3], mesh[i][2]));
                    }

                    return triMesh;
                }
            }

            /// <summary>
            /// A collection of Quads that represent a mesh.
            /// </summary>
            public class QuadMesh
            {
                public List<Quad> quads;

                public QuadMesh()
                {
                    quads = new List<Quad>();
                }
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

                public static QuadMesh MakeFromWireframe(Wireframe wireframe)
                {
                    QuadMesh mesh = new QuadMesh();
                    mesh.quads = new List<Quad>();
                    var polylines = (wireframe.uCurves.Count == 0) ? wireframe.vCurves : wireframe.uCurves;

                    for (int i = 1; i < polylines.Count; i++)
                        for (int e = 1; e < polylines[i].Count; e++)
                            mesh.quads.Add(new Quad(polylines[i - 1][e - 1], polylines[i][e - 1], polylines[i][e], polylines[i - 1][e]));

                    return mesh;
                }

                public static QuadMesh MakeFromTriangleMesh(TriangleMesh mesh)
                {
                    QuadMesh quadMesh = new QuadMesh();
                    quadMesh.quads = new List<Quad>();

                    for (int i = 0; i < mesh.Count; i += 2)
                        quadMesh.quads.Add(new Quad(mesh[i][0], mesh[i][1], mesh[i][2], mesh[i + 1][1]));

                    return quadMesh;
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
            public List<Polyline> polylines;
            public List<Curve> curves;
            public List<List<Polyline>> lineframe;
            public List<List<Curve>> wireframe;
            public List<Brep> triangles;
            public List<Brep> quads;
            public List<Surface> surfaces;

            /// <summary>
            /// Initializes all the lists.
            /// </summary>
            public void Initialize()
            {
                points = new List<Point3d>();
                grid = new List<List<Point3d>>();
                polylines = new List<Polyline>();
                curves = new List<Curve>();
                lineframe = new List<List<Polyline>>();
                wireframe = new List<List<Curve>>();
                triangles = new List<Brep>();
                quads = new List<Brep>();
                surfaces = new List<Surface>();
            }

            /// <summary>
            /// Adds all the objects to Rhino in separate layers.
            /// </summary>
            /// <param name="doc"></param>
            /// <param name="title"></param>
            public void AddAll(RhinoDoc doc, string title = "")
            {
                if (title.Length == 0 || !Rhino.DocObjects.Layer.IsValidName(title))
                    title = doc.Layers.GetUnusedLayerName();

                int index = doc.Layers.Add(title, System.Drawing.Color.Black);
                Rhino.DocObjects.Layer parent = doc.Layers[index];
                
                Rhino.DocObjects.Layer child = new Rhino.DocObjects.Layer();
                child.ParentLayerId = parent.Id;

                RhinoApp.WriteLine("Adding objects to Rhino...");

                if (points.Count != 0)
                {
                    child.Name = "Points";
                    index = doc.Layers.Add(child);
                    doc.Layers.FindIndex(index).IsVisible = false;

                    foreach (Point3d point in points)
                        doc.Objects.AddPoint(point, new Rhino.DocObjects.ObjectAttributes { LayerIndex = index });

                    doc.Views.Redraw();
                }
                if (grid.Count != 0)
                {
                    child.Name = "Grid";
                    index = doc.Layers.Add(child);
                    doc.Layers.FindIndex(index).IsVisible = false;

                    foreach (List<Point3d> list in grid)
                    {
                        List<Guid> guids = new List<Guid>();

                        foreach (Point3d point in list)
                            guids.Add(doc.Objects.AddPoint(point, new Rhino.DocObjects.ObjectAttributes { LayerIndex = index }));

                        doc.Groups.Add(guids);
                    }

                    doc.Views.Redraw();
                }
                if (polylines.Count != 0)
                {
                    child.Name = "Polylines";
                    index = doc.Layers.Add(child);
                    doc.Layers.FindIndex(index).IsVisible = false;

                    foreach (Polyline polyline in polylines)
                        doc.Objects.AddPolyline(polyline, new Rhino.DocObjects.ObjectAttributes { LayerIndex = index });

                    doc.Views.Redraw();
                }
                if (curves.Count != 0)
                {
                    child.Name = "Curves";
                    index = doc.Layers.Add(child);
                    doc.Layers.FindIndex(index).IsVisible = (surfaces.Count == 0);

                    foreach (Curve curve in curves)
                        doc.Objects.AddCurve(curve, new Rhino.DocObjects.ObjectAttributes { LayerIndex = index });

                    doc.Views.Redraw();
                }
                if (lineframe.Count != 0)
                {
                    child.Name = "Lineframe";
                    index = doc.Layers.Add(child);
                    doc.Layers.FindIndex(index).IsVisible = false;

                    foreach (List<Polyline> list in lineframe)
                    {
                        List<Guid> guids = new List<Guid>();
                        
                        foreach (Polyline polyline in list)
                            guids.Add(doc.Objects.AddPolyline(polyline, new Rhino.DocObjects.ObjectAttributes { LayerIndex = index }));

                        doc.Groups.Add(guids);
                    }

                    doc.Views.Redraw();
                }
                if (wireframe.Count != 0)
                {
                    child.Name = "Wireframe";
                    index = doc.Layers.Add(child);
                    doc.Layers.FindIndex(index).IsVisible = false;

                    foreach (List<Curve> list in wireframe)
                    {
                        List<Guid> guids = new List<Guid>();

                        foreach (Curve curve in list)
                            doc.Objects.AddCurve(curve, new Rhino.DocObjects.ObjectAttributes { LayerIndex = index });

                        doc.Groups.Add(guids);
                    }
                }
                if (triangles.Count != 0)
                {
                    child.Name = "Triangles";
                    index = doc.Layers.Add(child);
                    doc.Layers.FindIndex(index).IsVisible = false;

                    foreach (Brep brep in triangles)
                        doc.Objects.AddBrep(brep, new Rhino.DocObjects.ObjectAttributes { LayerIndex = index });

                    doc.Views.Redraw();
                }
                if (quads.Count != 0)
                {
                    child.Name = "Quads";
                    index = doc.Layers.Add(child);
                    doc.Layers.FindIndex(index).IsVisible = false;

                    foreach (Brep brep in quads)
                        doc.Objects.AddBrep(brep, new Rhino.DocObjects.ObjectAttributes { LayerIndex = index });

                    doc.Views.Redraw();
                }
                if (surfaces.Count != 0)
                {
                    child.Name = "Surfaces";
                    index = doc.Layers.Add(child);
                    doc.Layers.FindIndex(index).IsVisible = (quads.Count != 0);

                    foreach (Surface surface in surfaces)
                        doc.Objects.AddSurface(surface, new Rhino.DocObjects.ObjectAttributes { LayerIndex = index });

                    doc.Views.Redraw();
                }
            }

            /// <summary>
            /// Converts a Plot Point to a Rhino Point.
            /// </summary>
            /// <param name="point"></param>
            /// <returns></returns>
            public static Point3d PointToRhino(Objects.Point point)
            {
                return new Point3d(point.X, point.Y, point.Z);
            }

            /// <summary>
            /// Converts a Rhino Point to a Plot Point.
            /// </summary>
            /// <param name="point"></param>
            /// <returns></returns>
            public static Objects.Point Point3dToPlot(Point3d point)
            {
                return new Objects.Point(point.X, point.Y, point.Z);
            }

            /// <summary>
            /// Converts a Plot Grid object to a list of Rhino Point3ds.
            /// </summary>
            /// <param name="grid"></param>
            /// <returns></returns>
            public static List<List<Point3d>> GridToRhino(Objects.Grid grid)
            {
                List<List<Point3d>> g = new List<List<Point3d>>();

                for (int i = 0; i < grid.Count; i++)
                {
                    g.Add(new List<Point3d>());

                    for (int j = 0; j < grid[i].Count; j++)
                        g[i].Add(PointToRhino(grid[i][j]));
                }

                return g;
            }
            
            /// <summary>
            /// Converts a Plot Line to a Rhino Curve.
            /// </summary>
            /// <param name="line"></param>
            /// <returns></returns>
            public static Curve LineToRhino(Objects.Line line)
            {
                return Curve.CreateInterpolatedCurve(new List<Point3d> { PointToRhino(line.start), PointToRhino(line.end) }, 3);
            }

            /// <summary>
            /// Converts a Plot Polyline to a Rhino Polyline.
            /// </summary>
            /// <param name="polyline"></param>
            /// <returns></returns>
            public static Polyline PolylineToRhino(Objects.Polyline polyline)
            {
                List<Point3d> points = new List<Point3d>();

                for (int i = 0; i < polyline.Count; i++)
                    points.Add(PointToRhino(polyline[i]));

                return new Polyline(points);
            }

            /// <summary>
            /// Converts a Plot Polyline to a Rhino Curve.
            /// </summary>
            /// <param name="polyline"></param>
            /// <param name="degree"></param>
            /// <returns></returns>
            public static Curve CurveToRhino(Objects.Polyline polyline, int degree = 3)
            {
                List<Point3d> points = new List<Point3d>();

                for (int i = 0; i < polyline.Count; i++)
                    points.Add(PointToRhino(polyline[i]));

                return Curve.CreateInterpolatedCurve(points, degree);
            }

            /// <summary>
            /// Converts a Plot Wireframe into a 2D list of Rhino Polylines.
            /// </summary>
            /// <param name="wireframe"></param>
            /// <param name="uCurves"></param>
            /// <returns></returns>
            public static List<List<Polyline>> LineframeToRhino(Objects.Wireframe wireframe)
            {
                List<List<Polyline>> w = new List<List<Polyline>>();
                List<Polyline> u = new List<Polyline>();
                List<Polyline> v = new List<Polyline>();

                

                foreach (Objects.Polyline polyline in wireframe.uCurves)
                    u.Add(PolylineToRhino(polyline));
                foreach (Objects.Polyline polyline in wireframe.vCurves)
                    v.Add(PolylineToRhino(polyline));

                w.Add(u);
                w.Add(v);

                return w;
            }

            /// <summary>
            /// Converts a Plot Wireframe into a 2D list of Rhino Curves.
            /// </summary>
            /// <param name="wireframe"></param>
            /// <param name="uCurves"></param>
            /// <returns></returns>
            public static List<List<Curve>> WireframeToRhino(Objects.Wireframe wireframe)
            {
                List<List<Curve>> w = new List<List<Curve>>();
                List<Curve> u = new List<Curve>();
                List<Curve> v = new List<Curve>();

                foreach (Objects.Polyline polyline in wireframe.uCurves)
                    u.Add(CurveToRhino(polyline));
                foreach (Objects.Polyline polyline in wireframe.vCurves)
                    v.Add(CurveToRhino(polyline));

                w.Add(u);
                w.Add(v);

                return w;
            }

            /// <summary>
            /// Converts a Plot Triangle to a Rhino Brep.
            /// </summary>
            /// <param name="triangle"></param>
            /// <param name="tolerance"></param>
            /// <returns></returns>
            public static Brep TriangleToRhino(Objects.Triangle triangle, double tolerance = .00000001)
            {
                return Brep.CreateFromCornerPoints(PointToRhino(triangle[0]), PointToRhino(triangle[1]), PointToRhino(triangle[2]), tolerance);
            }

            /// <summary>
            /// Converts a Plot Quad to a Rhino Brep.
            /// </summary>
            /// <param name="quad"></param>
            /// <param name="tolerance"></param>
            /// <returns></returns>
            public static Brep QuadToRhino(Objects.Quad quad, double tolerance = .00000001)
            {
                return Brep.CreateFromCornerPoints(PointToRhino(quad[0]), PointToRhino(quad[1]), PointToRhino(quad[2]), PointToRhino(quad[3]), tolerance);
            }

            /// <summary>
            /// Converts a Plot TriangleMesh object to a list of Rhino Breps.
            /// </summary>
            /// <param name="mesh"></param>
            /// <returns></returns>
            public static List<Brep> TriangleMeshToRhino(Objects.TriangleMesh mesh)
            {
                List<Brep> list = new List<Brep>();

                for (int i = 0; i < mesh.Count; i++)
                    list.Add(TriangleToRhino(mesh[i]));

                return list;
            }

            /// <summary>
            /// Converts a Plot QuadMesh object to a list of Rhino Breps.
            /// </summary>
            /// <param name="mesh"></param>
            /// <returns></returns>
            public static List<Brep> QuadMeshToRhino(Objects.QuadMesh mesh)
            {
                List<Brep> list = new List<Brep>();

                for (int i = 0; i < mesh.Count; i++)
                    list.Add(QuadToRhino(mesh[i]));

                return list;
            }
        }

        /// <summary>
        /// Bool dictating whether plot generation is safe or not.
        /// </summary>
        protected bool success;

        /// <summary>
        /// Represents the kind of mathematical object the Plot is.
        /// </summary>
        protected Type plotType;

        /// <summary>
        /// Contains the Rhino objects when converted from Plot objects.
        /// </summary>
        protected RhinoObjects rhinoObjects;

        /// <summary>
        /// Any sliders are added here; used to see change over time.
        /// </summary>
        protected Dictionary<string, double> sliders = new Dictionary<string, double>();

        /// <summary>
        /// Returns the mathematical object type of the Plot.
        /// </summary>
        public Type GetPlotType => plotType;

        /// <summary>
        /// Returns the RhinoObjects made when generating a Plot object.
        /// </summary>
        public RhinoObjects GetRhinoObjects() => rhinoObjects;

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
        /// <returns></returns>
        public bool IsEquation()
        {
            return plotType == Type.STANDARD_EQUATION || plotType == Type.PARAMETRIC_EQUATION;
        }

        /// <summary>
        /// Checks Plot to see whether it is a standard equation or not.
        /// </summary>
        /// <returns></returns>
        public bool IsStandardEquation()
        {
            return plotType == Type.STANDARD_EQUATION;
        }

        /// <summary>
        /// Checks Plot to see whether it is a parametric equation or not.
        /// </summary>
        /// <returns></returns>
        public bool IsParametricEquation()
        {
            return plotType == Type.PARAMETRIC_EQUATION;
        }

        /// <summary>
        /// Checks Plot to see whether it is a mandelbrot fractal or not.
        /// </summary>
        /// <returns></returns>
        public bool IsMandelbrot()
        {
            return plotType == Type.MANDELBROT;
        }

        /// <summary>
        /// Checks Plot to see whether it is a strange attractor or not.
        /// </summary>
        /// <returns></returns>
        public bool IsStrangeAttractor()
        {
            return plotType == Type.STRANGE_ATTRACTOR;
        }

        /// <summary>
        /// Adds a slider to the list, which comprises of a variable and value.
        /// </summary>
        /// <param name="variable"></param>
        /// <param name="value"></param>
        public void AddSlider(string variable, double value)
        {
            sliders.Add(variable, value);
        }

        /// <summary>
        /// Removes slider from list based on variable name.
        /// </summary>
        /// <param name="variable"></param>
        public void RemoveSlider(string variable)
        {
            sliders.Remove(variable);
        }

        /// <summary>
        /// Changes a slider value.
        /// </summary>
        /// <param name="variable"></param>
        /// <param name="value"></param>
        public void ChangeSlider(string variable, double value)
        {
            sliders[variable] = value;
        }

        /// <summary>
        /// Updates a slider with an increment.
        /// </summary>
        /// <param name="variable"></param>
        /// <param name="increment"></param>
        public void UpdateSlider(string variable, double increment)
        {
            sliders[variable] += increment;
        }

        /// <summary>
        /// Outputs Plot informtion.
        /// </summary>
        /// <returns></returns>
        public abstract override string ToString();

        /// <summary>
        /// Creates objects.
        /// </summary>
        public abstract void Generate(RhinoDoc doc);
    }
}
