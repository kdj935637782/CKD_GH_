using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;
using NAudio.Wave;

// In order to load the result of this wizard, you will also need to
// add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.
// You can use the _GrasshopperDeveloperSettings Rhino command for that.

namespace CKD.Baidu_ai
{
    public class VoiceRecordComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public VoiceRecordComponent()
          : base("VoiceRecord", "R",
              "Description",
              "CKD", "ForFun")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("start","s","start record",GH_ParamAccess.item,false);
            pManager.AddTextParameter("path","p","filePath",GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
        }

        MyRecorder myRecorder;

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool start = false;
            string filePath=string.Empty;
            if (!DA.GetData(0, ref start)) { return; }
            if (!DA.GetData(1, ref filePath)) { return; }
            if (start)
            {
                myRecorder = new MyRecorder();
                myRecorder.SetFileName(@filePath);
                myRecorder.StartRec();
            }
            if(!start&&myRecorder!=null)
            {
                myRecorder.StopRec();
            }
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("cd4d9d9b-f50e-4cd1-b13c-302f087f5ae4"); }
        }
    }
}
