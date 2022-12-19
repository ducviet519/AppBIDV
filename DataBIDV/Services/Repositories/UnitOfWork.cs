using DataBIDV.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBIDV.Services.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        public IConnectAPI_BIDVClient API { get; }
        public IBIDV_Client BIDV_Client { get; }

        public UnitOfWork(IConnectAPI_BIDVClient connectAPI, IBIDV_Client BIDV)
        {
            API = connectAPI;
            BIDV_Client = BIDV;
        }

    }
}
