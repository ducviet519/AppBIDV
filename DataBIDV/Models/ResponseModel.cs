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

    public class ResponseToken
    {
        public string token_type { get; set; }
        public string access_token { get; set; }
        public string scope { get; set; }
        public string expires_in { get; set; }
        public string consented_on { get; set; }
    }

    public class ResponseGiaoDich_Encrypt
    {
        public string requestId { get; set; }
        public string errorCode { get; set; }
        public string errorDesc { get; set; }
        public string data { get; set; }
    }

    public class ResponseGiaoDich
    {
        public string requestId { get; set; }
        public string errorCode { get; set; }
        public string errorDesc { get; set; }
        public int totalRow { get; set; }
        public List<GiaoDichModel> rows { get; set; }
    }

    public class ResponseSoDuDauNgay
    {
        public string requestId { get; set; }
        public string errorCode { get; set; }
        public string errorDesc { get; set; }
        public int totalRow { get; set; }
        public List<SoDuModel> rows { get; set; }
    }

    public class ResponseSoDuTaiKhoan
    {
        public string requestId { get; set; }
        public string errorCode { get; set; }
        public string errorDesc { get; set; }
        public SoDuModel data { get; set; }
    }
}
