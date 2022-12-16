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
        public async Task<IActionResult> Index(RequestModel request)
        {
            string token = StaticHelper.EncodeBase64("0123456789");
            request.client_id = System.IO.File.ReadAllText(Path.Combine(_webHostEnvironment.ContentRootPath, "Keys\\client_id.asc"));
            request.client_secret = System.IO.File.ReadAllText(Path.Combine(_webHostEnvironment.ContentRootPath, "Keys\\client_secret.asc"));
            request.grant_type = "client_credentials";
            request.scope = "read";
            await _services.API.Get_API_Token(request);
            await _services.API.Get_DanhSachGiaoDich(token,request);
           
            return View();
        }
    }
}
