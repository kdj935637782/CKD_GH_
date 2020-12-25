using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Threading;

using AutoCAD;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using Rhino;
using System.Windows.Forms;

namespace CKD.CAD
{
    public class CadCurveWriter : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the PassToCad class.
        /// </summary>
        public CadCurveWriter()
          : base("CadLineWriter", "CLW",
              "pass the line to the cad",
              "CKD", "CAD")
        {
            buttonPressed=false;
            this.Message = "Click the icon\nto start write";
        }

        AcadApplication acad;
        public bool buttonPressed;

        public override void CreateAttributes()
        {
            this.Attributes = new CadCurveWriterrAttrubutes(this);
        }
        

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Curve","C","the curve pass to cad",GH_ParamAccess.list);
            pManager.AddTextParameter("Layer","L","the name layer",GH_ParamAccess.item,string.Empty);
            pManager.AddNumberParameter("Accuracy", "A", "accuracy of curve transmission",GH_ParamAccess.item,0.1);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
        }

        double factor;
        int fail;
        volatile int n = 0;

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Curve> geometryss = new List<Curve>();
            if (!DA.GetDataList<Curve>(0, geometryss)) { return; }
            if(geometryss.Count>=5000)
            {
                n = geometryss.Count / 500;
            }
            else
            {
                n = 10;
            }

            Task.Run(() =>
            {
                if (!buttonPressed) { return; }
                buttonPressed = false;
                fail = 0;
                GetActiveCad();

                DA.GetData(2, ref factor);

                string layerName = string.Empty;
                DA.GetData(1, ref layerName);
                if (layerName != string.Empty)
                {
                    SetLayer(layerName);
                }

                List<Curve> geometrys = new List<Curve>();
                if (!DA.GetDataList<Curve>(0, geometrys)) { return; }

                if (acad != null)
                {
                    foreach (Curve geometry in geometrys)
                    {
                        if (geometry != null)
                        {
                            this.StartWrite(geometry);
                        }
                    }
                }
            });
        }

        private void StartWrite(Curve geometry)
        {
            if (geometry.GetType() == typeof(LineCurve)) { TransitLine(geometry); }
            else if (geometry.GetType() == typeof(PolylineCurve)) { TransitPolyline(geometry); }
            else if (geometry.GetType() != typeof(LineCurve)) { TransitCurve(geometry); }
        }

        private void TransitPolyline(Curve curve)
        {
            Task.Run(() =>
            {
                try
                {
                    PolylineCurve polylineCurve = (PolylineCurve)curve;
                    double[] controlsXYZ = new double[polylineCurve.PointCount*3];
                    for(int i=0;i< polylineCurve.PointCount; i++)
                    {
                        controlsXYZ[3 * i] = polylineCurve.Point(i).X;
                        controlsXYZ[3 * i+1] = polylineCurve.Point(i).Y;
                        controlsXYZ[3 * i+2] = polylineCurve.Point(i).Z;
                    }
                    Add3dPolyline(controlsXYZ, 0);
                    UpdateApplication(0);
                }
                catch
                {
                    this.TransitCurve(curve);
                }
            });
        }

        private void TransitCurve(Rhino.Geometry.Curve curve)
        {
            Task.Run(() =>
            {
                Vector3d vecStart = curve.TangentAtStart;
                double[] vecStartD = { vecStart.X,vecStart.Y,vecStart.Z};
                Vector3d vecEnd = curve.TangentAtEnd;
                double[] vecEndD = { vecEnd.X, vecEnd.Y, vecEnd.Z };

                List<double> controlPoints = new List<double>();
                for(double i=0;i<=1;i+=factor)
                {
                    Rhino.Geometry.Point3d point= curve.PointAtNormalizedLength(i);
                    controlPoints.Add(point.X);
                    controlPoints.Add(point.Y);
                    controlPoints.Add(point.Z);
                }

                AddCurve(controlPoints.ToArray(),vecStartD,vecEndD,0);
                UpdateApplication(0);
            });
        }

        private void TransitLine(Curve line)
        {
            Task.Run(() =>
            {
                Rhino.Geometry.Point3d startPoint = line.PointAtStart;
                Rhino.Geometry.Point3d endPoint = line.PointAtEnd;
                double[] startD = { startPoint.X, startPoint.Y, startPoint.Z };
                double[] endD = { endPoint.X, endPoint.Y, endPoint.Z };
                AddLine(startD, endD,0);
                UpdateApplication(0);
            });
        }

        private void GetActiveCad()
        {
            try
            {
                acad = (AcadApplication)Marshal.GetActiveObject("AutoCAD.Application");
            }
            catch
            {
                acad = new AcadApplicationClass();
                acad.Visible = true;
            }
        }

        private void Add3dPolyline( double[] controlsXYZ,int i)
        {
            if (i < n)
            {
                try
                {
                    acad.ActiveDocument.ModelSpace.Add3DPoly(controlsXYZ);
                }
                catch
                {
                    Add3dPolyline(controlsXYZ, i + 1);
                }
            }
            else
            {
                fail += 1;
                this.ClearRuntimeMessages();
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "faild to pass" + fail.ToString() + "objects");
            }
        }

        private void AddCurve(double[] controls,double[] startVec,double[] endVec,int i)
        {
            if (i < n)
            {
                try
                {
                    acad.ActiveDocument.ModelSpace.AddSpline(controls, startVec, endVec);
                }
                catch
                {
                    AddCurve(controls, startVec, endVec, i + 1);
                }
            }
            else
            {
                fail += 1;
                this.ClearRuntimeMessages();
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "faild to pass" + fail.ToString() + "objects");
            }
        }

        private void AddLine(double[] a, double[] b,int i)
        {
            if (i < n)
            {
                try
                {
                    acad.ActiveDocument.ModelSpace.AddLine(a, b);
                }
                catch
                {

                    AddLine(a, b, i + 1);
                }
            }
            else
            {
                fail += 1;
                this.ClearRuntimeMessages();
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "faild to pass" + fail.ToString() + "objects");
            }
        }

        private void UpdateApplication(int i)
        {
            if (i <n)
            {
                try
                {
                    acad.Update();
                }
                catch
                {
                    UpdateApplication(i+1);
                }
            }
        }

        private void SetLayer(string layerName)
        {
            ///图层是否存在
            bool flag = false;
            ///设置激活图层
            foreach(AcadLayer acadLayer in acad.ActiveDocument.Layers)
            {
                if(acadLayer.Name==layerName)
                {
                    acad.ActiveDocument.ActiveLayer = acadLayer;
                    flag = true;
                }
            }
            if (flag) { return; }
            acad.ActiveDocument.Layers.Add(layerName);
            SetLayer(layerName);
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
                return Properties.Resources.CadLineWriter;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("97d1e767-85b2-4a3e-923c-bc80c3ad2c9c"); }
        }
    }
}