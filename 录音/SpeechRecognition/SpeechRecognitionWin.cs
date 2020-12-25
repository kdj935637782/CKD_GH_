using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Speech.Recognition;

namespace CKD.Baidu_ai
{
    public class SpeechRecognitionWin
    {
        //创建语音识别引擎
        public SpeechRecognitionEngine recognitionEngine;
        //用于普通听写语音识别语法
        DictationGrammar dictationGrammar = new DictationGrammar();
        //定义语音识别约束
        Grammar grammar;
        //语言识别约束机制
        GrammarBuilder grammarBuilder=new GrammarBuilder();
        //创建区域信息,默认值为中国地区
        public CultureInfo CultureInfo { get; set; } = new CultureInfo("zh-CN");
        //备选列表
        Choices Choices { get; set; }
        //关键词列表
        public List<string[]> KeyWords { get; set; } = new List<string[]>();

        public SpeechRecognitionWin()
        {
            ///选择语音引擎
            foreach (RecognizerInfo config in SpeechRecognitionEngine.InstalledRecognizers())
            {
                if(config.Culture.Equals(CultureInfo))
                {
                    recognitionEngine = new SpeechRecognitionEngine(config);
                    break;
                }
            }
            if(recognitionEngine==null)
            {
                throw new Exception("初始化语音识别引擎失败");
            }
            else
            {
                recognitionEngine.SetInputToDefaultAudioDevice();
                //确定最终识别前的沉默时间
                recognitionEngine.InitialSilenceTimeout = TimeSpan.FromSeconds(5);
            }
        }

        ///开始语音识别
        public void StartRecognition()
        {
            recognitionEngine.RecognizeAsync(RecognizeMode.Multiple);
        }

        ///停止语音识别
        public void StopRecognition()
        {
            recognitionEngine.RecognizeAsyncStop();
        }

        public void SetEventHandler(EventHandler<SpeechRecognizedEventArgs>[] eventHandlers)
        {
            foreach (EventHandler<SpeechRecognizedEventArgs> eventHandler in eventHandlers)
            {
                recognitionEngine.SpeechRecognized += eventHandler;
            }
            if(KeyWords!=null)
            {
                recognitionEngine.LoadGrammar(grammar);
            }
            ///recognitionEngine.LoadGrammar(dictationGrammar);
        }

        public void SetKeyWord(string[] keyword)
        {
            KeyWords.Add(keyword);
            grammarBuilder.Append(new Choices(keyword));
            grammar = new Grammar(grammarBuilder);
        }

        public void ClearGrammarBuilder()
        {
            grammarBuilder = new GrammarBuilder();
        }

        /// <summary>
        /// 卸载约束语法
        /// </summary>
        public void UnloadGrammar()
        {
            recognitionEngine.UnloadGrammar(grammar);
            ClearGrammarBuilder();
        }

        public void UnloadAllGrammar()
        {
            recognitionEngine.UnloadAllGrammars();
        }
    }
}
