using UnityEngine;

public static class ApiConfig
{
    // Base URLs (centralized)
    public static readonly string BaseMainUrl = "https://lssctc.azurewebsites.net/api/";
    public static readonly string BaseSimulationUrl = "https://lssctc-simulation.azurewebsites.net/api/";

    // Endpoints (exact same routes you used)
    public static string MyClasses(int userId) => $"{BaseMainUrl}Classes/myclasses/{userId}";
    public static string TraineePractices(int userId, int classId) => $"{BaseSimulationUrl}TraineePractices/trainee/{userId}/class/{classId}";
    public static string CreatePracticeAttempt(int sectionPracticeId, int userId) => $"{BaseSimulationUrl}PracticeAttempts/section-practice/{sectionPracticeId}/trainee/{userId}";
    public static string PracticeAttemptSteps(int attemptId) => $"{BaseSimulationUrl}TraineePractices/attempt/{attemptId}/steps";
    public static string CompleteAttempt(int attemptId) => $"{BaseSimulationUrl}PracticeAttempts/attempt/{attemptId}/complete";
    public static string SubmitStep(int attemptId, int userId) => $"{BaseSimulationUrl}TraineePractices/attempt/{attemptId}/trainee/{userId}/submit";
    public static string ComponentById(int id) => $"{BaseSimulationUrl}Components/{id}";
}
