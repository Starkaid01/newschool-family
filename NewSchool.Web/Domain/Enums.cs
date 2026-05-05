namespace NewSchool.Web.Domain;

public enum UserRole
{
    Admin = 1,
    Parent = 2
}

public enum LearningDomain
{
    Language = 1,
    Math = 2,
    World = 3,
    ExecutiveFunction = 4,
    Science = 5,
    History = 6,
    Geography = 7
}

public enum SupportProfile
{
    General = 1,
    TeaLevel1 = 2,
    TeaLevel2 = 3,
    TeaLevel3 = 4
}

public enum CurriculumSupportScope
{
    General = 1,
    TeaCommon = 2,
    TeaLevel1 = 3,
    TeaLevel2 = 4,
    TeaLevel3 = 5
}

public enum FunctionalSupportTrack
{
    Base = 1,
    Communication = 2,
    Regulation = 3,
    Sensory = 4,
    DailyLiving = 5,
    AcademicAdapted = 6
}

public enum SkillFeedbackLevel
{
    NeedsSupport = 1,
    Developing = 2,
    Secure = 3
}

public enum PlanDirectiveType
{
    PinnedActivity = 1,
    TrackFocus = 2
}
