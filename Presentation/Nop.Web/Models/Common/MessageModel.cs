using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Nop.Web.Models.Common
{
    //AF
    public class MessageModel
    {
        public List<string> MessageList { get; set; }
        public List<string> MessageListExt { get; set; }
        public string ActionUrl { get; set; }
        public string ActionText { get; set; }
        public bool Successful { get; set; }
     
        public MessageModel()
        {
            MessageList=new List<string>();
            MessageListExt = new List<string>();
        }
    }
}