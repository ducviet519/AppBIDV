using Dapper;
using DataBIDV.Models;
using DataBIDV.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBIDV.Services.Repositories
{
    public class ConnectDatabaseServices : IConnectDatabaseServices
    {
        #region Database Connection
        private readonly IConfiguration _configuration;
        public ConnectDatabaseServices(IConfiguration configuration)
        {
            _configuration = configuration;
            ConnectionString = _configuration.GetConnectionString("DefaultConnection");
            provideName = "System.Data.SqlClient";
        }
        public string ConnectionString { get; }
        public string provideName { get; }
        public IDbConnection Connection
        {
            get { return new SqlConnection(ConnectionString); }
        }
        #endregion
        public async Task<string> Insert_GiaoDich(GiaoDich giaodich)
        {
            string result = String.Empty;
            string query = @"INSERT INTO dbo.BIDV_Demo__BienDongSoDu(id, refId, transDate, transTime, accountNo, dorc, currency, amount, remark, refNo, dateUpdated)
                            VALUES(NEWID(), @refId, @transDate, @transTime, @accountNo, @dorc, @currency, @amount, @remark, @refNo, GETDATE())";
            try
            {
                using (IDbConnection dbConnection = Connection)
                {
                    dbConnection.Open();
                    var data = await dbConnection.ExecuteAsync(query,new {
                            refId = giaodich.requestId,
                            transDate = giaodich.transDate,
                            transTime = giaodich.transTime,
                            accountNo = giaodich.accountNo,
                            dorc= giaodich.dorc,
                            currency = giaodich.curr,
                            amount = giaodich.amount,
                            remark = giaodich.remark,
                            refNo = ""
                        });
                    dbConnection.Close();
                    if(data > 0)
                    {
                        result = "OK";
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                result = ex.Message;
                return result;
            }
        }
    }
}
