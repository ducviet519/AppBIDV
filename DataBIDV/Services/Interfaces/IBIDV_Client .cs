using DataBIDV.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBIDV.Services.Interfaces
{
    public interface IBIDV_Client
    {
        public Task<List<GiaoDich>> Get_DanhSachGiaoDich_Encrypt(RequestBody request);

        public Task<List<GiaoDich>> Get_DanhSachGiaoDich(RequestBody request);

        public Task<List<GiaoDich>> Get_TruyVanSoDu_DauNgay(RequestBody request);

        public Task<GiaoDich> Get_TruyVanSoDu_TaiKhoan(RequestBody request);
    }
}
