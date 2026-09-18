namespace Obhoy.Models;

public class CourseViewModel
{
    public int Id { get; set; }
    public int? AuthorId { get; set; }
    public string AuthorName { get; set; } = "Obhoy Clinical Team";
    public string TitleEn { get; set; } = string.Empty;
    public string TitleBn { get; set; } = string.Empty;
    public string DescriptionEn { get; set; } = string.Empty;
    public string DescriptionBn { get; set; } = string.Empty;
    public string CategoryEn { get; set; } = string.Empty;
    public string CategoryBn { get; set; } = string.Empty;
    
    // Mandatory Age Range Tag
    public string TargetGen { get; set; } = "Gen Z & Alpha (10-24y)";
    public string TargetGenBn { get; set; } = "à¦œà§‡à¦¨ à¦œà¦¿ à¦“ à¦†à¦²à¦«à¦¾ (à§§à§¦-à§¨à§ª à¦¬à¦›à¦°)";

    // Mandatory Course Language Tag
    public string Language { get; set; } = "Bangla"; // Bangla, English, Bilingual
    public string LanguageBn => Language switch
    {
        "Bangla" => "à¦¬à¦¾à¦‚à¦²à¦¾",
        "English" => "English",
        "Bilingual" => "à¦¦à§à¦¬à¦¿à¦­à¦¾à¦·à¦¿à¦• (à¦¬à¦¾à¦‚à¦²à¦¾ à¦“ English)",
        _ => "à¦¬à¦¾à¦‚à¦²à¦¾"
    };

    public string Duration { get; set; } = "45 mins";
    public string DurationBn { get; set; } = "à§ªà§« à¦®à¦¿à¦¨à¦¿à¦Ÿ";
    public int ModuleCount { get; set; } = 4;
    public int CompletedModules { get; set; } = 0;
    public int ProgressPercentage => ModuleCount > 0 ? (int)((double)CompletedModules / ModuleCount * 100) : 0;
    public bool IsFree { get; set; } = true;
    public bool IsPayWhatYouWant { get; set; } = false;
    public decimal SuggestedFeeBDT { get; set; } = 0;
    public double Rating { get; set; } = 4.9; // Capped at 5.0
    public int EnrolledCount { get; set; } = 1420;
    public string ApprovalStatus { get; set; } = "Approved"; // Pending, Approved, Rejected
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string BadgeEn { get; set; } = "Essential";
    public string BadgeBn { get; set; } = "à¦…à¦ªà¦°à¦¿à¦¹à¦¾à¦°à§à¦¯";
    public string ImageSvgKey { get; set; } = "shield";
    public string AccentColor { get; set; } = "teal";

    public List<string> Tags { get; set; } = new();
    public List<CourseModuleItem> Modules { get; set; } = new();
    public List<ReviewItemViewModel> Reviews { get; set; } = new();
    public List<string> KeyLearningsEn { get; set; } = new();
    public List<string> KeyLearningsBn { get; set; } = new();

    public bool IsUserEnrolled { get; set; }
    public int? UserRating { get; set; }
    public string? UserReviewComment { get; set; }
}

public class CourseModuleItem
{
    public int Id { get; set; }
    public int StepNumber { get; set; }
    public string TitleEn { get; set; } = string.Empty;
    public string TitleBn { get; set; } = string.Empty;
    public string ShortDescriptionBn { get; set; } = string.Empty;
    public string ShortDescriptionEn { get; set; } = string.Empty;
    public string Duration { get; set; } = "10m";
    public string DurationBn { get; set; } = "à§§à§¦ à¦®à¦¿à¦¨à¦¿à¦Ÿ";
    public bool IsCompleted { get; set; }
    public string Type { get; set; } = "Interactive Lesson";
    public string TypeEn { get; set; } = "Interactive Lesson";
    public string TypeBn { get; set; } = "à¦‡à¦¨à§à¦Ÿà¦¾à¦°à§‡à¦•à§à¦Ÿà¦¿à¦­ à¦ªà¦¾à¦ ";
    public string ContentMarkdown { get; set; } = string.Empty;
    public string OptionalMaterials { get; set; } = string.Empty; // Resource links / downloadable files
}

public class ReviewItemViewModel
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserAvatar { get; set; } = string.Empty;
    public int Rating { get; set; } // 1 to 5
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CoursesHubViewModel
{
    public string SelectedCategory { get; set; } = "All";
    public string SelectedAgeGroup { get; set; } = "All";
    public string SearchQuery { get; set; } = string.Empty;

    public List<CourseViewModel> Courses { get; set; } = new();

    public List<string> CategoriesEn { get; set; } = new();
    public List<string> CategoriesBn { get; set; } = new();
    public List<string> AgeGroups { get; set; } = new();

    public int TotalLearnersCount { get; set; } = 18450;
    public int CertificatesIssued { get; set; } = 9230;

    // Pagination System
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 6;
    public int TotalCoursesCount { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCoursesCount / PageSize);
    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;
}

public class CreateCourseViewModel
{
    public string TitleEn { get; set; } = string.Empty;
    public string TitleBn { get; set; } = string.Empty;
    public string CategoryEn { get; set; } = "Cyber Safety";
    public string CategoryBn { get; set; } = "à¦¸à¦¾à¦‡à¦¬à¦¾à¦° à¦¨à¦¿à¦°à¦¾à¦ªà¦¤à§à¦¤à¦¾";
    public string TargetGen { get; set; } = "Gen Z & Alpha (10-24y)";
    
    // Mandatory Language Selection Tag
    public string Language { get; set; } = "Bangla"; // Bangla, English, Bilingual

    public string DescriptionEn { get; set; } = string.Empty;
    public string DescriptionBn { get; set; } = string.Empty;
    public string Duration { get; set; } = "45 mins";
    public bool IsFree { get; set; } = true;
    public bool IsPayWhatYouWant { get; set; } = false;
    public decimal SuggestedFeeBDT { get; set; } = 0;
    public string TagsCsv { get; set; } = "Cyber Safety, Privacy, Digital Defense";

    // Modules with short description & optional materials
    public List<CreateModuleViewModel> Modules { get; set; } = new()
    {
        new() 
        { 
            StepNumber = 1, 
            TitleEn = "Introduction & Safety Fundamentals", 
            TitleBn = "à¦¸à§‚à¦šà¦¨à¦¾ à¦“ à¦¸à§à¦°à¦•à§à¦·à¦¾ à¦®à§‚à¦²à¦¨à§€à¦¤à¦¿", 
            ShortDescriptionBn = "à¦¸à¦¾à¦‡à¦¬à¦¾à¦° à¦¨à¦¿à¦°à¦¾à¦ªà¦¤à§à¦¤à¦¾ à¦“ à¦ªà§à¦°à¦®à¦¾à¦£ à¦¸à¦‚à¦°à¦•à§à¦·à¦£à§‡à¦° à¦ªà§à¦°à¦¾à¦¥à¦®à¦¿à¦• à¦§à¦¾à¦°à¦£à¦¾à¥¤",
            Duration = "10 mins", 
            ContentMarkdown = "Safety fundamentals overview and core rules.",
            OptionalMaterials = "https://obhoy.org/resources/cyber-safety-guide.pdf"
        },
        new() 
        { 
            StepNumber = 2, 
            TitleEn = "Practical Defense & Evidence Preservation", 
            TitleBn = "à¦ªà§à¦°à§à¦¯à¦¾à¦•à¦Ÿà¦¿à¦•à§à¦¯à¦¾à¦² à¦ªà§à¦°à¦¤à¦¿à¦°à¦•à§à¦·à¦¾ à¦“ à¦ªà§à¦°à¦®à¦¾à¦£ à¦¸à¦‚à¦°à¦•à§à¦·à¦£", 
            ShortDescriptionBn = "à¦¡à¦¿à¦œà¦¿à¦Ÿà¦¾à¦² à¦ªà§à¦°à¦®à¦¾à¦£ à¦¸à¦‚à¦—à§à¦°à¦¹ à¦“ à¦†à¦‡à¦¨à¦¿ à¦…à¦­à¦¿à¦¯à§‹à¦— à¦¦à¦¾à§Ÿà§‡à¦°à§‡à¦° à¦§à¦¾à¦ªà¥¤",
            Duration = "15 mins", 
            ContentMarkdown = "Step-by-step action plan for preserving screenshots and links.",
            OptionalMaterials = "https://obhoy.org/resources/evidence-checklist.pdf"
        }
    };
}

public class CreateModuleViewModel
{
    public int StepNumber { get; set; }
    public string TitleEn { get; set; } = string.Empty;
    public string TitleBn { get; set; } = string.Empty;
    public string ShortDescriptionBn { get; set; } = string.Empty;
    public string ShortDescriptionEn { get; set; } = string.Empty;
    public string Duration { get; set; } = "10 mins";
    public string ContentMarkdown { get; set; } = string.Empty;
    public string OptionalMaterials { get; set; } = string.Empty;
}
