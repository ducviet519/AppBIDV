using DataBIDV.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBIDV.Services.Interfaces
{
    public interface IConnectDatabaseServices
    {
        public Task<string> Insert_GiaoDich(GiaoDich giaodich);
    }
}
