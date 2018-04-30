using System;
using System.Collections.Generic;
using Rhino;
using Rhino.Geometry;
using NCalc;
using System.Linq;
using System.Collections;

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
            public struct Grid
            {
                private List<List<Point>> points;

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
            public struct Polyline
            {
                private List<Point> verticies;

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
            public struct Wireframe
            {
                public List<Polyline> uCurves;
                public List<Polyline> vCurves;

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
            public struct Triangle
            {
                private Point[] verticies;

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
            //@ Make trianglemesh and quadmesh more like lists
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
                if (title.Length == 0 || !Rhino.DocObjects.Layer.IsValidName(title) || doc.Layers.FindByFullPath(title, -1) == -1)
                    title = doc.Layers.GetUnusedLayerName();

                int index = doc.Layers.Add(title, System.Drawing.Color.Black);
                Rhino.DocObjects.Layer parent = doc.Layers[index];
                
                Rhino.DocObjects.Layer child = new Rhino.DocObjects.Layer();
                child.ParentLayerId = parent.Id;

                if (points.Count != 0)
                {
                    child.Name = "Points";
                    index = doc.Layers.Add(child);

                    foreach (Point3d point in points)
                        doc.Objects.FindId(doc.Objects.AddPoint(point)).Attributes.LayerIndex = index;
                }
                if (grid.Count != 0)
                {
                    child.Name = "Grid";
                    index = doc.Layers.Add(child);

                    foreach (List<Point3d> list in grid)
                    {
                        List<Guid> guids = new List<Guid>();

                        foreach (Point3d point in list)
                        {
                            guids.Add(doc.Objects.AddPoint(point));
                            doc.Objects.FindId(guids.Last()).Attributes.LayerIndex = index;
                        }

                        doc.Groups.Add(guids);
                    }

                }
                if (polylines.Count != 0)
                {
                    child.Name = "Polylines";
                    index = doc.Layers.Add(child);

                    foreach (Polyline polyline in polylines)
                        doc.Objects.FindId(doc.Objects.AddPolyline(polyline)).Attributes.LayerIndex = index;
                }
                if (curves.Count != 0)
                {
                    child.Name = "Curves";
                    index = doc.Layers.Add(child);

                    foreach (Curve curve in curves)
                        doc.Objects.FindId(doc.Objects.AddCurve(curve)).Attributes.LayerIndex = index;
                }
                if (lineframe.Count != 0)
                {
                    child.Name = "Lineframe";
                    index = doc.Layers.Add(child);

                    foreach (List<Polyline> list in lineframe)
                    {
                        List<Guid> guids = new List<Guid>();
                        
                        foreach (Polyline polyline in list)
                        {
                            guids.Add(doc.Objects.AddPolyline(polyline));
                            doc.Objects.FindId(guids.Last()).Attributes.LayerIndex = index;
                        }

                        doc.Groups.Add(guids);
                    }
                }
                if (wireframe.Count != 0)
                {
                    child.Name = "Wireframe";
                    index = doc.Layers.Add(child);

                    foreach (List<Curve> list in wireframe)
                    {
                        List<Guid> guids = new List<Guid>();

                        foreach (Curve curve in list)
                        {
                            guids.Add(doc.Objects.AddCurve(curve));
                            doc.Objects.FindId(guids.Last()).Attributes.LayerIndex = index;
                        }

                        doc.Groups.Add(guids);
                    }
                }
                if (triangles.Count != 0)
                {
                    child.Name = "Triangles";
                    index = doc.Layers.Add(child);

                    foreach (Brep brep in triangles)
                        doc.Objects.FindId(doc.Objects.AddBrep(brep)).Attributes.LayerIndex = index;
                }
                if (quads.Count != 0)
                {
                    child.Name = "Quads";
                    index = doc.Layers.Add(child);

                    foreach (Brep brep in quads)
                        doc.Objects.FindId(doc.Objects.AddBrep(brep)).Attributes.LayerIndex = index;
                }
                if (surfaces.Count != 0)
                {
                    child.Name = "Surfaces";
                    index = doc.Layers.Add(child);

                    foreach (Surface surface in surfaces)
                        doc.Objects.FindId(doc.Objects.AddSurface(surface)).Attributes.LayerIndex = index;
                }


            }

            /// <summary>
            /// Converts a Plot Point to a Rhino Point.
            /// </summary>
            /// <param name="point"></param>
            /// <returns></returns>
            public static Point3d PointToRhino(Objects.Point point)
            {
                return new Point3d(point.X, point.Y, point.Y);
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
                    for (int j = 0; j < grid[i].Count; j++)
                        g[i][j] = PointToRhino(grid[i][j]);

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
            /// <returns></returns>
            public static Curve CurveToRhino(Objects.Polyline polyline)
            {
                return PolylineToRhino(polyline).ToNurbsCurve();
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
            { "X" , new Bounds(Double.MaxValue, Double.MaxValue) },
            { "Y" , new Bounds(Double.MaxValue, Double.MaxValue) },
            { "Z" , new Bounds(Double.MaxValue, Double.MaxValue) }
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
        protected void CreateRhinoObjects(Objects.Wireframe wireframe)
        {
            var o = new RhinoObjects();

            if (dimension == 2)
            {
                // Point list creation
                o.points = new List<Point3d>();

                foreach (Objects.Polyline polyline in wireframe.uCurves)
                    foreach (Objects.Point point in polyline.Verticies)
                        o.points.Add(RhinoObjects.PointToRhino(point));

                // Polyline list creation
                o.polylines = new List<Polyline>();

                foreach (Objects.Polyline polyline in wireframe.uCurves)
                    o.polylines.Add(RhinoObjects.PolylineToRhino(polyline));

                // Curve list creation
                o.curves = new List<Curve>();

                foreach (Objects.Polyline polyline in wireframe.uCurves)
                    o.curves.Add(RhinoObjects.CurveToRhino(polyline));
            }
            else if (dimension == 3)
            {
                // Grid creation
                o.grid = new List<List<Point3d>>();

                foreach (Objects.Polyline polyline in wireframe.uCurves)
                {
                    List<Point3d> points = new List<Point3d>();

                    foreach (Objects.Point point in polyline.Verticies)
                        points.Add(RhinoObjects.PointToRhino(point));

                    o.grid.Add(points);
                }

                // Lineframe creation
                o.lineframe = RhinoObjects.LineframeToRhino(wireframe);

                // Wireframe creation
                o.wireframe = RhinoObjects.WireframeToRhino(wireframe);

                // Triangle list creation
                o.triangles = RhinoObjects.TriangleMeshToRhino(Objects.TriangleMesh.MakeFromWireframe(wireframe));

                // Quad list creation
                o.quads = RhinoObjects.QuadMeshToRhino(Objects.QuadMesh.MakeFromWireframe(wireframe));

                // Surface creation
                o.surfaces = new List<Surface>
                {
                    Create.SurfaceFromPoints(o.grid)
                };
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
                    d = System.Convert.ToDouble(args.Parameters[0].Evaluate());
                if (args.Parameters.Length > 1)
                    e = System.Convert.ToDouble(args.Parameters[1].Evaluate());
                if (args.Parameters.Length > 2)
                    f = System.Convert.ToDouble(args.Parameters[2].Evaluate());

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
                        args.Result = Math.Round(d, System.Convert.ToInt32(e));
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
        public StandardEquation(string expression, List<Bounds> bounds, int pointsPerCurve = 100, int curvesPerSurface = 100)
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
            if (pointsPerCurve > 1 && pointsPerCurve < 999)
            {
                RhinoApp.WriteLine("Points per curve must be bigger than 1 and less than 999.");
                return false;
            }
            if (curvesPerSurface > 1 && curvesPerSurface < 999)
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
        /// Checks to see if the expression is valid.
        /// </summary>
        /// <param name="vars"></param>
        /// <returns></returns>
        private bool ValidVariables(List<string> vars)
        {
            return ContainsRightVariables(vars) || !ContainsWrongVariables(vars);
        }

        /// <summary>
        /// Looks to see if valid vairables exist in expression.
        /// </summary>
        /// <param name="vars"></param>
        /// <returns></returns>
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
        /// <param name="vars"></param>
        /// <returns></returns>
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
        public override void Generate()
        {
            if (success)
            {
                var eq = new Expression(expression);
                SetParameters(ref eq);

                double pointIteration = (bounds[0].max - bounds[0].min) / pointsPerCurve;
                double curveIteration = 1;
                double result = 0;
                
                if (dimension == 2)
                    bounds.Add(equationType == Type.SPHERICAL ? new Bounds(Math.PI/2,2) : new Bounds(0,0));
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

                        if (Double.IsPositiveInfinity(result))
                            result = Double.MaxValue;
                        else if (Double.IsNegativeInfinity(result))
                            result = Double.MinValue;
                        else if (Double.IsNaN(result))
                            result = 0;

                        Objects.Point functionResult = ExpressionResult(eq, varOne, (dimension == 3) ? varTwo : result, (dimension == 3) ? result : varTwo);

                        // replace these lines
                        functionResult.X = Math.Min(functionResult.X, maxValues["X"].max);
                        functionResult.X = Math.Max(functionResult.X, maxValues["X"].min);
                        functionResult.Y = Math.Min(functionResult.Y, maxValues["Y"].max);
                        functionResult.Y = Math.Max(functionResult.Y, maxValues["Y"].min);
                        functionResult.Z = Math.Min(functionResult.Z, maxValues["Z"].max);
                        functionResult.Z = Math.Max(functionResult.Z, maxValues["Z"].min);

                        curve.Add(functionResult);
                    }

                    if (wrapPoints)
                        curve.Add(curve[0]);

                    wireframe.uCurves.Add(curve);
                }

                if (wrapCurves && dimension == 3)
                    wireframe.uCurves.Add(wireframe.uCurves[0]);

                CreateRhinoObjects(wireframe);
            }
            else
                RhinoApp.WriteLine("Unable to generate equation.");
        }
    }
}
