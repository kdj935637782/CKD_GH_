﻿using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace CKD
{
    public class CadGeneralWriter : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the CadGeneralWriter class.
        /// </summary>
        public CadGeneralWriter()
          : base("CadGeneralWriter", "Nickname",
              "Description",
              "CKD", "CAD")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("object", "o", "object to cad", GH_ParamAccess.list);
            pManager.AddTextParameter("Layer", "L", "the name layer", GH_ParamAccess.item, string.Empty);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
        }

        private void SendCommand_Rhino()
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
            get { return new Guid("06d719c8-e2d1-4add-83d0-b8b0f43282ad"); }
        }
    }
}