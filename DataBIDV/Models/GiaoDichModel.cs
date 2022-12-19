using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBIDV.Models
{
    public class GiaoDichModel
    {

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
    public class Data_GiaoDich
    {
        public List<Row_GiaoDich> rows { get; set; }
        public int totalRow { get; set; }
    }

    public class Root_GiaoDich
    {
        public string requestId { get; set; }
        public Data_GiaoDich data { get; set; }
        public string errorCode { get; set; }
        public string errorDesc { get; set; }
    }

    public class Row_GiaoDich
    {
        public string transDate { get; set; }
        public string transTime { get; set; }
        public string remark { get; set; }
        public string dorc { get; set; }
        public string amount { get; set; }
    }
}
