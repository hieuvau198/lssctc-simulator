using UnityEngine;

public static class ApiConfig
{
    // Base URLs (centralized)
    public static readonly string BaseMainUrl = "https://lssctc.azurewebsites.net/api/";
    public static readonly string BaseSimulationUrl = "https://lssctc-simulation.azurewebsites.net/api/";

    // Endpoints (exact same routes you used)
    public static string Login() => $"{BaseMainUrl}Authens/login-username";
    public static string ClassesForUser() => $"{BaseMainUrl}Classes/my-classes";
    public static string PracticesForClass(int classId) => $"{BaseMainUrl}Practices/trainee/class/{classId}";
    public static string PracticeAttemptComplete() => $"{BaseMainUrl}PracticeAttempts/by-code";

    public static string ComponentByCode(string code) => $"{BaseMainUrl}SimulationComponents/by-code/{code}";
    // 1. GET final exam SE practice by classId
    public static string FinalExamSePractices(int classId) => $"{BaseMainUrl}FinalExams/class/{classId}/my-final-se-practices"; 
    // 2. POST check the available Exam code
    public static string FinalExamSeValidateCode(int partialId) => $"{BaseMainUrl}FinalExams/partial/{partialId}/validate-se-code";
    // 3. POST submit the final exam SE
    public static string FinalExamSeSubmit(int partialId) => $"{BaseMainUrl}FinalExams/submit/se-final/{partialId}";
}
