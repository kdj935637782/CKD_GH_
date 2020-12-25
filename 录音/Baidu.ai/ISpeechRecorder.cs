using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Speech;

namespace CKD.Baidu_ai
{
    public interface ISpeechRecorder
    {
        void SetFileName(string fileName);
        void StartRec();
        void StopRec();
    }
}
