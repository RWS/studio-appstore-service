namespace AppStoreIntegrationServiceManagement.Model.Customization.Interface
{
    public interface ICustomizationHelper
    {
        List<string> FontFamilies { get; set; }
        List<CustomizableField> Fields { get; set; }
        string GetFontSizeForField(string field, string defaultValue);
        string GetForegroundForField(string field, string defaultValue);
        string GetBackgroundForField(string field, string defaultValue);
        string GetFontFamilyForField(string field, string defaultValue);
    }
}
