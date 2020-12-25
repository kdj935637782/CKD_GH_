using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO.MemoryMappedFiles;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.IO;
using System.Text;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Grasshopper;

using Autodesk.AutoCAD.Geometry;
using AutoCAD;

using Rhino.Geometry;
using Microsoft.CSharp;

namespace CKD.CAD
{
    public class CadCurveTextReader : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the CadLineReader class.
        /// </summary>
        public CadCurveTextReader()
          : base("CadTextReader", "CTR",
              "Description",
              "CKD", "CAD")
        {
        }

        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("heigh", "h", "", GH_ParamAccess.list);
            pManager.AddPointParameter("loaction", "l", "", GH_ParamAccess.list);
        }

        /// <summary>
        /// 字段
        /// </summary>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            AcadApplication acad;
            try
            {
                acad = (AutoCAD.AcadApplication)Marshal.GetActiveObject("AutoCAD.Application");
            }
            catch
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "No cad program is running");
                return;
            }
            if (acad == null)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "No cad program is running");
                return;
            }
            var texts = new List<string>();
            var points = new List<Rhino.Geometry.Point3d>();
            for(int i=0 ; i<acad.ActiveDocument.ModelSpace.Count;i++)
            {
                try
                {
                    AcadEntity entity = acad.ActiveDocument.ModelSpace.Item(i);
                    AcadBlockReference blockReference = (AcadBlockReference)entity;
                    dynamic att = blockReference.GetAttributes();
                    AcadAttributeReference attributeReference = (AcadAttributeReference)((object[])att)[0];
                    double[] point = (double[])attributeReference.TextAlignmentPoint;
                    string value = attributeReference.TextString;
                    points.Add(new Rhino.Geometry.Point3d(point[0], point[1], point[2]));
                    texts.Add(value);
                }
                catch(Exception e)
                {
                    RuntimeMessages(GH_RuntimeMessageLevel.Warning).Add(e.Message);
                }
            }
            DA.SetDataList(0, texts);
            DA.SetDataList(1, points);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("933d8e3b-7cfa-42af-855a-8fae2cc98bc5"); }
        }
    }
}