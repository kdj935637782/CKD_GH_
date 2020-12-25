using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CKD.Baidu_ai
{ 
    public class SpeechModel
    {
        public string err_no { get; set; }
        public string err_msg { get; set; }
        public string corpus_no { get; set; }
        public string sn { get; set; }
        public List<string> result { get; set; }
    }
}
