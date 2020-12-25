using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Speech.Recognition;
using System.Speech.Synthesis;
using System.Text.RegularExpressions;
using System.Globalization;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Windows.Forms;
using System.Threading;

namespace CKD.SpeechRecognition
{
    public delegate void MyEventHandler();
    public class CommandsRecognition
    {
        /// <summary>
        /// 开始语音播报的回调函数
        /// </summary>
        public EventHandler SpeakStartEvent;
        /// <summary>
        /// 结束语音播报的回调函数
        /// </summary>
        public EventHandler SpeakStopEvent;
        /// <summary>
        /// 指令完成的回调函数
        /// </summary>
        public EventHandler TaskFinsh;

        protected SpeechSynthesizer speechSynthesizer;
        public SpeechRecognitionWin recognitionWin;
        public bool IsSpeaking { get; private set; } = false;
        public string[] commandKeyWords { get; private set; }
        ///public string[] number { get; private set; }
        public Dictionary<string, int> number;
        /// <summary>
        /// 参数有负数时需要传入
        /// </summary>
        public string[] symbol = { " ","负"};

        ///提示文本
        public string text=string.Empty;
        
        /// <summary>
        /// true时，tips可改,否则不可改
        /// </summary>
        bool flag = true;
        ///语言合成的文本
        private  string tips
        {
            get { return _tips; }
            set
            {
                if (flag)
                {
                    _tips = value;
                    
                    speechSynthesizer.SpeakAsync(tips);
                }
            }
        }
        private string _tips;

        /// 命令的结果，Rhino的Geometry
        public List <object> result;

        Dictionary<string,int?> parameters;
        event MyEventHandler CommandeventHandler;
        
        /// <summary>
        /// 初始化
        /// </summary>
        public CommandsRecognition()
        {
            InitalcommandKeyWords();
            InitalNumber();
            result = new List<object>();

            speechSynthesizer = new SpeechSynthesizer();
            speechSynthesizer.SpeakStarted += new EventHandler<SpeakStartedEventArgs>(
                (object a, SpeakStartedEventArgs e) =>
                {
                    StopRecognition();
                    flag = false;
                    IsSpeaking = true;
                    if (SpeakStartEvent != null) { SpeakStartEvent.Invoke(new object(), new EventArgs()); }
                });
            speechSynthesizer.SpeakCompleted += new EventHandler<SpeakCompletedEventArgs>(
                (object sender, SpeakCompletedEventArgs e) =>
                {
                    StartRecognition();
                    flag = true;
                    IsSpeaking = false;
                    if (SpeakStopEvent != null) { SpeakStopEvent.Invoke(new object(), new EventArgs()); }
                });

            recognitionWin = new SpeechRecognitionWin(EventProcessing,commandKeyWords);
            recognitionWin.recognitionEngine.SpeechRecognitionRejected+=new EventHandler<SpeechRecognitionRejectedEventArgs>(
                (object a, SpeechRecognitionRejectedEventArgs e)=>
                {
                    text=tips = "对不起，我没听清楚！";
                });
        }

        /// <summary>
        /// /初始化命令关键字
        /// </summary>
        /// <param name="keyWords"></param>
        protected virtual void InitalcommandKeyWords() 
        {
            commandKeyWords = new string[]
            {
                "保存",
                "关闭",
                "参数调试",
                "开发者",
                "指令集",
                "创建长方体",
                "前进",
                "后退",
                "左移",
                "右移",
                "上升",
                "下降",
                "顺时针旋转",
                "逆时针旋转",
                "放大",
                "缩小",
                "取消",
                "返回",
            };
        }

        /// <summary>
        /// 初始化参数数组
        /// </summary>
        protected virtual void InitalNumber()
        {
            number = new Dictionary<string ,int>();
            int j = 1000;
            for (int i=0; i<=j; i++)
            {
                number.Add(NumberToZh(i ), i );
            }
            number.Add("取消",0000);
            number.Add("返回", 0001);
        }

        /// <summary>
        /// 阿拉伯数字转化为中文
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        protected virtual string NumberToZh(int source)
        {
            int flag = 0;///检测是正数还是负数
            
            string[] unit ={ "","十", "百", "千","万"};
            string target=string.Empty;
            Dictionary<int, string> number = new Dictionary<int, string>()
            {
                { 0,"零"},
                { 1,"一"},
                { 2,"二"},
                { 3,"三"},
                { 4,"四"},
                { 5,"五"},
                { 6,"六"},
                { 7,"七"},
                { 8,"八"},
                { 9,"九"},
            };
            if (source < 0)
            {
                target += symbol[1];
                source = source * (-1);
                flag = 1;
            }
            for(int i=0;i<source .ToString().Length;i++)
            {
                string value = string.Empty;
                number.TryGetValue(int.Parse(source.ToString()[i].ToString()), out value);
                target += value;
                if (value != "零") { target += unit[source.ToString().Length - i - 1]; }
                
                if (source % (Math.Pow(10, source.ToString().Length - i-1)) == 0){ break; }
            }
            if (source < 20 && source >= 10) { target = target.Remove(0+flag, 1); }
            return target;
        }

        #region 
        /*
        /// <summary>
        /// 事件处理程序
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void EventProcessing(object sender,SpeechRecognizedEventArgs e)
        {            
            tips = e.Result.Text+"\n";
            ///匹配命令语句
            string commandPattern = @"^[\u4e00-\u9fa5]+";
            string command = string.Empty;
            ////匹配整数参数
            string paramPattern = @"-?[1-9]\d*";
            int[] a = new int[0];
            int[] parameter = new int[recognitionWin.KeyWords == null ? 0 : recognitionWin.KeyWords.Length - 1];

            foreach (Match match in Regex.Matches(e.Result.Text,commandPattern))
            {
                command = match.Value;
            }
            int i = 0;
            foreach (Match match in Regex.Matches(e.Result.Text, paramPattern))
            {
                tips+= match.Value;
                i += 1;
            }

            ///检测是否有命令输入
            if (command.Length == 0)
            {
                tips += "没有检测到有语言输入";
            }
            else
            {
                switch (command)
                {
                    case "开发者":
                        {
                            tips = "你猜猜开发者是谁，猜到就告诉你";
                            break;
                        }
                    case "参数调试":
                        {
                            if (parameter.Length == 0)
                            {
                                tips += "没有输入参数";
                                break;
                            }
                            else
                            {
                                tips += "参数为:" + parameter[0].ToString();
                                break;
                            }
                        }
                }
            }
        }*/
        #endregion

        /// <summary>
        /// 事件处理程序
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void EventProcessing(object sender, SpeechRecognizedEventArgs e)
        {
            ///选择命令
            if (CommandeventHandler == null)
            {
                text = string.Empty;
                switch (e.Result.Text)
                {
                    case ("返回"):
                        {
                            text = tips = "执行返回命令";
                            if (result!=null||result.Count>=1)
                            {
                                result.RemoveAt(result.Count - 1);
                            }
                            return;
                        }
                    case (" 保存"):
                        {
                            break;
                        }
                    case ("指令集"):
                        {
                            foreach(string key in commandKeyWords)
                            {
                                text += key+"\n";
                            }
                            tips = text;
                            return;
                        }
                    case ("参数调试"):
                        {
                            ResetRecEngine (symbol,number.Keys.ToArray());
                            parameters = new Dictionary<string, int?>()
                                    {
                                        { "第一个参数",null},
                                        { "第二个参数",null},
                                        { "第三个参数",null},
                                    };
                            CommandeventHandler += new MyEventHandler(
                                () =>
                                {
                                    Reset();
                                });
                            StartRecognition();
                            break;
                        }
                    case ("开发者"):
                        {
                            text = tips = "陈锴迪";
                            return ;
                        }
                    case ("创建长方体"):
                        {
                            ResetRecEngine(number.Keys.ToArray());
                            parameters = new Dictionary<string, int?>()
                            {
                                {"长",null },
                                {"宽",null },
                                {"高",null },
                            };
                            CommandeventHandler += new MyEventHandler(
                                () =>
                                {
                                    Point3d point1 = new Point3d(0, 0, 0);
                                    Point3d point2 = new Point3d((double)parameters.Values.ToArray()[0],
                                        (double)parameters.Values.ToArray()[1],
                                        (double)parameters.Values.ToArray()[2]);
                                BoundingBox boundingBox = new BoundingBox(point1 ,point2);
                                    result.Add( boundingBox.ToBrep());
                                    Reset();
                                });
                            StartRecognition();
                            break;
                        }
                   /* case ("前进"):
                        {
                            ResetRecEngine(number.Keys.ToArray());
                            parameters = new Dictionary<string, int?>()
                            {
                                {"距离",null }
                            };
                            CommandeventHandler += new MyEventHandler(
                                () =>
                                {
                                    for(int i=0;i<result.Count;i++)
                                    {
                                        result[i]
                                    };
                                    Reset();
                                });
                            StartRecognition();
                            break;
                        }*/
                    default:
                        {
                            ///MessageBox.Show("running default command");
                            return; ;
                        }
                }
                    string pa = string.Empty;
                    for (int i = 0; i < parameters.Count; i++)
                    {
                        pa += parameters.Keys.ToArray()[i] + ";";
                    }
                    tips = "执行" + e.Result.Text + "命令\n" + "请输入" + pa + "\n";
                    text = tips;
            }

            ///选择参数
            else
            {
                if(e.Result.Text=="取消"||e.Result.Text=="返回")
                {
                    switch(e.Result.Text)
                    {
                        case ("取消"):
                            {
                                CommandeventHandler = null;
                                parameters = null;
                                text = string.Empty; tips = string.Empty;
                                Reset();
                                break;
                            }
                        case ("返回"):
                            {
                                for (int i = 0; i < parameters.Count; i++)
                                {
                                    if (parameters.Values.ToArray()[i] == null)
                                    {
                                        if (i == 0) { Reset(); return; }
                                        parameters[parameters.Keys.ToArray()[i - 1]] = null;
                                        string patterns = @"[\u4e00-\u9fa5]+\W*\d*\n";
                                        int count = 0;
                                        string temp = string.Empty;
                                        foreach (Match match in Regex.Matches(text,patterns))
                                        {
                                            if (count < i)
                                            {
                                                temp += match.Value;
                                                count += 1;
                                            }
                                            text = temp;
                                        }
                                        tips = string.Empty;
                                        break;
                                    }
                                }
                                break;
                            }
                        default:
                            {
                                MessageBox.Show("running default params");
                                break;
                            }
                    }
                    return;
                }
                for(int i=0;i<parameters.Count;i++)
                {

                    string pattern;
                    int flag = 1;
                    if (e.Result.Text[0].ToString() == "负") { flag = -1; pattern = e.Result.Text.Remove(0, 1); }
                    else { pattern = e.Result.Text; }
                    if (parameters.Values.ToArray()[i]==null)
                    {
                        int a = -2048;
                        number.TryGetValue(pattern , out a);
                        parameters[parameters.Keys.ToArray()[i]] = a*flag;
                        tips= parameters.Keys.ToArray()[i] + "为" + e.Result.Text + "\n";
                        text += parameters.Keys.ToArray()[i] + "为" + (flag*a).ToString() + "\n";
                        if (i != parameters.Count - 1) { return; }
                    }
                }
                CommandeventHandler.Invoke();
                ///回调函数
                if (TaskFinsh != null) { TaskFinsh.Invoke(new object(),new EventArgs()); }
            }
        }

        public virtual void StartRecognition()
        {
            try { recognitionWin.recognitionEngine.RecognizeAsync(RecognizeMode.Multiple); }
            catch { }
        }

        public virtual void StopRecognition()
        {
            recognitionWin.recognitionEngine.RecognizeAsyncStop();
        }

        /// <summary>
        /// 重置参数及对应的事件处理程序以及恢复关键字为command
        /// </summary>
        protected virtual void Reset()
        {
            parameters = null;
            CommandeventHandler = null;
            //text = string.Empty;
            tips = string.Empty;
            ResetRecEngine(commandKeyWords);
            StartRecognition();
        }

        /// <summary>
        /// 重置约束语法
        /// </summary>
        /// <param name="keyword"></param>
        protected virtual void ResetRecEngine(params string[][] keywords)
        {
            try { StopRecognition(); } catch { }
            recognitionWin.recognitionEngine=null;
            recognitionWin = new SpeechRecognitionWin(EventProcessing,keywords);
        }

        /// <summary>
        /// 
        /// </summary>
        public class SpeechRecognitionWin
        {
            //创建语音识别引擎
            public SpeechRecognitionEngine recognitionEngine;
            //定义语音识别约束
            Grammar grammar;
            //语言识别约束机制
            GrammarBuilder grammarBuilder;
            //创建区域信息,默认值为中国地区
            public CultureInfo CultureInfo { get; set; } = new CultureInfo("zh-CN");
            //关键词列表
            public string[][] KeyWords;

            public SpeechRecognitionWin(EventHandler<SpeechRecognizedEventArgs> eventHandler, params string[][] keyWords)
            {
                ///选择语音引擎
                foreach (RecognizerInfo config in SpeechRecognitionEngine.InstalledRecognizers())
                {
                    if (config.Culture.Equals(CultureInfo))
                    {
                        recognitionEngine = new SpeechRecognitionEngine(config);
                        break;
                    }
                }
                if (recognitionEngine == null)
                {
                    throw new Exception("初始化语音识别引擎失败");
                }
                else
                {
                    if (keyWords.Length == 0)
                    {
                        recognitionEngine.LoadGrammar(new DictationGrammar());
                    }
                    else
                    {
                        KeyWords = keyWords;
                        grammarBuilder = new GrammarBuilder();
                        foreach (string[] KeyWord in KeyWords)
                        {
                            Choices Choices = new Choices(KeyWord);
                            grammarBuilder.Append(Choices);
                        }
                        grammar = new Grammar(grammarBuilder);
                        recognitionEngine.LoadGrammar(grammar);
                    }
                    recognitionEngine.SetInputToDefaultAudioDevice();
                    //确定最终识别前的沉默时间
                    recognitionEngine.InitialSilenceTimeout = TimeSpan.FromSeconds(1);
                    ///recognitionEngine.BabbleTimeout = TimeSpan.FromSeconds(5);
                    //设置事件处理程序
                    recognitionEngine.SpeechRecognized += eventHandler;
                }
            }
        }
    }
}
