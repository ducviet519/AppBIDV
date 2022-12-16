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
        public IConnectAPIService API { get; }
        public UnitOfWork(IConnectAPIService connectAPI)
        {
            API = connectAPI;
        }

    }
}
