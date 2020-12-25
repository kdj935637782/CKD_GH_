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
using Rhino.FileIO;

namespace CKD.CAD
{
    public class CadCurveReader : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the CadLineReader class.
        /// </summary>
        public CadCurveReader()
          : base("CadReader", "CR",
              "Description",
              "CKD", "CAD")
        {
            fileName =fileialog+ DateTime.Now.ToFileTime().ToString();
        }

        public override void CreateAttributes()
        {
            this.Attributes =new CadCurveReaderAttrubutes(this);
        }


        /// <summary>
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
             pManager.AddGenericParameter("", "", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// 字段
        /// </summary>
        #region
        string fileName = string.Empty;
        const string fileialog= "D:\\";
        AcadApplication acad;
        #endregion
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if (((CadCurveReaderAttrubutes)this.Attributes).start != 0)
            {
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

                List<IGH_Goo> list = new List<IGH_Goo>();
                this.SendCommand_CAD();
                //this.GhListToGET(list);

                Rhino.RhinoApp.RunScript(("_-Import " + fileName + ".dxf  _Enter _Enter _Enter"), true);

                File.Delete(fileName+".dxf");


                foreach (Rhino.DocObjects.RhinoObject rhinoObject in Rhino.RhinoDoc.ActiveDoc.Objects.GetSelectedObjects(true, true))
                {
                    IGH_Goo igh_Goo = GH_Convert.ToGoo(rhinoObject.Geometry);
                    list.Add(igh_Goo);
                }

                Rhino.RhinoApp.RunScript("_-Delete  _Enter", true);
                DA.SetDataList(0, list);

            }
        }

        //private void GhListToGET(List<IGH_Goo> list)
        //{
        //    try
        //    {
        //        acad.ActiveDocument.ActiveSelectionSet.Clear();
        //        acad.ActiveDocument.SendCommand("SELECT\n");
        //        acad.Update();
        //        int count = acad.ActiveDocument.ActiveSelectionSet.Count;
        //        for(int i=0;i<count;i++)
        //        {
        //            IGH_Goo igh_Goo = GH_Convert.ToGoo(acad.ActiveDocument.ActiveSelectionSet.Item(i));
        //            list.Add(igh_Goo);
        //        }



        //        string blockName = Clipboard.GetText();


        //        //acad.WindowTop = 0;

        //    }
        //    catch
        //    {
        //        this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Cad is using!");
        //        return;
        //    }
        //}

        /// <summary>
        /// 在cad中导出文件
        /// </summary>
        private void SendCommand_CAD()
        {
            try
            {
                //acad.ActiveDocument.ActiveSelectionSet.Clear();
                //acad.Update();
                //acad.WindowTop = 1;
                //acad.ActiveDocument.SendCommand("PassObjectToGh_CalledByGh\n");
                //acad.Update();
                acad.ActiveDocument.Export(fileName, "dxf", acad.ActiveDocument.ActiveSelectionSet);



                //string blockName = Clipboard.GetText();


                //acad.WindowTop = 0;

            }
            catch
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Cad is using!");
                return;
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
                return CKD.Properties.Resources.cadreader;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("933d8e3b-7cfa-42af-855a-8fae2cc84bc5"); }
        }
    }
}