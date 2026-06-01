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
    ///
    /// Two things that cause round-trip bugs and that this class handles explicitly:
    ///   1. Re-instantiated objects MUST get fresh GUIDs, or dropping the same
    ///      snipit twice collides and Grasshopper silently misbehaves.
    ///   2. Objects must be re-positioned to the user's cursor, not dumped back
    ///      at their original captured coordinates.
    /// </summary>
    public static class SnipitEngine
    {
        // Archive chunk key used for both write and read. Keep these in sync.
        private const string ChunkKey = "Snipit";

        // ---- CAPTURE -------------------------------------------------------

        /// <summary>
        /// Serialize the currently selected objects in a document to a byte[].
        /// Returns null if nothing meaningful is selected.
        /// </summary>
        public static byte[] CaptureSelection(GH_Document doc, out int objectCount)
        {
            objectCount = 0;
            if (doc == null) return null;

            var selected = doc.SelectedObjects();
            if (selected == null || selected.Count == 0) return null;

            objectCount = selected.Count;

            // GH's own clipboard mechanism: copies current selection (and only
            // the wires connecting objects within that selection) into an
            // internal document we can then serialize.
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

        /// <summary>
        /// Read snipit bytes, create fresh objects with new GUIDs, offset them
        /// to <paramref name="dropPoint"/>, and add them to <paramref name="targetDoc"/>.
        /// Returns the list of newly added objects.
        /// </summary>
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

            // Read into a throwaway document we can inspect before merging.
            var sourceDoc = new GH_Document();
            if (!archive.ExtractObject(sourceDoc, ChunkKey)) return added;
            if (sourceDoc.ObjectCount == 0) return added;

            // CRITICAL #1: give every object a brand-new ID so repeated drops
            // never collide with each other or with existing canvas objects.
            sourceDoc.MutateAllIds();

            // CRITICAL #2: figure out the bounding box of the captured objects
            // and shift everything so the result lands under the cursor.
            var bounds = sourceDoc.BoundingBox(false);
            var offsetX = dropPoint.X - bounds.X;
            var offsetY = dropPoint.Y - bounds.Y;

            foreach (var obj in sourceDoc.Objects.ToList())
            {
                // If a component's defining library isn't installed on this
                // machine, GH deserializes it as an unknown placeholder. Report.
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

            // Merge into the live document as one undo step.
            var record = targetDoc.UndoUtil.CreateGenericObjectEvent("Drop Snipit", added);
            targetDoc.UndoServer.PushUndoRecord(record);

            foreach (var obj in added)
                targetDoc.AddObject(obj, false);

            targetDoc.NewSolution(false);
            return added;
        }
    }
}