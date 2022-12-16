using DataBIDV.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBIDV.Services.Interfaces
{
    public interface IConnectAPIService
    {
        public Task<TokenAPI> Get_API_Token(RequestModel request);

        public Task<List<GiaoDichModel>> Get_DanhSachGiaoDich(string token, RequestModel request);

    }
}
