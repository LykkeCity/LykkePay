using System;
using System.Collections.Generic;
using System.Text;
using LykkePay.Business;

namespace LykkePay.Business.Test.Models
{
    class TestRequest:BaseRequest
    {
        public string Test1 { get; set; }
        public int Test2 { get; set; }
        public string Test3 { get; set; }
        public int Test4 { get; set; }
    }
}
