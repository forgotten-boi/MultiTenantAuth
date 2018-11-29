using System.Data.Common;
using System.Data.SqlClient;
using DataAccess.Data.Interfaces;
using DataAccess.Databases;
using MultiTenancy;

namespace DataAccess.Data.Repositories
{
    public class TenantDatabase: SqlDatabase, ITenantDatabase
    {
        private readonly ITenantContext _tenantContext;
        public TenantDatabase(ITenantContext tenantContext, string conString) : base(conString)
        {
            _tenantContext = tenantContext;
        }

        protected override DbConnection CreateConnection(string conString)
        {
            //modify database name here
            var tenant = _tenantContext.GetTenant();
            var builder = new SqlConnectionStringBuilder(conString)
            {
                InitialCatalog = tenant.DatabaseName
            };
            conString = builder.ConnectionString;
            return base.CreateConnection(conString);
        }

    }
}