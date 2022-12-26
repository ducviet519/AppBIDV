using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBIDV.Services.Interfaces
{
    public interface IUnitOfWork
    {
        IConnectAPI_BIDVClient API { get; }
    }
}
