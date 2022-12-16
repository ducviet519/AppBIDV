using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBIDV.Models
{
    public class RequestModel
    {
        public string grant_type { get; set; }
        public string scope { get; set; }
        public string client_id { get; set; }
        public string client_secret { get; set; }

    }

    public class RequestBody
    {
        public string accountNo { get; set; }
        public string transDate { get; set; }
        public int pageNum { get; set; }

    }
}
