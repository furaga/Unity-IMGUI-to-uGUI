using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace BBoxAnnotationTool
{
    public class UndoRedoManager
    {
        private static UndoRedoManager instance = new UndoRedoManager();

        public static UndoRedoManager Instance
        {
            get
            {
                return instance;
            }
        }

        private UndoRedoManager()
        {
        }

        public class Operation
        {
            public object ModifiedObject;
            public Action Redo;
            public Action Undo;
            public Operation(object modifiedObject, Action redo, Action undo)
            {
                ModifiedObject = modifiedObject;
                Redo = redo;
                Undo = undo;
            }
        }

        List<Operation> history_ = new List<Operation>();
        int counter_ = -1;

        public void Reset()
        {
            history_.Clear();
            counter_ = -1;
        }

        public void Do(object modifiedObject, Action redo, Action undo)
        {
            redo();

            // Memo operation
            history_ = history_.Take(counter_ + 1).ToList();
            history_.Add(new Operation(modifiedObject, redo, undo));
            counter_ = history_.Count - 1;
        }

        public object Undo()
        {
            if (counter_ <= -1)
            {
                return null;
            }
            var modObj = history_[counter_].ModifiedObject;
            history_[counter_].Undo();
            counter_--;
            return modObj;
        }

        public object Redo()
        {
            if (history_.Count <= counter_ + 1)
            {
                return null;
            }
            counter_++;
            var modObj = history_[counter_].ModifiedObject;
            history_[counter_].Redo();
            return modObj;
        }
        
        public void AddBBox(BBoxAnnotation annot, BoundingBox bbox)
        {

            Do(bbox,
               () => { annot.BBoxs.Add(bbox); },
               () => { annot.BBoxs.Remove(bbox); });
        }

        public void RemoveBBox(BBoxAnnotation annot, BoundingBox bbox)
        {
            Do(annot,
               () => { annot.BBoxs.Remove(bbox); },
               () => { annot.BBoxs.Add(bbox); });
        }

        public void ChangeBBoxRect(BoundingBox bbox, Rect prevRect, Rect newRect)
        {
            Do(bbox,
               () => { bbox.Rect = newRect; },
               () => { bbox.Rect = prevRect; });
        }
    }
}