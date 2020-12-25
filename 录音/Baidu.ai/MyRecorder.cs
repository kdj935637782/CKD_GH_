using System;
using System.Collections.Generic;
using System.Text;
using NAudio;
using NAudio.Wave;
using System.Threading.Tasks;
using System.Linq;
using System.Windows.Forms;

namespace CKD.Baidu_ai
{
    public delegate void MyEvntHandler();
    public class MyRecorder:ISpeechRecorder
    {
        public WaveIn waveIn = null;
        public WaveFileWriter waveFileWriter = null;
        private string fileName = string.Empty;
        public Timer time = new Timer();
        public MyEvntHandler doWhenStart;
        public MyEvntHandler doWhenStop;

        public MyRecorder()
        {
            time.Interval = 5000;
            time.Tick += new EventHandler(StopByTimer);
            time.Enabled = false;
        }

        ///开始录音
        public void StartRec()
        {
            doWhenStart?.Invoke();

            waveIn = new WaveIn();
            waveIn.WaveFormat = new WaveFormat(16000,16,1);// 16bit,16KHz,Mono的录音格式
            waveFileWriter = new WaveFileWriter(fileName, waveIn.WaveFormat);

            waveIn.DataAvailable += new EventHandler<WaveInEventArgs>(waveIn_DataAvailable);
            waveIn.RecordingStopped += new EventHandler<StoppedEventArgs>(waveIn_RecordingStopped);
            
            waveIn.StartRecording();
            time.Enabled = true;
        }

        private  void StopByTimer(object sender,EventArgs e)
        {
            StopRec();
        }
        ///停止录音
        public void StopRec()
        {
            time.Enabled = false;
            waveIn.StopRecording();
            waveFileWriter.Close();

            if(waveIn!=null)
            {
                waveIn.Dispose();
                waveIn = null;
            }
            if(waveFileWriter!=null)
            {
                waveFileWriter.Dispose();
                waveFileWriter = null;
            }

            doWhenStop?.Invoke();
        }

        ///结束录音保存文件的路径
        public void SetFileName(string fileName)
        {
            this.fileName = fileName;
        }

        ///开始录音回调函数
        private void waveIn_DataAvailable(object sender,WaveInEventArgs e)
        {
            if(waveFileWriter!=null)
            {
                waveFileWriter.Write(e.Buffer, 0, e.BytesRecorded);
                waveFileWriter.Flush();
            }
        }

        ///录音结束之后的回调函数
        private void waveIn_RecordingStopped(object sender,EventArgs e)
        {
            if (waveIn != null)
            {
                waveIn.Dispose();
                waveIn = null;
            }
            if (waveFileWriter != null)
            {
                waveFileWriter.Dispose();
                waveFileWriter = null;
            }
        }
    }
}
