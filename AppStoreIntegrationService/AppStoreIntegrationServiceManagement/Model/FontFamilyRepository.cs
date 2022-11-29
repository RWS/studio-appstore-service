using AppStoreIntegrationServiceManagement.Model.Plugins;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;

namespace AppStoreIntegrationServiceManagement.Model
{
    public class FontFamilyRepository
    {
        private readonly IHttpContextAccessor _context;

        public FontFamilyRepository(IHttpContextAccessor context)
        {
            _context = context;
            Init();
        }

        public SelectList FontFamilies { get; set; }

        private void Init()
        {
            var reader = new RemoteStreamReader(new Uri("https://www.googleapis.com/webfonts/v1/webfonts?key=AIzaSyCD-AbV2jkrj3LOTmLMac6TmILG2EiJciM"));
            var items = JsonConvert.DeserializeObject<FontFamilyResponse>(reader.ReadAsStringAsync().Result).Items;
            var savedFont = _context.HttpContext.Request.Cookies["FontFamily"];
            FontFamily selectedValue = null;

            if (!string.IsNullOrEmpty(savedFont))
            {
                selectedValue = items.FirstOrDefault(f => f.Family.Equals(savedFont));
            }

            FontFamilies = new SelectList(items, null, nameof(FontFamily.Family), selectedValue);
        }
    }
}
