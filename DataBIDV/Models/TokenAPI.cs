using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBIDV.Models
{
    public class TokenAPI
    {
        public string token_type { get; set; }
        public string access_token { get; set; }
        public string scope { get; set; }
        public string expires_in { get; set; }
        public string consented_on { get; set; }
    }
}
