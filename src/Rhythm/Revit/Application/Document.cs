﻿using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Geometry;
using Autodesk.Revit.DB;
using Dynamo.Graph.Nodes;
using Revit.Elements;
using Revit.GeometryConversion;
using RevitServices.Persistence;
using RevitServices.Transactions;
using Rhythm.Utilities;

namespace Rhythm.Revit.Application
{

    /// <summary>
    /// Wrapper class for document nodes.
    /// </summary>
    public class Documents
    {
        private Documents()
        { }

        /// <summary>
        /// This node will copy the given elements from the given linked document into the active document.
        /// </summary>
        /// <param name="sourceDocument">The background opened document object, (preferably this is the title as obtained with Applications.OpenDocumentFile from Rhythm).</param>
        /// <param name="sourceInstance">The instance of the link to copy from.</param>
        /// <param name="elements">The elements to copy.</param>
        /// <returns name="newElements">The copied elements.</returns>
        /// <search>
        /// copy
        /// </search>
        [NodeCategory("Actions")]
        public static List<global::Revit.Elements.Element> CopyElementsFromLinkedDocument(Autodesk.Revit.DB.Document sourceDocument,
            global::Revit.Elements.Element sourceInstance, List<global::Revit.Elements.Element> elements)
        {
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            //converts elements to ids in a collection
            ICollection<ElementId> idCollection = new List<ElementId>();

            foreach (global::Revit.Elements.Element element in elements)
            {
                idCollection.Add(element.InternalElement.Id);
            }
            Autodesk.Revit.DB.RevitLinkInstance internalInstance = (Autodesk.Revit.DB.RevitLinkInstance)sourceInstance.InternalElement;

            //creates blank copy paste options
            CopyPasteOptions copyOpts = new CopyPasteOptions();
            //creates blank transform.
            Transform transform = Transform.CreateTranslation(Vector.XAxis().ToRevitType());
            //commits the transaction
            TransactionManager.Instance.EnsureInTransaction(doc);
            ICollection<ElementId> newElementIds = ElementTransformUtils.CopyElements(sourceDocument, idCollection, doc, internalInstance.GetTransform(), copyOpts);
            TransactionManager.Instance.TransactionTaskDone();
            //create a new list for the new elements
            List<global::Revit.Elements.Element> newElements = new List<global::Revit.Elements.Element>();
            //gets the new elements
            foreach (ElementId id in newElementIds)
            {
                newElements.Add(doc.GetElement(id).ToDSType(true));
            }

            return newElements;
        }

        /// <summary>
        /// This node will copy the given elements from the given document into the active document.
        /// </summary>
        /// <param name="sourceDocument">The background opened document object, (preferably this is the title as obtained with Applications.OpenDocumentFile from Rhythm).</param>
        /// <param name="elements">The elements to copy.</param>
        /// <returns name="newElements">The copied elements.</returns>
        /// <search>
        /// copy
        /// </search>
        [NodeCategory("Actions")]
        public static List<global::Revit.Elements.Element> CopyElementsFromDocument(Autodesk.Revit.DB.Document sourceDocument, List<global::Revit.Elements.Element> elements)
        {
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            //converts elements to ids in a collection
            ICollection<ElementId> idCollection = new List<ElementId>();
            foreach (global::Revit.Elements.Element element in elements)
            {
                idCollection.Add(element.InternalElement.Id);
            }
            //creates blank copy paste options
            CopyPasteOptions copyOpts = new CopyPasteOptions();
            //creates blank transform.
            Transform transform = Transform.CreateTranslation(Vector.XAxis().ToRevitType());
            //commits the transaction
            TransactionManager.Instance.EnsureInTransaction(doc);
            ICollection<ElementId> newElementIds = ElementTransformUtils.CopyElements(sourceDocument, idCollection, doc, transform, copyOpts);
            TransactionManager.Instance.TransactionTaskDone();
            //create a new list for the new elements
            List<global::Revit.Elements.Element> newElements = new List<global::Revit.Elements.Element>();
            //gets the new elements
            foreach (ElementId id in newElementIds)
            {
                newElements.Add(doc.GetElement(id).ToDSType(true));
            }
            
            return newElements;
        }
        /// <summary>
        /// This node will set the starting view of the document, given the view element.
        /// </summary>
        /// <param name="sourceDocument">The background opened document object, (preferably this is the title as obtained with Applications.OpenDocumentFile from Rhythm).</param>
        /// <param name="view">The view to set.</param>
        /// <returns name="result">The result.</returns>
        /// <search>
        /// startingView
        /// </search>
        [NodeCategory("Actions")]
        public static string SetStartingView(Autodesk.Revit.DB.Document sourceDocument, global::Revit.Elements.Element view)
        {
            try
            {
                TransactionManager.Instance.EnsureInTransaction(sourceDocument);
                StartingViewSettings.GetStartingViewSettings(sourceDocument).ViewId = view.InternalElement.Id;
                TransactionManager.Instance.TransactionTaskDone();
                return "Success";
            }
            catch
            {
                return "Failure";
            }
        }
        /// <summary>
        /// This node will copy the given drafting views and their contents from the given document into the active document.
        /// </summary>
        /// <param name="sourceDocument">The background opened document object, (preferably this is the title as obtained with Applications.OpenDocumentFile from Rhythm).</param>
        /// <param name="draftingViews">The drafting views to copy.</param>
        /// <returns name="newDraftingViews">The copied drafting views with their elements.</returns>
        /// <search>
        /// copy
        /// </search>
        [NodeCategory("Actions")]
        public static List<global::Revit.Elements.Element> CopyDraftingViewsFromDocument(Autodesk.Revit.DB.Document sourceDocument, List<global::Revit.Elements.Element> draftingViews)
        {
            Autodesk.Revit.DB.Document doc = DocumentManager.Instance.CurrentDBDocument;
            
            //converts elements to ids in a collection
            ICollection<ElementId> idCollection = new List<ElementId>();

            foreach (global::Revit.Elements.Element view in draftingViews)
            {
                idCollection.Add(view.InternalElement.Id);
            }

            TransactionManager.Instance.ForceCloseTransaction();
            List<View> newDraftingViews =
                ViewUtils.DuplicateDraftingViews(sourceDocument, idCollection, doc);
            List<global::Revit.Elements.Element> vList = new List<global::Revit.Elements.Element>(newDraftingViews.Select(v => v.ToDSType(true)));

            return vList;
        }
        /// <summary>
        /// This node will save the Revit document to another path.
        /// </summary>
        /// <param name="document">A valid Revit Document.</param>
        /// <param name="filePath">The file path to save the document.</param>
        /// <returns name="result">A string message whether the save as was successful or a failure.</returns>
        [NodeCategory("Action")]
        public static string SaveAs(Autodesk.Revit.DB.Document document, string filePath)
        {
            try
            {
                document.SaveAs(filePath);
                return "Successful Save";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}
