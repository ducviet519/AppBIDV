﻿using DataBIDV.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBIDV.Services.Interfaces
{
    public interface IConnectAPI_BIDVClient
    {
        public Task<TokenAPI> Get_API_Token();

        public Task<List<GiaoDich>> Get_DanhSachGiaoDich_Encrypt(string token, RequestBody request);

        public Task<List<GiaoDich>> Get_DanhSachGiaoDich(string token, RequestBody request);

        public Task<List<GiaoDich>> Get_TruyVanSoDu_DauNgay(string token, RequestBody request);

        public Task<List<GiaoDich>> Get_TruyVanSoDu_TaiKhoan(string token, RequestBody request);
    }
}
