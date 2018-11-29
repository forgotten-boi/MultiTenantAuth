using MultiTenancy;

namespace Entities
{
    public class AppTenant : IAppTenant
    {
        public int TenantID { get; set; }
        public string TenantName { get; set; }
        public string APIKey { get; set; }
        public string SigningKey { get; set; }
        public string HostName { get; set; }
        public string DatabaseName { get; set; }
        public string DateFormat { get; set; }
    }
}
