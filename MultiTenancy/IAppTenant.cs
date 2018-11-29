namespace MultiTenancy
{
    public interface IAppTenant
    {
        int TenantID { get; set; }
        string TenantName { get; set; }
        string APIKey { get; set; }
        string SigningKey { get; set; }
        string HostName { get; set; }
        string DatabaseName { get; set; }
        string DateFormat { get; set; }
    }
}