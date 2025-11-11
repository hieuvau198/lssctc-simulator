using UnityEngine;

public static class ApiConfig
{
    // Base URLs (centralized)
    public static readonly string BaseMainUrl = "https://lssctc.azurewebsites.net/api/";
    public static readonly string BaseSimulationUrl = "https://lssctc-simulation.azurewebsites.net/api/";

    // Endpoints (exact same routes you used)
    public static string CreatePracticeAttempt(int sectionPracticeId, int userId) => $"{BaseSimulationUrl}PracticeAttempts/section-practice/{sectionPracticeId}/trainee/{userId}";
    public static string CompleteAttempt(int attemptId) => $"{BaseSimulationUrl}PracticeAttempts/attempt/{attemptId}/complete";
    public static string ComponentById(int id) => $"{BaseMainUrl}SimulationComponents/{id}";
}
