using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel.Attributes;
using Grasshopper.Kernel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Grasshopper.Kernel.Components;
using System.Threading;

namespace CKD.CAD
{
    public class CadCurveWriterrAttrubutes:GH_ComponentAttributes
    {
        public CadCurveWriterrAttrubutes(CadCurveWriter owner):base(owner)
        {
            owner.IconDisplayMode = GH_IconDisplayMode.icon;
            start = 0;
        }

        protected override void Layout()
        {
            this.Pivot = GH_Convert.ToPoint(this.Pivot);
            this.m_innerBounds = this.LayoutControlBox();
            GH_ComponentAttributes.LayoutInputParams(base.Owner, this.m_innerBounds);
            GH_ComponentAttributes.LayoutOutputParams(base.Owner, this.m_innerBounds);
            this.Bounds = GH_ComponentAttributes.LayoutBounds(base.Owner, this.m_innerBounds);
        }

        private RectangleF LayoutControlBox()
        {
            int num = 32;
            int num2 = 32;
            Point location = GH_Convert.ToPoint(this.Pivot);
            num2 = Math.Max(num2, Math.Max(base.Owner.Params.Input.Count, base.Owner.Params.Output.Count) * 20);
            Rectangle r = new Rectangle(location, new Size(num, num2));
            float num3 = 0.5f * (float)(r.Left + r.Right);
            float num4 = 0.5f * (float)(r.Top + r.Bottom);
            this.m_buttonBounds = GH_Convert.ToRectangle(new RectangleF(num3 - 13f, num4 - 13f, 26f, 26f));
            new RectangleF(base.Owner.Attributes.Pivot.X - 0.5f * (float)num, base.Owner.Attributes.Pivot.Y - 0.5f * (float)num2, (float)num, (float)num2);
            ///UpperCenter = new PointF(base.Owner.Attributes.Pivot.X - 0.5f * (float)num, base.Owner.Attributes.Pivot.Y - 0.5f * (float)num2);
            return r;
        }

       /// private PointF UpperCenter; 

        protected override void Render(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
        {
            if (channel != GH_CanvasChannel.Wires)
            {
                if (channel != GH_CanvasChannel.Objects)
                {
                    return;
                }
            }
            else
            {
                try
                {
                    foreach(IGH_Param igh_Param in base.Owner.Params.Input)
                    {
                        igh_Param.Attributes.RenderToCanvas(canvas, GH_CanvasChannel.Wires);
                    }
                    return;
                }
                finally
                {
                    List<IGH_Param>.Enumerator enumerator=new List<IGH_Param>.Enumerator();
                    ((IDisposable)enumerator).Dispose();
                }
            }
            GH_Viewport viewport = canvas.Viewport;
            RectangleF bounds = this.Bounds;
            bool flag = viewport.IsVisible(ref bounds, 10f);
            this.Bounds = bounds;

            Rectangle buttonBounds = this.m_buttonBounds;
            Point point = canvas.PointToClient(Cursor.Position);
            if (flag )
            {
                base.RenderComponentCapsule(canvas, graphics, true, false, false, true, true, true);
                if (CadCurveWriterrAttrubutes.m_Icon == null)
                {
                    CadCurveWriterrAttrubutes.m_Icon = CKD.Properties.Resources.CadLineWriter;
                }
                int zoomFadeLow = GH_Canvas.ZoomFadeLow;
                if (zoomFadeLow > 0)
                {
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    point = GH_Convert.ToPoint(canvas.Viewport.UnprojectPoint(point));
                    if (buttonBounds.Contains(point))
                    {
                        GH_GraphicsUtil.RenderHighlightBox(graphics, buttonBounds, 0);
                    }
                    GH_GraphicsUtil.RenderCenteredIcon(graphics, buttonBounds, CadCurveWriterrAttrubutes.m_Icon, (double)zoomFadeLow / 255.0);
                                  
                }
                
            }          
        }

        public override GH_ObjectResponse RespondToMouseMove(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            if (e.Button == MouseButtons.None)
            {
                if (((RectangleF)this.m_buttonBounds).Contains(e.CanvasLocation))
                {
                    sender.Invalidate();
                    return GH_ObjectResponse.Handled;
                }
            }
            return base.RespondToMouseMove(sender, e);
        }

        public override GH_ObjectResponse RespondToMouseUp(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            GH_ObjectResponse result;

            if (start !=0 && e.Button == MouseButtons.Left && ((RectangleF)this.m_buttonBounds).Contains(e.CanvasLocation))
            {
                CadCurveWriter cadLineReader = (CadCurveWriter)base.Owner;
                cadLineReader.buttonPressed = true;
                cadLineReader.ExpireSolution(true);
                result = GH_ObjectResponse.Handled;
            }

            else
            {
                result = base.RespondToMouseUp(sender, e);
            }


            start += 1;
            return result;
        }
        public static Bitmap m_Icon;
        private Rectangle m_buttonBounds;
        /// <summary>
        /// true开始工作，
        /// </summary>
        public int start=0;
    }
}
