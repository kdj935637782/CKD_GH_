using System;
using System.Collections.Generic;
using System.IO;
using Grasshopper.Kernel;
using Rhino.Geometry;
using NAudio.Wave;
using Baidu.Aip.Speech;
using Baidu;
using System.Media;
using System.Speech;

// In order to load the result of this wizard, you will also need to
// add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.
// You can use the _GrasshopperDeveloperSettings Rhino command for that.

namespace CKD.Baidu_ai
{
    public class TextToVoiceComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public TextToVoiceComponent()
          : base("TextToVoice", "T",
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
            pManager.AddTextParameter("text","t","text",GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
        }

        MP3Player mP3Player=new MP3Player();
        string app_id = "18325978";
        string api_key = "aFo7CdiZ6Vjcvtl4sM9Iv5UG";
        string secret_key = "kqXn81nR8QabsBzWFrnvr3XgX8Uix6ZE";
        string filePath = @"D:\1.mp3";

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool start = false;
            string text = string.Empty;
            if (!DA.GetData(0,ref start)) { return;}
            if (!DA.GetData(1, ref text)) { return;}

            if (start)
            {
                mP3Player.Stop();
                Tts client = new Tts(api_key, secret_key);

                var option = new Dictionary<string, object>
                {
                    {"spd",4},
                    {"vol",7},
                    {"pit",9},
                    {"per",1}
                };

                var result = client.Synthesis(text, option);

                File.WriteAllBytes(filePath, result.Data);
                using (FileStream fileStream = new FileStream(filePath, FileMode.OpenOrCreate))
                {
                    fileStream.Write(result.Data, 0, result.Data.Length);
                    SoundPlayer soundPlayer = new SoundPlayer();                
                }
                mP3Player.FilePath = filePath;
                mP3Player.Play();
                File.Delete(filePath);
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
            get { return new Guid("cd4d9d9b-f50e-4cd1-a5f3-302f087f5ae4"); }
        }
    }
}
