using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Speech.Recognition;
using System.Media;
using Baidu.Aip.Speech;
using System.IO;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace CKD.Baidu_ai
{
    public class CommandsRec
    {
        string app_id = "18325978";
        string api_key = "aFo7CdiZ6Vjcvtl4sM9Iv5UG";
        string secret_key = "kqXn81nR8QabsBzWFrnvr3XgX8Uix6ZE";

        //播放引擎
        SoundPlayer soundPlayer;
        //录音引擎
        MyRecorder myRecorder;
        
        //缓存文件路径
        public string filePath = @"D:\1.wav ";       
        //语音合成的文本
        public string tips=string.Empty;
        /// 命令的结果，Rhino的Geometry
        public object []result;

        public CommandsRec()
        {
            soundPlayer = new SoundPlayer();

            myRecorder = new MyRecorder();
            myRecorder.SetFileName(filePath);
            myRecorder.doWhenStart = new MyEvntHandler(StartRecHappend);
            myRecorder.doWhenStop = new MyEvntHandler(StopRecHappend);
            
            myRecorder.StartRec();
        }

        ///开始录音的回调函数
        void StartRecHappend()
        {
            tips = "正在倾听!";
            TextToVoice(tips);
        }

        ///结束录音的回调函数
        void StopRecHappend()
        {
            tips = "正在识别!";
            TextToVoice(tips);
            tips=SpeechRecognition.AsrData(filePath);

            myRecorder.StartRec();
        }

        ///语音合成
        void TextToVoice(string text)
        {
            soundPlayer.Stop();
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
            soundPlayer.SoundLocation = filePath;
            soundPlayer.Play();
            File.Delete(filePath);
        }
    }
}
