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
            get { return "CartesianCurve"; }
        }

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            // TODO: start here modifying the behaviour of your command.
            // ---
            RhinoApp.WriteLine("{0} will plot a graph.", EnglishName);

            //new Eto.Forms.Application().Run(new MyForm());

            StandardEquation eq = new StandardEquation("y=sin(x)", new List<Bounds> { new Bounds(-10, 10),  });
            eq.Generate();
            eq.GetRhinoObjects().AddAll(doc, "Sine Wave");

            // ---

            return Result.Success;
        }
    }
}
