using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace Snipit
{
    // =====================================================================
    //  SPIKE COMPONENTS
    //  These exist only to prove the engine + storage work end to end.
    //  Once the round-trip is solid, we replace these with a canvas widget
    //  and popup dialog.
    // =====================================================================

    /// <summary>Captures the current canvas selection into a named snipit file.</summary>
    public class SnipitCaptureComponent : GH_Component
    {
        public SnipitCaptureComponent()
          : base("Snipit Capture", "Capture",
                 "Save the currently selected components as a snipit.",
                 "Snipit", "Dev")
        { }

        public override Guid ComponentGuid =>
            new Guid("7E5E5ADA-D73E-422A-9776-7FD184C24018");

        protected override void RegisterInputParams(GH_InputParamManager p)
        {
            p.AddTextParameter("Tab", "T", "Tab name", GH_ParamAccess.item, "General");
            p.AddTextParameter("Name", "N", "Snipit name", GH_ParamAccess.item, "MySnipit");
            p.AddBooleanParameter("Run", "R", "Set true to capture", GH_ParamAccess.item, false);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager p)
        {
            p.AddTextParameter("Status", "S", "Result", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess da)
        {
            string tab = "General", name = "MySnipit"; bool run = false;
            da.GetData(0, ref tab); da.GetData(1, ref name); da.GetData(2, ref run);
            if (!run) { da.SetData(0, "Idle (set Run = true)"); return; }

            var doc = OnPingDocument();
            var bytes = SnipitEngine.CaptureSelection(doc, out int count);
            if (bytes == null)
            {
                da.SetData(0, "Nothing selected — select components on the canvas first.");
                return;
            }

            var store = new SnipitStore();
            store.Save(tab, name, bytes, thumbnail: null);
            da.SetData(0, $"Saved '{name}' to tab '{tab}' ({count} objects).");
        }
    }

    /// <summary>Drops a saved snipit onto the canvas near this component.</summary>
    public class SnipitDeployComponent : GH_Component
    {
        public SnipitDeployComponent()
          : base("Snipit Deploy", "Deploy",
                 "Drop a saved snipit onto the canvas.",
                 "Snipit", "Dev")
        { }

        public override Guid ComponentGuid =>
            new Guid("251AFD25-1833-4056-B191-7A774CD062A6");

        protected override void RegisterInputParams(GH_InputParamManager p)
        {
            p.AddTextParameter("Tab", "T", "Tab name", GH_ParamAccess.item, "General");
            p.AddTextParameter("Name", "N", "Snipit name", GH_ParamAccess.item, "MySnipit");
            p.AddBooleanParameter("Run", "R", "Set true to deploy", GH_ParamAccess.item, false);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager p)
        {
            p.AddTextParameter("Status", "S", "Result", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess da)
        {
            string tab = "General", name = "MySnipit"; bool run = false;
            da.GetData(0, ref tab); da.GetData(1, ref name); da.GetData(2, ref run);
            if (!run) { da.SetData(0, "Idle (set Run = true)"); return; }

            var doc = OnPingDocument();
            var store = new SnipitStore();
            var match = store.ListSnipits(tab).Find(s => s.Name == name);
            if (match == null) { da.SetData(0, $"Snipit '{name}' not found in '{tab}'."); return; }

            // Drop just to the right of this component.
            var pivot = Attributes.Pivot;
            var dropPoint = new PointF(pivot.X + 150, pivot.Y);

            var bytes = store.Load(match);
            var added = SnipitEngine.Deploy(bytes, doc, dropPoint, out var missing);

            var msg = $"Deployed {added.Count} objects.";
            if (missing.Count > 0)
                msg += $" Warning: missing plugins for: {string.Join(", ", missing)}.";
            da.SetData(0, msg);
        }
    }
}