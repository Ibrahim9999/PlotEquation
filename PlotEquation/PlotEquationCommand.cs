using System;
using System.Collections.Generic;
using Rhino;
using Rhino.Commands;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;

namespace PlotEquation
{
    public class EquationCommand : Command
    {
        public EquationCommand()
        {
            // Rhino only creates one instance of each command class defined in a
            // plug-in, so it is safe to store a refence in a static property.
            Instance = this;
        }

        ///<summary>The only instance of this command.</summary>
        public static EquationCommand Instance
        {
            get; private set;
        }

        ///<returns>The command name as it appears on the Rhino command line.</returns>
        public override string EnglishName
        {
            get { return "Equation"; }
        }

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            // TODO: start here modifying the behaviour of your command.
            // ---
            RhinoApp.WriteLine("{0} will plot a graph.\n", EnglishName);

            //new Eto.Forms.Application().Run(new MyForm());

            int dimension = 2;
            Result rc = RhinoGet.GetInteger("Dimension", true, ref dimension);

            string expression = "z=sin(x)+sin(y)";
            rc = RhinoGet.GetString("Equation", true, ref expression);

            StandardEquation eq;

            if (dimension == 2)
                eq = new StandardEquation(expression, new List<Bounds> { new Bounds(-10, 10) });
            else
                eq = new StandardEquation(expression, new List<Bounds> { new Bounds(-10, 10), new Bounds(-10, 10) }, 50, 50);

            eq.Generate(doc);
            eq.GetRhinoObjects().AddAll(doc, expression);

            // ---

            return Result.Success;
        }
    }

    public class ParametricEquationCommand : Command
    {
        public ParametricEquationCommand()
        {
            // Rhino only creates one instance of each command class defined in a
            // plug-in, so it is safe to store a refence in a static property.
            Instance = this;
        }

        ///<summary>The only instance of this command.</summary>
        public static ParametricEquationCommand Instance
        {
            get; private set;
        }

        ///<returns>The command name as it appears on the Rhino command line.</returns>
        public override string EnglishName
        {
            get { return "ParametricEquation"; }
        }

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            // TODO: start here modifying the behaviour of your command.
            // ---
            RhinoApp.WriteLine("{0} will plot a graph.\n", EnglishName);

            //new Eto.Forms.Application().Run(new MyForm());

            int dimension = 2;
            Result rc = RhinoGet.GetInteger("Dimension", true, ref dimension);

            string x = "x=(2+sin(2*pi*v)*sin(2*pi*u))*sin(3*pi*v)";
            string y = "y=sin(2*pi*v)*cos(2*pi*u)+4*v-2";
            string z = "z=(2+sin(2*pi*v)*sin(2*pi*u))*cos(3*pi*v)";
            rc = RhinoGet.GetString("Equation", true, ref x);

            StandardEquation eq;

            if (dimension == 2)
                eq = new StandardEquation(x, new List<Bounds> { new Bounds(-10, 10) });
            else
                eq = new StandardEquation(x, new List<Bounds> { new Bounds(-10, 10), new Bounds(-10, 10) }, 50, 50);

            eq.Generate(doc);
            eq.GetRhinoObjects().AddAll(doc, x);

            // ---

            return Result.Success;
        }
    }
}
