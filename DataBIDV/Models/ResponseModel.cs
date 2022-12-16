using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBIDV.Models
{
    public class ResponseModel<T> where T : class
    {
        public List<T> rows { get; set; }
        public int totalRow { get; set; }
    }
}
