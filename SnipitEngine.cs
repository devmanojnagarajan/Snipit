using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using GH_IO.Serialization;
using Grasshopper.Kernel;

namespace Snipit
{
    /// <summary>
    /// The heart of the plugin. A "snipit" is a serialized chunk of one or more
    /// Grasshopper document objects, with their internal wiring preserved.
    ///
    /// Capture  : selected objects -> GH_Archive -> bytes on disk (.snipit)
    /// Deploy   : bytes -> GH_Archive -> fresh objects with NEW GUIDs -> add to active doc
    /// </summary>
    public static class SnipitEngine
    {
        private const string ChunkKey = "Snipit";

        // ---- CAPTURE -------------------------------------------------------

        public static byte[] CaptureSelection(GH_Document doc, out int objectCount)
        {
            objectCount = 0;
            if (doc == null) return null;

            var selected = doc.SelectedObjects();
            if (selected == null || selected.Count == 0) return null;

            objectCount = selected.Count;

            var io = new GH_DocumentIO(doc);
            if (!io.Copy(GH_ClipboardType.Local))
                return null;

            var clipDoc = io.Document;
            if (clipDoc == null || clipDoc.ObjectCount == 0) return null;

            var archive = new GH_Archive();
            archive.AppendObject(clipDoc, ChunkKey);

            return archive.Serialize_Binary();
        }

        // ---- DEPLOY --------------------------------------------------------

        public static List<IGH_DocumentObject> Deploy(
            byte[] snipitBytes,
            GH_Document targetDoc,
            PointF dropPoint,
            out List<string> missingPlugins)
        {
            missingPlugins = new List<string>();
            var added = new List<IGH_DocumentObject>();
            if (snipitBytes == null || targetDoc == null) return added;

            var archive = new GH_Archive();
            if (!archive.Deserialize_Binary(snipitBytes)) return added;

            var sourceDoc = new GH_Document();
            if (!archive.ExtractObject(sourceDoc, ChunkKey)) return added;
            if (sourceDoc.ObjectCount == 0) return added;

            // CRITICAL #1: fresh IDs so repeated drops never collide.
            sourceDoc.MutateAllIds();

            // CRITICAL #2: offset everything so it lands under the cursor.
            var bounds = sourceDoc.BoundingBox(false);
            var offsetX = dropPoint.X - bounds.X;
            var offsetY = dropPoint.Y - bounds.Y;

            foreach (var obj in sourceDoc.Objects.ToList())
            {
                if (obj is IGH_Component comp && comp.Params == null)
                    missingPlugins.Add(obj.Name);

                if (obj.Attributes != null)
                {
                    obj.Attributes.Pivot = new PointF(
                        obj.Attributes.Pivot.X + offsetX,
                        obj.Attributes.Pivot.Y + offsetY);
                    obj.Attributes.ExpireLayout();
                }
                added.Add(obj);
            }

            foreach (var obj in added)
                targetDoc.AddObject(obj, false);

            targetDoc.NewSolution(false);
            return added;
        }
    }
}