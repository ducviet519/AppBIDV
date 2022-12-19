using DataBIDV.Extensions;
using DataBIDV.Models;
using DataBIDV.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
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
            //var data = await _services.API.Get_API_Token();
            //string token = data.access_token;

            var requestBody = new RequestBody { accountNo = "0123456789", pageNum = 1, transDate = "221216" };
            //await _services.API.Get_DanhSachGiaoDich_Encrypt(token, requestBody);
            //await _services.API.Get_DanhSachGiaoDich(token, requestBody);

            //await _services.BIDV_Client.Get_TruyVanSoDu_DauNgay(requestBody);
            await _services.BIDV_Client.Get_DanhSachGiaoDich_Encrypt(requestBody);


            return View();
        }
    }
}
