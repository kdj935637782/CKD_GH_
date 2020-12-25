using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Grasshopper.Kernel.Parameters;
using GH_IO.Serialization;
using System.Threading.Tasks;
using System.Linq;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Microsoft.VisualBasic.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Drawing.Drawing2D;
using System.Drawing;
using Grasshopper;

namespace CKD.SpeechRecognition
{
    public class RecognitionComponent : GH_Component,IGH_VariableParameterComponent
    {
        /// <summary>
        /// Initializes a new instance of the RecognitionComponent class.
        /// </summary>
        public RecognitionComponent()
          : base("RecognitionComponent", "Nickname",
              "Description",
             "CKD", "ForFun")
        {
            commandsRecognition = new CommandsRecognition();
            commandsRecognition.SpeakStartEvent += new EventHandler(this.SpeakStart);
            commandsRecognition.SpeakStopEvent += new EventHandler(this.SpeakStop);

            base.Params.ParameterChanged += this.ParamChanged;
            this.Message = commandsRecognition.text;

            this.Mode = RecognitionMode.Close;
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
            base.RemovedFromDocument(document);
            this.Menu_CloseClicked(new object(), new EventArgs());
            this.commandsRecognition.recognitionWin.recognitionEngine.Dispose();
            RecognitionAttrubutes.m_Icon = Properties.Resources.close;
        }
        

        /// <summary>
        /// 设置下拉菜单的选择项
        /// </summary>
        /// <param name="menu"></param>
        /// <returns></returns>
        public override bool AppendMenuItems(ToolStripDropDown menu)
        {
            ToolStripMenuItem toolStripMenuItem_open = GH_DocumentObject.Menu_AppendItem(menu, "Open", new EventHandler(this.Menu_OpenClicked));
            ToolStripMenuItem toolStripMenuItem_close = GH_DocumentObject.Menu_AppendItem(menu, "Close", new EventHandler(this.Menu_CloseClicked));
            toolStripMenuItem_open.ToolTipText = "打开语音识别";
            toolStripMenuItem_close.ToolTipText = "关闭语音识别";
            switch(this.Mode)
            {
                case (RecognitionMode.Open):
                    {
                        toolStripMenuItem_open.Checked = true;break;
                    }
                case (RecognitionMode.Close):
                    {
                        toolStripMenuItem_close.Checked = true;break;
                    }
            }
            return false;
        }

        public bool CanInsertParameter(GH_ParameterSide side, int index)
        {
            return side != GH_ParameterSide.Output;
        }

        public bool CanRemoveParameter(GH_ParameterSide side, int index)
        {
            return side != GH_ParameterSide.Output && base.Params.Input.Count > 1;
        }

        public override void CreateAttributes()
        {
            this.m_attributes = new RecognitionAttrubutes(this);
        }

        public IGH_Param CreateParameter(GH_ParameterSide side, int index)
        {
            Param_GenericObject new_param = new Param_GenericObject();
            base.Params.RegisterOutputParam(new_param, index);
            return new Param_GenericObject
            {
                NickName = string.Empty
            };
        }
        public bool DestroyParameter(GH_ParameterSide side, int index)
        {
            base.Params.UnregisterOutputParameter(base.Params.Output[index]);
            return true;
        }


        private void MatchNickNames()
        {
            int num = base.Params.Input.Count - 1;
            int num2 = 0;
            while (num2 <= num && num2 < base.Params.Output.Count)
            {
                base.Params.Output[num2].NickName = base.Params.Input[num2].NickName;
                base.Params.Output[num2].Name = string.Format("Data {0}", base.Params.Output[num2].NickName);
                num2++;
            }
        }

        public void Menu_OpenClicked(object sender,EventArgs e)
        {
            if(this.Mode!=RecognitionMode.Open)
            {
                base.RecordUndoEvent("Recogniton Mode Change");
                this.Mode = RecognitionMode.Open;
                RecognitionAttrubutes.m_Icon = Properties.Resources.hear;
                CommandsRecognition.StartRecognition();
                this.ExpireSolution(true);
            }
        }

        public void Menu_CloseClicked(object sender,EventArgs e)
        {
            if (this.Mode != RecognitionMode.Close)
            {
                base.RecordUndoEvent("Recogniton Mode Change");
                this.Mode = RecognitionMode.Close;
                RecognitionAttrubutes.m_Icon = Properties.Resources.close;
                CommandsRecognition.StopRecognition();
                this.ExpireSolution(true);
            }
        }

        private void ParamChanged(object sender, GH_ParamServerEventArgs e)
        {
            IsPaarametrChange=true;
            if (e.ParameterSide != GH_ParameterSide.Output && (e.OriginalArguments.Type == GH_ObjectEventType.NickName || e.OriginalArguments.Type == GH_ObjectEventType.NickNameAccepted))
            {
                this.MatchNickNames();
            }
        }

        private void SpeakStart(object sender,EventArgs e)
        {
            RecognitionAttrubutes.m_Icon = Properties.Resources.speak;
            ((RecognitionAttrubutes)this.Attributes).m_overButton = 3;
            this.ExpireSolution(true);
            Mode = RecognitionMode.Speaking;
                    
        }

        private void SpeakStop(object sender, EventArgs e)
        {
            RecognitionAttrubutes.m_Icon = Properties.Resources.hear;
            ((RecognitionAttrubutes)this.Attributes).m_overButton = 2;
            this.ExpireSolution(true);
            Mode = RecognitionMode.Open;
            
        }


        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Geometry", "A", "Geometry to operate", GH_ParamAccess.list);
            pManager[0].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Geometry", "A", "Geometry after operate", GH_ParamAccess.list);
        }

        private void RepairParameterMatching()
        {
            if (base.Params.Output.Count != base.Params.Input.Count)
            {
                while (base.Params.Output.Count != base.Params.Input.Count)
                {
                    if (base.Params.Output.Count < base.Params.Input.Count)
                    {
                        base.Params.RegisterOutputParam(new Param_GenericObject());
                    }
                    else
                    {
                        base.Params.UnregisterOutputParameter(base.Params.Output[base.Params.Output.Count - 1]);
                    }
                }
                this.VariableParameterMaintenance();
            }
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            int num = base.Params.Input.Count - 1;
            if (commandsRecognition.result == null||IsPaarametrChange)
            {
                for (int i = 0; i <= num; i++)
                {
                    if (!DA.GetDataList<object>(i,commandsRecognition.result)) { return; }
                }
            }
            if(commandsRecognition.result!=null)
            {
                for(int i=0;i<base.Params.Input.Count;i++)
                {
                    DA.SetDataList(i, commandsRecognition.result);
                }
            }

        }

        private void TaskFinsh(object seder,EventArgs e)
        {
            this.ComputeData();
        }

        public Size TextCenter()
        {           
            int factor = 20;
            if (commandsRecognition.text != string.Empty)
            {
                string[] ContentLines = commandsRecognition.text.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
                int maxWidth = 0;
                int count = ContentLines.Length;
                foreach (string s in ContentLines)
                {
                    if (maxWidth <= s.Length) { maxWidth = s.Length; }
                }
                Size size = new Size();
                size.Width = maxWidth * 20;
                size.Height = count * factor+5;
                return size;
            }
            return new Size();
        }

        public void VariableParameterMaintenance()
        {
            int num = base.Params.Input.Count - 1;
            for (int i = 0; i <= num; i++)
            {
                IGH_Param igh_Param = base.Params.Input[i];
                if (Operators.CompareString(igh_Param.NickName, string.Empty, false) == 0)
                {
                    igh_Param.NickName = GH_ComponentParamServer.InventUniqueNickname("ABCDEFGHIJKLMNOPQRSTUVWXYZ", base.Params.Input);
                }
                igh_Param.Name = string.Format("Data {0}", igh_Param.NickName);
                igh_Param.Description = "Data to buffer";
                igh_Param.Access = GH_ParamAccess.tree;
                igh_Param.Optional = true;
            }
            if (base.Params.Output.Count != base.Params.Input.Count)
            {
                this.RepairParameterMatching();
            }
            int num2 = base.Params.Input.Count - 1;
            for (int j = 0; j <= num2; j++)
            {
                base.Params.Output[j].Name = base.Params.Input[j].Name;
                base.Params.Output[j].NickName = base.Params.Input[j].NickName;
                base.Params.Output[j].Description = "Buffered data";
                base.Params.Output[j].MutableNickName = false;
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
                return Properties.Resources.hear;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("c30b02da-6394-4989-8162-dfd30208f1e3"); }
        }

        /// <summary>
        /// 描述开启还是关闭
        /// </summary>
        public RecognitionMode Mode { get; private set; }

        public bool RecognitionPossible
        {
            get
            {
                return this.Mode == RecognitionComponent.RecognitionMode.Open||this.Mode==RecognitionMode.Speaking;    ////&& !this.m_isSpeaking;
            }
        }

        public CommandsRecognition CommandsRecognition
        {
            get
            {
                return this.commandsRecognition;
            }
        }

        private bool IsPaarametrChange;
        public CommandsRecognition commandsRecognition;

        /// <summary>
        /// 录音模式，选择开启还是关闭还是延迟时间 
        /// </summary>
        public enum RecognitionMode
        {
            Open,
            Close,
            Speaking
        }
        
    }
}