namespace Haven.Models;

public class TherapistViewModel
{
    public int Id { get; set; }
    public string NameEn { get; set; } = string.Empty;
    public string NameBn { get; set; } = string.Empty;
    public string TitleEn { get; set; } = string.Empty;
    public string TitleBn { get; set; } = string.Empty;
    public string RegistrationNo { get; set; } = string.Empty; // e.g. BMDC Reg: A-84920 or BCPS/DU-Psych-402
    public bool IsBMDCVerified { get; set; } = true;
    public string DegreeEn { get; set; } = string.Empty;
    public string DegreeBn { get; set; } = string.Empty;
    public string InstitutionEn { get; set; } = string.Empty;
    public string InstitutionBn { get; set; } = string.Empty;
    public int ExperienceYears { get; set; } = 6;
    public double Rating { get; set; } = 4.95;
    public int ReviewCount { get; set; } = 124;
    public int BaseFeeBDT { get; set; } = 500;
    public bool OffersSubsidizedOrFree { get; set; } = true;
    public string AvatarSeed { get; set; } = "therapist1";
    public string BioEn { get; set; } = string.Empty;
    public string BioBn { get; set; } = string.Empty;
    public List<string> SpecializationsEn { get; set; } = new();
    public List<string> SpecializationsBn { get; set; } = new();
    public List<string> LanguagesEn { get; set; } = new();
    public List<string> LanguagesBn { get; set; } = new();
    public List<string> AvailableModesEn { get; set; } = new(); // Online Video, Audio/Chat, In-Person (Dhanmondi, Dhaka)
    public List<string> AvailableModesBn { get; set; } = new();
    public List<TherapySlot> AvailableSlots { get; set; } = new();
}

public class TherapySlot
{
    public int Id { get; set; }
    public string DayEn { get; set; } = "Today";
    public string DayBn { get; set; } = "আজ";
    public string TimeEn { get; set; } = "04:00 PM - 05:00 PM";
    public string TimeBn { get; set; } = "বিকাল ৪:০০ - ৫:০০";
    public string DateFormatted { get; set; } = "2026-08-29";
    public bool IsAvailable { get; set; } = true;
    public string Type { get; set; } = "Video / Confidential Audio";
    public string TypeBn { get; set; } = "ভিডিও / গোপনীয় অডিও";
}

public class TherapyDirectoryViewModel
{
    public List<TherapistViewModel> Therapists { get; set; } = new();
    public List<string> AllSpecializationsEn { get; set; } = new();
    public List<string> AllSpecializationsBn { get; set; } = new();
    public string SelectedSpecialty { get; set; } = "All";
    public string SelectedMode { get; set; } = "All";
    public int VerifiedCount => Therapists.Count(t => t.IsBMDCVerified);
}
