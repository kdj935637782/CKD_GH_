using System;
using System.Collections.Generic;
using Grasshopper.GUI.Base;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Grasshopper.GUI.Canvas;
using Grasshopper.GUI;
using System.Drawing;
using Grasshopper;
using System.Windows.Forms;
// In order to load the result of this wizard, you will also need to
// add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.
// You can use the _GrasshopperDeveloperSettings Rhino command for that.

namespace CKD.Background
{
    
    public class ChangeBackComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public ChangeBackComponent()
          : base("更改背景样式", "Nickname",
              "ChangeColor",
              "CKD", "ForFun")
        {
            this.m_W = 0.5;
            this.paintMode = PaintMode.FULL;
            IsClose = false;
        }

        protected override void BeforeSolveInstance()
        {
            base.BeforeSolveInstance();
            foreach (IGH_DocumentObject igh_DocumentObject in Instances.ActiveCanvas.Document.Objects)
            {
                bool flag = igh_DocumentObject.ComponentGuid == this.ComponentGuid && igh_DocumentObject.InstanceGuid != base.InstanceGuid;
                if (flag)
                {
                    MessageBox.Show("Only one this component is allowed on the canvas. Removing the excessive component.");
                    base.OnPingDocument().RemoveObject(this, false);
                    break;
                }
            }
        }

        public override void RemovedFromDocument(GH_Document document)
        {
            IsClose = true;
            ChangeBackAttrubutes.m_Icon = Properties.Resources.close2;
            if (((ChangeBackAttrubutes)this.Attributes).m_overButton)
            {
                Instances.ActiveCanvas.CanvasPaintBackground -= PaintHandler;
            }
            else
            {
                base.RemovedFromDocument(document);
            }
        }

        public override bool AppendMenuItems(ToolStripDropDown menu)
        {
            ToolStripMenuItem toolStripMenuItem_up = GH_DocumentObject.Menu_AppendItem
                (menu, "Up", new EventHandler(this.Menu_UpClick));
            ToolStripMenuItem toolStripMenuItem_down = GH_DocumentObject.Menu_AppendItem
                (menu, "Down", new EventHandler(this.Menu_DownClick));
            ToolStripMenuItem toolStripMenuItem_left = GH_DocumentObject.Menu_AppendItem
                (menu, "Left", new EventHandler(this.Menu_LeftClick));
            ToolStripMenuItem toolStripMenuItem_right = GH_DocumentObject.Menu_AppendItem
                (menu, "Right", new EventHandler(this.Menu_RightClick));
            ToolStripMenuItem toolStripMenuItem_center = GH_DocumentObject.Menu_AppendItem
                (menu, "Center", new EventHandler(this.Menu_centerClick));
            ToolStripMenuItem toolStripMenuItem_full = GH_DocumentObject.Menu_AppendItem
                (menu, "Full", new EventHandler(this.Menu_fullClick));

            ToolStripTextBox toolStripTextBox = GH_DocumentObject.Menu_AppendTextItem(menu,
                this.m_W.ToString(),
                new GH_MenuTextBox.KeyDownEventHandler(this.TextBoxKeyDown),
                null,true, 200, true);
            switch(this.paintMode)
            {
                case (PaintMode.CENTER):
                    {
                        toolStripMenuItem_center.Checked = true;
                        break;
                    }
                case (PaintMode.DOWN):
                    {
                        toolStripMenuItem_down.Checked = true;
                        break;
                    }
                case (PaintMode.FULL):
                    {
                        toolStripMenuItem_full.Checked= true;
                        break;
                    }
                case (PaintMode.LEFT):
                    {
                        toolStripMenuItem_left.Checked = true;
                        break;
                    }
                case (PaintMode.RIGHT):
                    {
                        toolStripMenuItem_right.Checked = true;
                        break;
                    }
                case (PaintMode.UP):
                    {
                        toolStripMenuItem_up.Checked = true;
                        break;
                    }
            }
            return false;
        }


        private void Menu_UpClick(object sender, EventArgs e)
        {
            this.paintMode = PaintMode.UP;
            this.Click();
        }

        private void Menu_DownClick(object sender, EventArgs e)
        {
            this.paintMode = PaintMode.DOWN;
            this.Click();
        }

        private void Menu_RightClick(object sender,EventArgs e)
        {
            this.paintMode = PaintMode.RIGHT;
            this.Click();
        }

        private void Menu_LeftClick(object sender, EventArgs e)
        {
            this.paintMode = PaintMode.LEFT;
            this.Click();
        }

        private void Menu_centerClick(object sender, EventArgs e)
        {
            this.paintMode = PaintMode.CENTER;
            this.Click();
        }

        private void Menu_fullClick(object sender, EventArgs e)
        {
            this.paintMode = PaintMode.FULL;
            this.Click();
        }

        private void Click()
        {
            if(((ChangeBackAttrubutes)this.Attributes).m_overButton)
            {
                Instances.ActiveCanvas.CanvasPaintBackground -= PaintHandler;
            }
            if (!((ChangeBackAttrubutes)this.Attributes).m_overButton)
            {
                Instances.ActiveCanvas.CanvasPaintBackground += PaintHandler;
            }
            this.ExpireSolution(true);

        }

        private void TextBoxKeyDown(GH_MenuTextBox sender, KeyEventArgs e)
        {
            Keys keyCode = e.KeyCode;
            if(keyCode==Keys.Enter)
            {
                if (sender.Text == null) { return; }
                try
                {
                    double value = double.Parse(sender.Text);
                    if (value > 1) { value = 1; }
                    if (value < 0) { value = 0; }
                    m_W = value;
                    sender.CloseEntireMenuStructure();
                    e.Handled = true;
                    return;
                }
                catch { return; }
            }
            if (keyCode != Keys.Escape)
            {
                return;
            }
            sender.CloseEntireMenuStructure();
            e.Handled = true;
        }

        public override void CreateAttributes()
        {
            this.Attributes = new ChangeBackAttrubutes(this);
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
        }

        public override void ExpireSolution(bool recompute)
        {
            Instances.ActiveCanvas.Invoke(new MethodInvoker(()=>
            {
                if (((ChangeBackAttrubutes)this.Attributes).m_overButton)
                {
                    Instances.ActiveCanvas.CanvasPaintBackground += PaintHandler;
                }
                else
                {
                    Instances.ActiveCanvas.CanvasPaintBackground -= PaintHandler;
                }
                base.ExpireSolution(recompute);
            }));
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
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
                return Properties.Resources.close2;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("30cbe586-3c42-4b04-a6af-1ff19ff985d6"); }
        }
        
        public double m_W;

        private void PaintHandler(Grasshopper.GUI.Canvas.GH_Canvas sender)
        {
            if (IsClose) { return; }
            sender.Graphics.ResetTransform();

            double ancho = sender.Width * m_W;

            Pen pen = new Pen(Color.FromArgb(255, 65, 65, 65), 0);
            Rectangle rect;
            switch(this.paintMode)
            {
                case (PaintMode.CENTER):
                    {
                        double ancho2 = sender.Height * m_W;
                        rect = GH_Convert.ToRectangle(new Rectangle((int)(sender.Width*(1-m_W))/2, (int)(sender.Height * (1 - m_W)) / 2, (int)ancho, (int)ancho2));
                        break;
                    }
                case (PaintMode.DOWN):
                    {
                        ancho = sender.Height * m_W;
                        rect = GH_Convert.ToRectangle(new Rectangle(0, sender.Height-(int)ancho, sender.Width, (int)ancho));
                        break;
                    }
                case (PaintMode.LEFT):
                    {
                        rect = GH_Convert.ToRectangle(new Rectangle(0, 0, (int)ancho, (sender.Height)));
                        break;
                    }
                case (PaintMode.RIGHT):
                    {
                        rect = GH_Convert.ToRectangle(new Rectangle((int)(sender.Width*(1-m_W)), 0, (int)ancho, (sender.Height)));
                        break;
                    }
                case (PaintMode.UP):
                    {
                        ancho = sender.Height * m_W;
                        rect = GH_Convert.ToRectangle(new Rectangle(0, 0, sender.Width, (int)ancho));
                        break;
                    }
                default:
                    {
                        rect = GH_Convert.ToRectangle(new Rectangle(0, 0, sender.Width,(sender.Height)));
                        break;
                    }
            }
            sender.Graphics.DrawRectangle(pen, rect);

            SolidBrush brush = new SolidBrush(Color.Snow);
            sender.Graphics.FillRectangle(brush, rect);

            brush.Dispose();

            pen.Dispose();

            sender.Viewport.ApplyProjection(sender.Graphics);


            System.Windows.Forms.Form form = Grasshopper.Instances.DocumentEditor;

            form.TransparencyKey = Color.Snow;
            form.Show();
        }
        private PaintMode paintMode;
        private bool IsClose;
        enum PaintMode
        {
            UP,
            DOWN,
            LEFT,
            RIGHT,
            CENTER,
            FULL
        }
    }
}
