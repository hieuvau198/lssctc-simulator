using System;
using System.Collections.Generic;
using UnityEngine;

// ---- Practice list responses ----
[Serializable]
public class TraineePracticeResponse
{
    public List<TraineePracticeItem> items;
}

[Serializable]
public class TraineePracticeItem
{
    public int sectionPracticeId;
    public int partitionId;
    public int practiceId;
    public string customDeadline;
    public string status;
    public bool isCompleted;
    public string practiceName;
    public string practiceDescription;
    public int estimatedDurationMinutes;
    public string difficultyLevel;
}

// ---- Class list ----
[Serializable]
public class ClassListResponse
{
    public List<ClassItem> items;
}

[Serializable]
public class ClassItem
{
    public int courseId;
    public string courseName;
    public string courseCode;
    public int durationHours;
    public string imageUrl;
    public int classId;
    public string className;
    public string classCode;
    public int status;
    public int instructorId;
    public string instructorName;
    public string startDate;
    public string endDate;
}

// ---- Practice Attempt response ----
[Serializable]
public class PracticeAttemptResponse
{
    public int practiceAttemptId;
    public int sectionPracticeId;
    public int learningRecordPartitionId;
    public int score;
    public string attemptDate;
    public int attemptStatus;
    public string description;
    public bool isPass;
}

// ---- TraineePracticeStep (exact same fields) ----
[Serializable]
public class TraineePracticeStep
{
    public int stepId;
    public string stepName;
    public string stepDescription;
    public string expectedResult;
    public int stepOrder;
    public bool isCompleted;
    public int practiceId;

    // Optional extras you had
    public string actionName;
    public string actionDescription;
    public string actionKey;
    public string componentName;
    public string componentDescription;
    public string componentImageUrl;
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

// ---- Submit step DTO (keep the same names) ----
[Serializable]
public class SubmitStepData
{
    public int currentStepId;
    public int componentId;
    public string actionKey;
}

// ---- Generic ApiResponse if you want to parse message field (optional) ----
[Serializable]
public class ApiResponse
{
    public string message;
}
