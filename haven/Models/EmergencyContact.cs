namespace Haven.Models;

public class EmergencyContact
{
    public string Number { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string NameBn { get; set; } = string.Empty;
    public string DescriptionEn { get; set; } = string.Empty;
    public string DescriptionBn { get; set; } = string.Empty;
    public string TagEn { get; set; } = string.Empty;
    public string TagBn { get; set; } = string.Empty;
    public string Icon { get; set; } = "phone";
    public bool IsTollFree { get; set; }
    public bool Is24x7 { get; set; } = true;
    public string ColorScheme { get; set; } = "rose"; // rose, amber, emerald, sky, indigo
}
