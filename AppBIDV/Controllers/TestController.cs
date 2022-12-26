using DataBIDV.Models;
using DataBIDV.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace AppBIDV.Controllers
{
    public class TestController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IUnitOfWork _services;
        public TestController(IWebHostEnvironment webHostEnvironment, IUnitOfWork services)
        {
            _webHostEnvironment = webHostEnvironment;
            _services = services;
        }
        public async Task<IActionResult> Index()
        {
            string transDate = DateTime.Now.ToString("yyMMdd", CultureInfo.InvariantCulture);
            string accountNo = new Random().Next().ToString();

            var requestBody = new RequestBody { accountNo = accountNo, pageNum = 1, transDate = transDate };
            List<GiaoDich> DanhSachGiaoDich_Encrypt =  await _services.API.Get_DanhSachGiaoDich_Encrypt(requestBody);
            //List<GiaoDich> DanhSachGiaoDich = await _services.API.Get_DanhSachGiaoDich(requestBody);
            //await _services.API.Get_TruyVanSoDu_DauNgay(requestBody);
            //await _services.API.Get_TruyVanSoDu_TaiKhoan(requestBody);

            return View();
        }
    }
}
