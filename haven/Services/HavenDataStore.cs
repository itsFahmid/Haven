using Haven.Models;

namespace Haven.Services;

public static class HavenDataStore
{
    public static List<EmergencyContact> GetEmergencyHotlines()
    {
        return new List<EmergencyContact>
        {
            new()
            {
                Number = "999",
                NameEn = "National Emergency Service",
                NameBn = "জাতীয় জরুরি সেবা",
                DescriptionEn = "Instant dispatch for Police, Ambulance, and Fire Service across Bangladesh.",
                DescriptionBn = "সমগ্র বাংলাদেশে পুলিশ, অ্যাম্বুলেন্স এবং ফায়ার সার্ভিসের তাৎক্ষণিক সহায়তা।",
                TagEn = "24/7 Police & Medical",
                TagBn = "২৪/৭ পুলিশ ও চিকিৎসা",
                Icon = "shield-alert",
                IsTollFree = true,
                ColorScheme = "rose"
            },
            new()
            {
                Number = "1098",
                NameEn = "Child Helpline Bangladesh",
                NameBn = "চাইল্ড হেল্পলাইন বাংলাদেশ",
                DescriptionEn = "Ministry of Women & Children Affairs 24/7 toll-free helpline for child abuse, protection & emergency safety.",
                DescriptionBn = "মহিলা ও শিশু বিষয়ক মন্ত্রণালয়ের শিশু নির্যাতন প্রতিরোধ ও জরুরি সুরক্ষায় ২৪ ঘণ্টার টোল-ফ্রি হেল্পলাইন।",
                TagEn = "Child Protection & Safety",
                TagBn = "শিশু সুরক্ষা ও প্রতিরোধ",
                Icon = "baby",
                IsTollFree = true,
                ColorScheme = "emerald"
            },
            new()
            {
                Number = "109",
                NameEn = "Violence Against Women & Children Helpline",
                NameBn = "নারী ও শিশু নির্যাতন প্রতিরোধ সেল",
                DescriptionEn = "Immediate crisis rescue, legal guidance, and shelter assistance for female and young victims.",
                DescriptionBn = "নির্যাতনের শিকার নারী ও শিশুদের তাৎক্ষণিক আইনি পরামর্শ, উদ্ধার ও আশ্রয় সহায়তা।",
                TagEn = "Women & Youth Crisis",
                TagBn = "নারী ও কিশোরী সুরক্ষা",
                Icon = "heart-handshake",
                IsTollFree = true,
                ColorScheme = "amber"
            },
            new()
            {
                Number = "01779554391",
                NameEn = "Kaan Pete Roi (Emotional Helpline)",
                NameBn = "কান পেতে রই (মানসিক স্বাস্থ্য হেল্পলাইন)",
                DescriptionEn = "First emotional support and suicide prevention helpline in Bangladesh. Completely non-judgmental & confidential.",
                DescriptionBn = "বাংলাদেশের প্রথম মানসিক স্বাস্থ্য ও আত্মহত্যা প্রতিরোধ সহায়তা কেন্দ্র। সম্পূর্ণ গোপনীয় ও বিচারহীন।",
                TagEn = "Mental Health & Suicide Prevention",
                TagBn = "মানসিক স্বাস্থ্য ও সহায়তা",
                Icon = "sparkles",
                IsTollFree = false,
                ColorScheme = "sky"
            },
            new()
            {
                Number = "106",
                NameEn = "Anti-Corruption & Harassment Hotline",
                NameBn = "দুদক হটলাইন ও অভিযোগ সেল",
                DescriptionEn = "Direct reporting for extortion, institutional harassment, and abuse of power.",
                DescriptionBn = "প্রতিষ্ঠানিক হয়রানি, চাঁদাবাজি ও ক্ষমতার অপব্যবহারের বিরুদ্ধে সরাসরি রিপোর্ট।",
                TagEn = "Harassment Reporting",
                TagBn = "হয়রানি প্রতিরোধ",
                Icon = "alert-triangle",
                IsTollFree = true,
                ColorScheme = "indigo"
            }
        };
    }

    public static List<CourseViewModel> GetCourses()
    {
        return new List<CourseViewModel>
        {
            new()
            {
                Id = 1,
                TitleEn = "Cyberbullying & Digital Footprint Defense",
                TitleBn = "সাইবার বুলিং ও ডিজিটাল নিরাপত্তা প্রতিরক্ষা",
                DescriptionEn = "Master evidence collection, screenshot forensics, dealing with leaked photos/blackmail, and legal filing under Bangladesh Cyber Security laws.",
                DescriptionBn = "ডিজিটাল ব্ল্যাকমেইল, আপত্তিকর ছবি ফাঁস, প্রমাণ সংগ্রহ এবং বাংলাদেশের সাইবার নিরাপত্তা আইনে প্রতিকার পাওয়ার কার্যকরী গাইড।",
                CategoryEn = "Cyber Safety",
                CategoryBn = "সাইবার নিরাপত্তা",
                TargetGen = "Gen Z & Alpha (10-24y)",
                TargetGenBn = "জেন জি ও আলফা (১০-২৪ বছর)",
                Duration = "40 mins",
                DurationBn = "৪০ মিনিট",
                ModuleCount = 4,
                CompletedModules = 2,
                IsFree = true,
                IsPayWhatYouWant = false,
                Rating = 4.98,
                EnrolledCount = 4820,
                BadgeEn = "Crucial Life-Skill",
                BadgeBn = "জরুরি দক্ষতা",
                ImageSvgKey = "cyber",
                AccentColor = "teal",
                KeyLearningsEn = new()
                {
                    "How to preserve legally admissible digital evidence",
                    "Immediate takedown requests via BTRC and Cyber Police Unit",
                    "Securing Facebook, WhatsApp & Instagram accounts with 2FA"
                },
                KeyLearningsBn = new()
                {
                    "আইনগতভাবে গ্রহণযোগ্য ডিজিটাল প্রমাণাদি সংরক্ষণের নিয়মাবলী",
                    "বিটিআরসি ও সাইবার পুলিশ ইউনিটের মাধ্যমে তাৎক্ষণিক কন্টেন্ট রিমুভাল",
                    "টু-ফ্যাক্টর অথেনটিকেশনের মাধ্যমে ফেসবুক ও হোয়াটসঅ্যাপ সুরক্ষা"
                },
                Modules = new()
                {
                    new() { StepNumber = 1, TitleEn = "Recognizing Digital Exploitation & Blackmail Red Flags", TitleBn = "ডিজিটাল ব্ল্যাকমেইল ও হয়রানির প্রাথমিক লক্ষণ শনাক্তকরণ", Duration = "8m", DurationBn = "৮ মিনিট", IsCompleted = true, Type = "Interactive Lesson", TypeBn = "ইন্টারেক্টিভ লেসন" },
                    new() { StepNumber = 2, TitleEn = "Emergency Evidence Capture Without Alerting the Offender", TitleBn = "অপরাধীকে সতর্ক না করে ডিজিটাল প্রমাণ সংগ্রহের কৌশল", Duration = "12m", DurationBn = "১২ মিনিট", IsCompleted = true, Type = "Practical Walkthrough", TypeBn = "ব্যবহারিক নির্দেশিকা" },
                    new() { StepNumber = 3, TitleEn = "Connecting to Cyber Police Help Desk & Filing a GD", TitleBn = "সাইবার পুলিশ হেল্পডেস্কে যোগাযোগ ও অনলাইন জিডি করার নিয়ম", Duration = "10m", DurationBn = "১০ মিনিট", IsCompleted = false, Type = "Legal Guide", TypeBn = "আইনি নির্দেশিকা" },
                    new() { StepNumber = 4, TitleEn = "Reclaiming Your Digital Peace: Psychological Recovery", TitleBn = "মানসিক শান্তি পুনরুদ্ধার ও ট্রমা কাটিয়ে ওঠার উপায়", Duration = "10m", DurationBn = "১০ মিনিট", IsCompleted = false, Type = "Recovery Quiz", TypeBn = "মানসিক প্রশান্তি কুইজ" }
                }
            },
            new()
            {
                Id = 2,
                TitleEn = "Personal Boundaries, Consent & Body Autonomy",
                TitleBn = "ব্যক্তিগত সীমানা, সম্মতি ও শারীরিক সুরক্ষার পাঠ",
                DescriptionEn = "Age-appropriate safety guidance on identifying 'Safe Touch vs. Unsafe Touch', saying 'NO' without guilt, and reporting inappropriate touch by relatives or peers.",
                DescriptionBn = "নিরাপদ ও অনিরাপদ স্পর্শ চেনা, অপরাধবোধ ছাড়া স্পষ্ট 'না' বলা এবং পরিচিত বা অপরিচিতদের অযাচিত আচরণে প্রতিরোধ গড়ার উপায়।",
                CategoryEn = "Personal Safety",
                CategoryBn = "ব্যক্তিগত সুরক্ষা",
                TargetGen = "Gen Alpha & Beta (6-16y)",
                TargetGenBn = "জেন আলফা ও বেটা (৬-১৬ বছর)",
                Duration = "35 mins",
                DurationBn = "৩৫ মিনিট",
                ModuleCount = 3,
                CompletedModules = 1,
                IsFree = true,
                IsPayWhatYouWant = false,
                Rating = 4.95,
                EnrolledCount = 3190,
                BadgeEn = "Child & Teen Safety",
                BadgeBn = "কিশোর ও শিশু সুরক্ষা",
                ImageSvgKey = "shield-check",
                AccentColor = "emerald",
                KeyLearningsEn = new()
                {
                    "The Swimsuit Rule & Bodily Sovereignty",
                    "Deconstructing fear when pressured by authority figures",
                    "Finding your 3 'Trusted Haven Adults' to talk to"
                },
                KeyLearningsBn = new()
                {
                    "শারীরিক সীমানা ও গোপনীয়তা রক্ষার গোল্ডেন রুলস",
                    "পরিচিত বা ক্ষমতাধর ব্যক্তির অনৈতিক চাপ প্রতিরোধের কৌশল",
                    "নিজের ৩ জন 'বিশ্বস্ত অভিভাবক' চিহ্নিত করা"
                },
                Modules = new()
                {
                    new() { StepNumber = 1, TitleEn = "My Body, My Boundaries: The Concept of Consent", TitleBn = "আমার শরীর, আমার অধিকার: শারীরিক সীমানার ধারণা", Duration = "10m", DurationBn = "১০ মিনিট", IsCompleted = true, Type = "Animated Video", TypeBn = "অ্যানিমেটেড ভিডিও" },
                    new() { StepNumber = 2, TitleEn = "Breaking The Secret: Why Bad Secrets Must Be Told", TitleBn = "গোপনীয়তার ভয় ভাঙা: কেন খারাপ গোপন কথা প্রকাশ করতে হয়", Duration = "12m", DurationBn = "১২ মিনিট", IsCompleted = false, Type = "Interactive Scenario", TypeBn = "বাস্তব দৃশ্যপট সমাধান" },
                    new() { StepNumber = 3, TitleEn = "Emergency Action Protocol & Safe Shelter Contact", TitleBn = "জরুরি বিপদ সংকেত ও নিরাপদ আশ্রয়স্থলের তালিকা", Duration = "13m", DurationBn = "১৩ মিনিট", IsCompleted = false, Type = "Checklist Test", TypeBn = "সুরক্ষা চেকলিস্ট" }
                }
            },
            new()
            {
                Id = 3,
                TitleEn = "Emergency Psychological First Aid & Panic De-escalation",
                TitleBn = "মানসিক প্রাথমিক চিকিৎসা ও প্যানিক অ্যাটাক নিয়ন্ত্রণ",
                DescriptionEn = "Scientifically backed 4-7-8 breathing, 5-4-3-2-1 sensory grounding, and immediate de-escalation methods for panic attacks, exam anxiety, and acute depressive spikes.",
                DescriptionBn = "প্যানিক অ্যাটাক, তীব্র হতাশা কিংবা পরীক্ষার অতিরিক্ত ভীতি তাৎক্ষণিক দূর করার বৈজ্ঞানিক পদ্ধতি ও গ্রাউন্ডিং টেকনিক।",
                CategoryEn = "Mental Health",
                CategoryBn = "মানসিক স্বাস্থ্য",
                TargetGen = "Gen Z & Youth (13-28y)",
                TargetGenBn = "জেন জি ও তরুণ সমাজ (১৩-২৮ বছর)",
                Duration = "50 mins",
                DurationBn = "৫০ মিনিট",
                ModuleCount = 4,
                CompletedModules = 0,
                IsFree = false,
                IsPayWhatYouWant = true,
                SuggestedFeeBDT = 150,
                Rating = 4.99,
                EnrolledCount = 5280,
                BadgeEn = "PWYW / Free Subsidy",
                BadgeBn = "ঐচ্ছিক ফি / ফ্রি সহায়তা",
                ImageSvgKey = "heart-pulse",
                AccentColor = "purple",
                KeyLearningsEn = new()
                {
                    "Real-time somatic grounding during panic episodes",
                    "Stopping catastrophic thought spirals",
                    "Supporting a friend in acute emotional crisis safely"
                },
                KeyLearningsBn = new()
                {
                    "প্যানিক বা আতঙ্কের মুহূর্তে তাৎক্ষণিক শরীর শান্ত করার উপায়",
                    "নেতিবাচক ভাবনার ঘূর্ণি থামিয়ে ইতিবাচক চিন্তা ফিরিয়ে আনা",
                    "বিপদে থাকা বন্ধুকে সঠিক মানসিক প্রাথমিক সহায়তা প্রদান"
                },
                Modules = new()
                {
                    new() { StepNumber = 1, TitleEn = "Anatomy of a Panic Attack: What Happens in Your Body", TitleBn = "প্যানিক অ্যাটাকের পেছনের বিজ্ঞান: মস্তিষ্কের প্রতিক্রিয়া", Duration = "10m", DurationBn = "১০ মিনিট", IsCompleted = false, Type = "Video Lesson", TypeBn = "ভিডিও লেসন" },
                    new() { StepNumber = 2, TitleEn = "The 5-4-3-2-1 Sensory Grounding Technique", TitleBn = "৫-৪-৩-২-১ ইন্দ্রিয় নির্ভর মন শান্ত করার কৌশল", Duration = "15m", DurationBn = "১৫ মিনিট", IsCompleted = false, Type = "Guided Exercise", TypeBn = "অনুশীলন গাইড" },
                    new() { StepNumber = 3, TitleEn = "Somatic Reset: 4-7-8 Box Breathing Interactive Tool", TitleBn = "৪-৭-৮ বক্স ব্রিদিং ইন্টারঅ্যাকটিভ টুল ব্যবহার", Duration = "10m", DurationBn = "১০ মিনিট", IsCompleted = false, Type = "Interactive Tool", TypeBn = "ইন্টারেক্টিভ টুল" },
                    new() { StepNumber = 4, TitleEn = "Crisis De-escalation Kit for Friends and Family", TitleBn = "পরিবার ও বন্ধুদের জন্য মানসিক সংকট নিরসন কিট", Duration = "15m", DurationBn = "১৫ মিনিট", IsCompleted = false, Type = "Action Toolkit", TypeBn = "অ্যাকশন টুলকিট" }
                }
            },
            new()
            {
                Id = 4,
                TitleEn = "Parenting Gen Alpha & Beta in the Digital Era",
                TitleBn = "ডিজিটাল যুগে জেন আলফা ও বেটা প্যারেন্টিং গাইড",
                DescriptionEn = "Designed for modern young parents: screen-time moderation, AI-awareness, detecting behavioral changes, and building a stigma-free emotional bridge.",
                DescriptionBn = "তরুণ পিতা-মাতার জন্য: স্ক্রিনটাইম নিয়ন্ত্রণ, এআই জগতের ঝুঁকি, শিশুর মানসিক পরিবর্তন বোঝা এবং দূরত্বহীন বন্ধুসুলভ সম্পর্ক তৈরি।",
                CategoryEn = "Parenting & Guardians",
                CategoryBn = "অভিভাবকত্ব ও গাইডেন্স",
                TargetGen = "Gen Beta & Parents",
                TargetGenBn = "জেন বেটা ও অভিভাবক",
                Duration = "60 mins",
                DurationBn = "৬০ মিনিট",
                ModuleCount = 4,
                CompletedModules = 0,
                IsFree = false,
                IsPayWhatYouWant = true,
                SuggestedFeeBDT = 250,
                Rating = 4.92,
                EnrolledCount = 2140,
                BadgeEn = "For Young Parents",
                BadgeBn = "তরুণ অভিভাবকদের জন্য",
                ImageSvgKey = "users",
                AccentColor = "amber",
                KeyLearningsEn = new()
                {
                    "Managing algorithmic gaming addictions and TikTok dopamine loops",
                    "Empathetic communication instead of authoritarian shaming",
                    "Child protection settings on routers and personal devices"
                },
                KeyLearningsBn = new()
                {
                    "অনলাইন গেম ও অতিরিক্ত স্ক্রিন আসক্তি নিয়ন্ত্রণের সহজ কৌশল",
                    "শাসন ও ভীতি প্রদর্শনের বদলে বন্ধুসুলভ যোগাযোগের উপায়",
                    "বাসার ওয়াইফাই ও ডিভাইসে প্যারেন্টাল সিকিউরিটি ফিল্টার বসানো"
                },
                Modules = new()
                {
                    new() { StepNumber = 1, TitleEn = "Understanding the Neurobiology of Digital Natives", TitleBn = "ডিজিটাল যুগের শিশুদের মানসিক বিকাশ ও মস্তিষ্কের স্বভাব", Duration = "15m", DurationBn = "১৫ মিনিট", IsCompleted = false, Type = "Video Lecture", TypeBn = "ভিডিও লেকচার" },
                    new() { StepNumber = 2, TitleEn = "Replacing Screentime with High-Connection Offline Habits", TitleBn = "স্ক্রিনটাইম কমিয়ে বাস্তব জীবনে আনন্দময় মুহূর্ত তৈরি", Duration = "15m", DurationBn = "১৫ মিনিট", IsCompleted = false, Type = "Practical Roadmap", TypeBn = "বাস্তবায়ন রোডম্যাপ" },
                    new() { StepNumber = 3, TitleEn = "Navigating Dark Web, AI Clones & Cyber Predators", TitleBn = "ডার্ক ওয়েব, এআই ডিপফেক ও অনলাইন ফাঁদ থেকে সন্তানকে রক্ষা", Duration = "15m", DurationBn = "১৫ মিনিট", IsCompleted = false, Type = "Case Studies", TypeBn = "কেস স্টাডি" },
                    new() { StepNumber = 4, TitleEn = "Building Open Disclosure at Home: No-Shame Family Policy", TitleBn = "পরিবারে খোলামেলা আলোচনার নিরাপদ পরিবেশ সৃষ্টি", Duration = "15m", DurationBn = "১৫ মিনিট", IsCompleted = false, Type = "Family Contract", TypeBn = "পারিবারিক চুক্তিপত্র" }
                }
            }
        };
    }

    public static List<TherapistViewModel> GetTherapists()
    {
        return new List<TherapistViewModel>
        {
            new()
            {
                Id = 1,
                NameEn = "Dr. Samira Tasneem, MBBS, MPhil (Psychiatry)",
                NameBn = "ডাঃ সামিরা তাসনীম, এমবিবিএস, এমফিল (সাইকিয়াট্রি)",
                TitleEn = "Consultant Psychiatrist & Adolescent Mental Health Specialist",
                TitleBn = "কনসালট্যান্ট সাইকিয়াট্রিস্ট ও কিশোর-কিশোরী মানসিক স্বাস্থ্য বিশেষজ্ঞ",
                RegistrationNo = "BMDC Reg: A-74291",
                IsBMDCVerified = true,
                DegreeEn = "MBBS (Dhaka Medical College), MPhil (National Institute of Mental Health)",
                DegreeBn = "এমবিবিএস (ঢাকা মেডিকেল কলেজ), এমফিল (জাতীয় মানসিক স্বাস্থ্য ইনস্টিটিউট)",
                InstitutionEn = "NIMH & Bangabandhu Sheikh Mujib Medical University (BSMMU)",
                InstitutionBn = "জাতীয় মানসিক স্বাস্থ্য ইনস্টিটিউট ও বিএসএমএমইউ",
                ExperienceYears = 8,
                Rating = 4.98,
                ReviewCount = 186,
                BaseFeeBDT = 600,
                OffersSubsidizedOrFree = true,
                AvatarSeed = "dr_samira",
                BioEn = "Passionate about Gen Z and Alpha mental health. Specializes in clinical depression, exam trauma, gender dysphoria, and crisis stabilization with zero stigma.",
                BioBn = "তরুণ ও কিশোরদের মানসিক স্বাস্থ্য সুরক্ষায় নিবেদিত। ডিপ্রেশন, উদ্বেগ, জেন্ডার আইডেন্টিটি এবং ক্রাইসিস কাউন্সেলিংয়ে আন্তর্জাতিক মানসম্মত সেবা প্রদান করেন।",
                SpecializationsEn = new() { "Adolescent Trauma", "Clinical Depression", "Cyber Harassment Recovery", "LGBTQ+ Affirmative" },
                SpecializationsBn = new() { "কিশোর ট্রমা", "ক্লিনিক্যাল ডিপ্রেশন", "সাইবার হয়রানি পরবর্তী মানসিক চিকিৎসা", "জেন্ডার সহমর্মী" },
                LanguagesEn = new() { "Bangla (Native)", "English (Fluent)" },
                LanguagesBn = new() { "বাংলা (মাতৃভাষা)", "ইংরেজি" },
                AvailableModesEn = new() { "Encrypted Video Call", "Confidential Voice Call", "In-Person (Dhanmondi, Dhaka)" },
                AvailableModesBn = new() { "এনক্রিপ্টেড ভিডিও কল", "গোপনীয় অডিও কল", "সরাসরি চেম্বার (ধানমন্ডি, ঢাকা)" },
                AvailableSlots = new()
                {
                    new() { Id = 101, DayEn = "Today", DayBn = "আজ", TimeEn = "04:30 PM - 05:30 PM", TimeBn = "বিকাল ৪:৩০ - ৫:৩০", DateFormatted = "2026-08-28", IsAvailable = true },
                    new() { Id = 102, DayEn = "Tonight", DayBn = "আজ রাত", TimeEn = "09:00 PM - 10:00 PM", TimeBn = "রাত ৯:০০ - ১০:০০", DateFormatted = "2026-08-28", IsAvailable = true },
                    new() { Id = 103, DayEn = "Tomorrow", DayBn = "আগামীকাল", TimeEn = "06:00 PM - 07:00 PM", TimeBn = "সন্ধ্যা ৬:০০ - ৭:০০", DateFormatted = "2026-08-29", IsAvailable = true }
                }
            },
            new()
            {
                Id = 2,
                NameEn = "Fariha Chowdhury, MS, M.Phil (Clinical Psychology)",
                NameBn = "ফারিহা চৌধুরী, এমএস, এম.ফিল (ক্লিনিক্যাল সাইকোলজি)",
                TitleEn = "Senior Clinical Psychologist & Child Trauma Therapist",
                TitleBn = "সিনিয়র ক্লিনিক্যাল সাইকোলজিস্ট ও শিশু ট্রমা থেরাপিস্ট",
                RegistrationNo = "BCPS Reg: CP-1082 (Verified)",
                IsBMDCVerified = true,
                DegreeEn = "B.Sc & M.S in Clinical Psychology (University of Dhaka)",
                DegreeBn = "বিএসসি ও এমএস (ক্লিনিক্যাল সাইকোলজি, ঢাকা বিশ্ববিদ্যালয়)",
                InstitutionEn = "University of Dhaka & Haven Crisis Center",
                InstitutionBn = "ঢাকা বিশ্ববিদ্যালয় ও হেভেন ক্রাইসিস সেন্টার",
                ExperienceYears = 7,
                Rating = 4.96,
                ReviewCount = 142,
                BaseFeeBDT = 500,
                OffersSubsidizedOrFree = true,
                AvatarSeed = "fariha_c",
                BioEn = "Dedicated to child abuse recovery, family mediation, and cognitive behavioral therapy (CBT). Conducts safe-space workshops across universities.",
                BioBn = "শিশু ও তরুণদের শারীরিক ও মানসিক আঘাত নিরাময়, পারিবারিক বিরোধ নিরসন এবং সিবিটি থেরাপিতে অভিজ্ঞ ও সহানুভূতিশীল কাউন্সিলর।",
                SpecializationsEn = new() { "Child Sexual Abuse Recovery", "Panic & OCD", "Family Mediation", "Academic Burnout" },
                SpecializationsBn = new() { "শিশু যৌন নির্যাতন পরবর্তী নিরাময়", "প্যানিক ও ওসিডি", "পারিবারিক কাউন্সিলিং", "পড়াশোনার চাপ ও বার্নআউট" },
                LanguagesEn = new() { "Bangla", "English", "Sylheti" },
                LanguagesBn = new() { "বাংলা", "ইংরেজি", "সিলেটি" },
                AvailableModesEn = new() { "Encrypted Video Call", "Private Chat Session", "In-Person (Uttara, Dhaka)" },
                AvailableModesBn = new() { "এনক্রিপ্টেড ভিডিও কল", "প্রাইভেট চ্যাট সেশন", "সরাসরি চেম্বার (উত্তরা, ঢাকা)" },
                AvailableSlots = new()
                {
                    new() { Id = 201, DayEn = "Tomorrow", DayBn = "আগামীকাল", TimeEn = "11:00 AM - 12:00 PM", TimeBn = "সকাল ১১:০০ - ১২:০০", DateFormatted = "2026-08-29", IsAvailable = true },
                    new() { Id = 202, DayEn = "Tomorrow", DayBn = "আগামীকাল", TimeEn = "04:00 PM - 05:00 PM", TimeBn = "বিকাল ৪:০০ - ৫:০০", DateFormatted = "2026-08-29", IsAvailable = true },
                    new() { Id = 203, DayEn = "Sunday", DayBn = "রবিবার", TimeEn = "07:30 PM - 08:30 PM", TimeBn = "সন্ধ্যা ৭:৩০ - ৮:৩০", DateFormatted = "2026-08-30", IsAvailable = true }
                }
            },
            new()
            {
                Id = 3,
                NameEn = "Dr. Tanvir Rahman, MBBS, MD (Psychiatry)",
                NameBn = "ডাঃ তানভীর রহমান, এমবিবিএস, এমডি (সাইকিয়াট্রি)",
                TitleEn = "Associate Professor & Addiction Recovery Specialist",
                TitleBn = "সহযোগী অধ্যাপক ও আসক্তি নিরাময় বিশেষজ্ঞ",
                RegistrationNo = "BMDC Reg: A-61840",
                IsBMDCVerified = true,
                DegreeEn = "MBBS (Chittagong Medical College), MD (BSMMU)",
                DegreeBn = "এমবিবিএস (চট্টগ্রাম মেডিকেল কলেজ), এমডি (বিএসএমএমইউ)",
                InstitutionEn = "Chittagong Medical College Hospital",
                InstitutionBn = "চট্টগ্রাম মেডিকেল কলেজ হাসপাতাল",
                ExperienceYears = 11,
                Rating = 4.93,
                ReviewCount = 210,
                BaseFeeBDT = 700,
                OffersSubsidizedOrFree = true,
                AvatarSeed = "dr_tanvir",
                BioEn = "Specialized in substance abuse recovery, gaming & pornography addiction in teens, and suicide prevention. Employs compassionate motivational interviewing.",
                BioBn = "তরুণদের মাদক, গেমিং ও পর্নোগ্রাফি আসক্তি থেকে মুক্তি, আত্মহত্যার প্রবণতা দূরীকরণ এবং দীর্ঘমেয়াদী সুস্থতায় অভিজ্ঞ চিকিৎসক।",
                SpecializationsEn = new() { "Substance & Screen Addiction", "Suicide Prevention", "Bipolar Disorder", "Anger Management" },
                SpecializationsBn = new() { "মাদক ও স্ক্রিন আসক্তি মুক্তি", "আত্মহত্যা প্রতিরোধ", "বাইপোলার ডিসঅর্ডার", "রাগ নিয়ন্ত্রণ" },
                LanguagesEn = new() { "Bangla", "English", "Chittagonian" },
                LanguagesBn = new() { "বাংলা", "ইংরেজি", "চাঁটগাঁইয়া" },
                AvailableModesEn = new() { "Encrypted Video Call", "In-Person (GEC Circle, Chittagong)" },
                AvailableModesBn = new() { "এনক্রিপ্টেড ভিডিও কল", "সরাসরি চেম্বার (জিইসি মোড়, চট্টগ্রাম)" },
                AvailableSlots = new()
                {
                    new() { Id = 301, DayEn = "Today", DayBn = "আজ", TimeEn = "08:00 PM - 09:00 PM", TimeBn = "রাত ৮:০০ - ৯:০০", DateFormatted = "2026-08-28", IsAvailable = true },
                    new() { Id = 302, DayEn = "Saturday", DayBn = "শনিবার", TimeEn = "05:00 PM - 06:00 PM", TimeBn = "বিকাল ৫:০০ - ৬:০০", DateFormatted = "2026-08-29", IsAvailable = true }
                }
            }
        };
    }

    public static List<HallOfFameDonor> GetRecentDonors()
    {
        return new List<HallOfFameDonor>
        {
            new() { Name = "Anonymous Champion", AmountBDT = 5000, BadgeEn = "Guardian Angel", BadgeBn = "অভিভাবক দূত", TimeAgoEn = "1 hour ago", TimeAgoBn = "১ ঘণ্টা আগে", City = "Gulshan, Dhaka" },
            new() { Name = "Tahmidul Islam", AmountBDT = 1000, BadgeEn = "Youth Protector", BadgeBn = "তরুণদের রক্ষক", TimeAgoEn = "3 hours ago", TimeAgoBn = "৩ ঘণ্টা আগে", City = "Chittagong" },
            new() { Name = "Nusrat & Friends", AmountBDT = 2500, BadgeEn = "Haven Sustainer", BadgeBn = "হেভেন সহযোগী", TimeAgoEn = "5 hours ago", TimeAgoBn = "৫ ঘণ্টা আগে", City = "Sylhet" },
            new() { Name = "Anonymous Student", AmountBDT = 100, BadgeEn = "Micro Hero", BadgeBn = "মাইক্রো হিরো", TimeAgoEn = "8 hours ago", TimeAgoBn = "৮ ঘণ্টা আগে", City = "Rajshahi" },
            new() { Name = "Dr. Kabir Chowdhury", AmountBDT = 10000, BadgeEn = "Crisis Benefactor", BadgeBn = "ক্রাইসিস দাতা", TimeAgoEn = "Yesterday", TimeAgoBn = "গতকাল", City = "Dhaka" }
        };
    }

    public static List<QuickHelpPrompt> GetQuickPrompts()
    {
        return new List<QuickHelpPrompt>
        {
            new()
            {
                PromptEn = "Someone is threatening to leak my private photos on social media.",
                PromptBn = "কেউ একজন ফেসবুকে আমার গোপন ছবি ফাঁসের হুমকি দিচ্ছে। আমি কী করব?",
                CategoryEn = "Cyber Extortion",
                CategoryBn = "সাইবার ব্ল্যাকমেইল",
                Icon = "shield-alert"
            },
            new()
            {
                PromptEn = "I am having overwhelming panic attacks and feeling hopeless.",
                PromptBn = "আমার প্রচণ্ড প্যানিক অ্যাটাক হচ্ছে এবং খুব একা ও অসহায় লাগছে।",
                CategoryEn = "Panic & Anxiety",
                CategoryBn = "প্যানিক ও মানসিক চাপ",
                Icon = "heart-pulse"
            },
            new()
            {
                PromptEn = "How do I report domestic abuse or violence anonymously in Bangladesh?",
                PromptBn = "পরিবারে বা আশেপাশে নির্যাতন হলে কীভাবে পরিচয় গোপন রেখে অভিযোগ করব?",
                CategoryEn = "Abuse Reporting",
                CategoryBn = "নির্যাতন অভিযোগ",
                Icon = "eye-off"
            },
            new()
            {
                PromptEn = "I need guidance on booking a free/subsidized confidential therapy session.",
                PromptBn = "আমি একজন ভেরিফায়েড থেরাপিস্টের সাথে কম খরচে বা ফ্রিতে কথা বলতে চাই।",
                CategoryEn = "Therapy Guidance",
                CategoryBn = "কাউন্সেলিং সহায়তা",
                Icon = "user-check"
            }
        };
    }
}
