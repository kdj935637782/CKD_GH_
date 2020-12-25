using System;
using System.Collections.Generic;
using System.Drawing;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace CKD
{
    public class CadLayerComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the CadLayerComponent class.
        /// </summary>
        public CadLayerComponent()
          : base("CadLayerComponent", "Nickname",
              "Description",
              "CKD", "CAD")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("LayerName", "L", "layer name", GH_ParamAccess.item);
            pManager.AddColourParameter("Colour", "C", "layer colour", GH_ParamAccess.item,Color.Red);
        }

     

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Layer", "L", "Layer", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
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
            get { return new Guid("2072312c-995b-4f9b-99f8-1abab2630b2b"); }
        }
    }
}