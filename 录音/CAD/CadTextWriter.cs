using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AutoCAD;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Rhino.Runtime.InteropWrappers;

namespace CKD.CAD
{
    public class CadTextWriter : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the CadTextWriter class.
        /// </summary>
        public CadTextWriter()
          : base("CadTextWriter", "Nickname",
              "Description",
              "CKD", "CAD")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Text", "T", "Text", GH_ParamAccess.item);
            pManager.AddPointParameter("Location", "L", "Location of text", GH_ParamAccess.item,new Point3d(0,0,0));
            pManager.AddNumberParameter("Size", "S", "Size of text", GH_ParamAccess.item, 1);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
        }

        AcadApplication acad;

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string text = string.Empty;
            Point3d location = Point3d.Origin;
            double size = 0;
            if(!DA.GetData(0,ref text)) { return; }
            DA.GetData(1,ref location);
            DA.GetData(2, ref size);

            try
            {
                
                acad = (AutoCAD.AcadApplication)Marshal.GetActiveObject("AutoCAD.Application");
            }
            catch
            {
                acad = new AcadApplicationClass();
                acad.Visible = true;
            }
            try
            {
                double[] locationXYZ = { location.X, location.Y, location.Z };
                acad.ActiveDocument.ModelSpace.AddMText(locationXYZ,size,text);
                acad.Update();
            }
            catch(Exception ex)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, ex.Message);
            }

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
            get { return new Guid("a1127231-ff18-41ec-bee6-72bd9ad37dc1"); }
        }
    }
}