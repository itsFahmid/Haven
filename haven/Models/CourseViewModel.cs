namespace Haven.Models;

public class CourseViewModel
{
    public int Id { get; set; }
    public string TitleEn { get; set; } = string.Empty;
    public string TitleBn { get; set; } = string.Empty;
    public string DescriptionEn { get; set; } = string.Empty;
    public string DescriptionBn { get; set; } = string.Empty;
    public string CategoryEn { get; set; } = string.Empty;
    public string CategoryBn { get; set; } = string.Empty;
    public string TargetGen { get; set; } = "Gen Z & Alpha"; // Gen Z, Gen Alpha, Gen Beta & Parents
    public string TargetGenBn { get; set; } = "জেন জি ও আলফা";
    public string Duration { get; set; } = "45 mins";
    public string DurationBn { get; set; } = "৪৫ মিনিট";
    public int ModuleCount { get; set; } = 4;
    public int CompletedModules { get; set; } = 1;
    public int ProgressPercentage => ModuleCount > 0 ? (int)((double)CompletedModules / ModuleCount * 100) : 0;
    public bool IsFree { get; set; } = true;
    public bool IsPayWhatYouWant { get; set; } = false;
    public int SuggestedFeeBDT { get; set; } = 0;
    public double Rating { get; set; } = 4.9;
    public int EnrolledCount { get; set; } = 1420;
    public string BadgeEn { get; set; } = "Essential";
    public string BadgeBn { get; set; } = "অপরিহার্য";
    public string ImageSvgKey { get; set; } = "shield";
    public string AccentColor { get; set; } = "teal"; // teal, purple, amber, sky, emerald
    public List<CourseModuleItem> Modules { get; set; } = new();
    public List<string> KeyLearningsEn { get; set; } = new();
    public List<string> KeyLearningsBn { get; set; } = new();
}

public class CourseModuleItem
{
    public int StepNumber { get; set; }
    public string TitleEn { get; set; } = string.Empty;
    public string TitleBn { get; set; } = string.Empty;
    public string Duration { get; set; } = "10m";
    public string DurationBn { get; set; } = "১০ মিনিট";
    public bool IsCompleted { get; set; }
    public string Type { get; set; } = "Interactive Lesson"; // Video, Interactive Lesson, Quiz, Action Plan
    public string TypeBn { get; set; } = "ইন্টারেক্টিভ লেসন";
}

public class CoursesHubViewModel
{
    public string SelectedCategory { get; set; } = "All";
    public List<CourseViewModel> Courses { get; set; } = new();
    public List<string> CategoriesEn { get; set; } = new();
    public List<string> CategoriesBn { get; set; } = new();
    public int TotalLearnersCount { get; set; } = 18450;
    public int CertificatesIssued { get; set; } = 9230;
}
