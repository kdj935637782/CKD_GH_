using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Grasshopper.Kernel;
using Rhino.Geometry;
using AutoCAD;
using System.Threading.Tasks;

namespace CKD
{
    public class CadSendCommand : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public CadSendCommand()
          : base("CadSendCommand", "CSC",
              "Send command to running cad",
              "CKD", "CAD")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Command", "C", "Command to send", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Send", "S", "Send", GH_ParamAccess.item, false);
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
            bool flag = false;
            DA.GetData(1, ref flag);
            string command = string.Empty;
            if(!DA.GetData(0,ref command)) { return; }

            if (flag)
            {
                Task.Run(() =>
                {
                    AcadApplication acad = null;
                    try
                    {
                        acad = (AutoCAD.AcadApplication)Marshal.GetActiveObject("AutoCAD.Application");
                    }
                    catch
                    {
                        try
                        {
                            acad = new AcadApplicationClass();
                            acad.Visible = true;
                        }
                        catch (Exception ex)
                        {
                            this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, ex.Message);
                        }

                    }
                    try
                    {
                        acad.ActiveDocument.SendCommand(command + "\n");
                    }

                    catch (Exception ex)
                    {
                        this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, ex.Message);
                    }
                });
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
            get { return new Guid("b2a29fbe-0185-4c2b-9351-17f67d9e4e4e"); }
        }
    }
}