﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBIDV.Models
{
    public class GiaoDichModel
    {
        public string amount { get; set; }
        public string curr { get; set; }
        public string dorc { get; set; }
        public string transDate { get; set; }
        public string transTime { get; set; }
        public string remark { get; set; }
    }

    public class GiaoDich
    {
        public string requestId { get; set; }
        public string amount { get; set; }
        public string curr { get; set; }
        public string dorc { get; set; }
        public string transDate { get; set; }
        public string transTime { get; set; }
        public string remark { get; set; }
        public string accountNo { get; set; }
    }
}
