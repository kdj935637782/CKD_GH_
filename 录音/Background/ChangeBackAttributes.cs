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

namespace CKD.Background
{
    public class ChangeBackAttrubutes : GH_ComponentAttributes
    {
        public ChangeBackAttrubutes(ChangeBackComponent owner) : base(owner)
        {
            owner.IconDisplayMode = GH_IconDisplayMode.icon;
            m_overButton = false;
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
                    foreach (IGH_Param igh_Param in base.Owner.Params.Input)
                    {
                        igh_Param.Attributes.RenderToCanvas(canvas, GH_CanvasChannel.Wires);
                    }
                    return;
                }
                finally
                {
                    List<IGH_Param>.Enumerator enumerator = new List<IGH_Param>.Enumerator();
                    ((IDisposable)enumerator).Dispose();
                }
            }
            GH_Viewport viewport = canvas.Viewport;
            RectangleF bounds = this.Bounds;
            bool flag = viewport.IsVisible(ref bounds, 10f);
            this.Bounds = bounds;

            Rectangle buttonBounds = this.m_buttonBounds;
            Point point = canvas.PointToClient(Cursor.Position);
            if (flag)
            {
                base.RenderComponentCapsule(canvas, graphics, true, false, false, true, true, true);
                if (ChangeBackAttrubutes.m_Icon == null)
                {
                    ChangeBackAttrubutes.m_Icon = Properties.Resources.close2;
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
                    GH_GraphicsUtil.RenderCenteredIcon(graphics, buttonBounds, ChangeBackAttrubutes.m_Icon, (double)zoomFadeLow / 255.0);

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
            if (clickNumber == null)
            {
                clickNumber = 1;
                result = base.RespondToMouseUp(sender, e);
            }
            else if (e.Button == MouseButtons.Left && ((RectangleF)this.m_buttonBounds).Contains(e.CanvasLocation))
            {
                ChangeBackComponent recognitionComponent = (ChangeBackComponent)base.Owner;
                if (!m_overButton)
                {
                    m_overButton = true;
                    Task.Run(() =>
                    {
                        m_Icon = Properties.Resources.open;
                        recognitionComponent.ExpireSolution(true);
                    });
                }
                else if (m_overButton)
                {
                    m_overButton = false;
                    Task.Run(() =>
                    {
                        m_Icon = Properties.Resources.close2;
                        recognitionComponent.ExpireSolution(true);
                    });

                }
                result = GH_ObjectResponse.Handled;
            }
            else
            {
                result = base.RespondToMouseUp(sender, e);
            }
            return result;
        }

        public override void SetupTooltip(PointF canvasPoint, GH_TooltipDisplayEventArgs e)
        {
            if (!((RectangleF)this.m_buttonBounds).Contains(canvasPoint))
            {
                base.SetupTooltip(canvasPoint, e);
                return;
            }
            e.Title = "Change baskground";
            e.Text = "Click here to change background";
            if (m_overButton)
            {
                e.Icon = Properties.Resources.open;
                return;
            }
            e.Icon = Properties.Resources.close2;
        }
        
        public static Bitmap m_Icon;
        private Rectangle m_buttonBounds;
        public bool m_overButton;
        private int? clickNumber;
    }
}
