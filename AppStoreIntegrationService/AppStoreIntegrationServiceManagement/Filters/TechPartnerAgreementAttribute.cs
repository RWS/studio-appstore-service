using Microsoft.AspNetCore.Mvc;

namespace AppStoreIntegrationServiceManagement.Filters
{
    public class TechPartnerAgreementAttribute : TypeFilterAttribute
    {
        public TechPartnerAgreementAttribute() : base(typeof(TechPartnerAgreementFilter)) { }
    }
}
