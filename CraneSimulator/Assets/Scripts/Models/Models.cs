using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LoginRequest
{
    public string username;
    public string password;
}

[System.Serializable]
public class LoginResponse
{
    public string userName;
    public string accessToken;
    public int expiresIn;
}

[Serializable]
public class ClassDto
{
    public int id;
    public string name;
    public int capacity;
    public int programId;
    public int courseId;
    public string classCode;
    public string description;
    public string status;
    public string startDate;
    public string endDate;
}

[Serializable]
public class ComponentDto
{
    public int id;
    public string name;
    public string description;
    public string imageUrl;
    public bool isActive;
    public string createdDate;
}
[System.Serializable]
public class PracticeDto
{
    public int id;
    public string practiceName;
    public string practiceDescription;
    public int estimatedDurationMinutes;
    public string difficultyLevel;
    public int maxAttempts;
    public string createdDate;
    public bool isActive;
    public int activityRecordId;
    public int activityId;
    public bool isCompleted;
}
[Serializable]
public class PracticeAttemptTaskDto
{
    public int taskId;
    public int score;
    public bool isPass;
}

[Serializable]
public class PracticeAttemptCompleteDto
{
    public int classId;
    public int practiceId;
    public int score;
    public string description;
    public bool isPass;
    public List<PracticeAttemptTaskDto> practiceAttemptTasks;
}

// ---- Generic ApiResponse if you want to parse message field (optional) ----
[Serializable]
public class ApiResponse
{
    public string message;
}
