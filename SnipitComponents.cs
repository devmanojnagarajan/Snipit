using System;
using Grasshopper.Kernel;

namespace Snipit
{
    public class SnipitTest : GH_Component
    {
        public SnipitTest()
          : base("Snipit Test", "STest",
                 "A test component to verify Snipit loads.",
                 "Snipit", "Dev")
        { }

        public override Guid ComponentGuid =>
            new Guid("69D71C49-C79B-42E5-B03F-069FF4504544");

        protected override void RegisterInputParams(GH_InputParamManager p)
        {
            p.AddTextParameter("Input", "I", "Test input", GH_ParamAccess.item, "Hello");
        }

        protected override void RegisterOutputParams(GH_OutputParamManager p)
        {
            p.AddTextParameter("Output", "O", "Test output", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess da)
        {
            string input = "";
            da.GetData(0, ref input);
            da.SetData(0, "Snipit says: " + input);
        }
    }
}