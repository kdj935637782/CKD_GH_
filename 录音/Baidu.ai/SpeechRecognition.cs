using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Speech.Recognition;
using System.Speech;
using System.Threading;
using Microsoft.Win32;
using System.IO;
using Baidu.Aip.Speech;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace CKD.Baidu_ai
{
    public class SpeechRecognition
    {
        private static string APP_ID= "18325978";
        private static string API_KEY= "aFo7CdiZ6Vjcvtl4sM9Iv5UG";
        private static string SECRET_KEY = "kqXn81nR8QabsBzWFrnvr3XgX8Uix6ZE";

        private static Asr _asrClient = new Asr(APP_ID,API_KEY, SECRET_KEY) { Timeout = 6000 };
        private static Tts _ttsClient = new Tts(API_KEY, SECRET_KEY) { Timeout = 6000 };

        static string audioFormat = "wav";
        static SpeechModel speechModel=new SpeechModel();

        static public Dictionary<string,object> options = new Dictionary<string, object>
            {
                {"dev_pid",1536}
            };

        /// <summary>
        /// 通过文件路径，调用百度语音识别接口返回识别结果
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="audioFormat">音频格式</param>
        /// <returns></returns>
        public static string AsrData(string filePath)
        {
            try
            {
                byte[] data = File.ReadAllBytes(filePath);
                _asrClient.Timeout = 120000; // 若语音较长，建议设置更大的超时时间. ms
                var result = _asrClient.Recognize(data, audioFormat, 16000,options);
                var speechRedult = JsonConvert.DeserializeObject<SpeechModel>(result.ToString());
                return speechModel.result[0];
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
    }
}
