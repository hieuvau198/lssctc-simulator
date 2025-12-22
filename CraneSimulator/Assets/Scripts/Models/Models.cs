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
    public string practiceCode;
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
[System.Serializable]
public class SePracticeListDto 
{
    public int finalExamPartialId; 
    public string finalExamPartialStatus;
    public string startTime; 
    public string endTime;
    public int id;
    public string practiceName;
    public string practiceCode;
    public string practiceDescription;
    public int estimatedDurationMinutes;
    public string difficultyLevel;
    public int maxAttempts;
    public string createdDate;
    public bool isActive;  
    public bool isCompleted;
}
[System.Serializable]
public class ValidateExamCodeDto
{
    public string examCode;
}


[System.Serializable]
public class FinalExamPartialDtoResponse
{
    public int id;
    public string type;
    public decimal marks;
    public string status;
    public string practiceName;
    public string startTime;
}
[Serializable]
public class PracticeAttemptTaskDto
{
    public string taskCode;
    public int score;
    public int mistakes;
    public string description;
    public bool isPass;
}

[Serializable]
public class PracticeAttemptCompleteDto
{
    public int activityRecordId;
    public int classId;
    public string practiceCode;

    public int score;
    public string description;
    public bool isPass;

    public int totalMistakes;

    public string startTime;       
    public string endTime;         
    public int durationSeconds;

    public List<PracticeAttemptTaskDto> practiceAttemptTasks;
}

[System.Serializable]
public class PracticeAttemptCompleteResponse
{
    public int id;
    public int activityRecordId;
    public int practiceId;
    public string practiceCode;
    public int score;
    public string attemptDate;
    public string attemptStatus;
    public string description;
    public bool isPass;
    public bool isCurrent;
    public List<PracticeAttemptTaskResponse> practiceAttemptTasks;
}

[System.Serializable]
public class PracticeAttemptTaskResponse
{
    public int id;
    public int practiceAttemptId;
    public int taskId;
    public string taskCode;
    public string taskName;
    public int score;
    public string description;
    public bool isPass;
}

//Final Exam
[Serializable]
public class SubmitSeTaskDto
{
    public string taskCode;
    public bool isPass;
    public int durationSecond;
    public int mistake;
}

[Serializable]
public class SeTaskDto
{
    public int id;
    public int feSimulationId;
    public string taskCode;
    public string name;
    public string description;
    public bool isPass;
    public string completeTime;
    public int durationSecond;
}

[Serializable]
public class SubmitSeFinalDto
{
    
    public float marks; 
    public bool isPass;
    public string description;
    public string startTime;
    public string completeTime;

    
    public List<SubmitSeTaskDto> tasks;
}
[System.Serializable]
public class FinalExamSubmitDto
{
    public int marks;
    public bool isPass;
    public string description;
    public string completeTime;
}
[System.Serializable]
public class FinalExamResponse
{
    public int id;
    public int enrollmentId;
    public string traineeName;
    public string traineeCode;
    public bool isPass;
    public int totalMarks;
    public string completeTime;
    public string examCode;
    public List<FinalExamPartial> partials;
}
[System.Serializable]
public class FinalExamPartial
{
    public int id;
    public string type;
    public float marks;
    public float examWeight;
    public string description;
    public int duration;
    public string startTime;
    public string endTime;
    public string completeTime;
    public string status;
    public bool isPass;
    public int? quizId;
    public string quizName;
    public int? practiceId;
    public string practiceName;
    public List<FinalExamChecklist> checklists;

    public List<SeTaskDto> tasks;
}
[System.Serializable]
public class FinalExamChecklist
{
}
// ---- Generic ApiResponse if you want to parse message field (optional) ----
[Serializable]
public class ApiResponse
{
    public string message;
}
[Serializable]
public class ErrorMessageDto
{
    public string message;
}
