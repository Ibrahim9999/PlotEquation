using System;
using System.Collections.Generic;
using Rhino;
using Rhino.Commands;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;

namespace PlotEquation
{
    public class PlotEquationCommand : Command
    {
        public PlotEquationCommand()
        {
            // Rhino only creates one instance of each command class defined in a
            // plug-in, so it is safe to store a refence in a static property.
            Instance = this;
        }

        ///<summary>The only instance of this command.</summary>
        public static PlotEquationCommand Instance
        {
            get; private set;
        }

        ///<returns>The command name as it appears on the Rhino command line.</returns>
        public override string EnglishName
        {
            get { return "EquationCurve"; }
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
                eq = new StandardEquation(expression, new List<Bounds> { new Bounds(-10, 10), new Bounds(-10, 10) }, 100, 100);

            eq.Generate(doc);
            eq.GetRhinoObjects().AddAll(doc, expression);

            // ---

            return Result.Success;
        }
    }
}
