# Course Progress Debug & Fix Guide

## ?? V?n ??: Progress bar hi?n th? 133% (7/5)

### Nguyên nhân

Progress bar hi?n th? **7 completed / 5 total** là do:

1. **Duplicate records** trong b?ng `CourseProgress`
2. **Invalid ContentIds** - ContentId không còn t?n t?i trong khóa h?c
3. **Old data** t? l?n test tr??c

## ? Gi?i pháp ?ã implement

### 1. S?a logic ??m progress

**File: `CourseProgressController.cs`**

```csharp
// ? L?y t?t c? ContentIds h?p l? t? LessonContents
var allCourseContentIds = course.CourseChapters
    .SelectMany(ch => ch.Lessons)
    .SelectMany(l => l.LessonContents)
    .Select(c => c.ContentId)
    .Distinct()
    .ToList();

// ? Ch? ??m ContentId ?ã complete VÀ thu?c khóa h?c này
var completedContentIds = await _context.CourseProgresses
    .Where(p => p.CourseId == course.CourseId && 
               p.UserId == userId && 
               p.IsCompleted && 
               allCourseContentIds.Contains(p.ContentId)) // ? Filter invalid IDs
    .Select(p => p.ContentId)
    .Distinct() // ? Lo?i b? duplicate
    .ToListAsync();

// ? Cap progress at 100%
var completionPercentage = Math.Min((double)completedContentIds.Count / totalContents * 100, 100);
```

### 2. Debug API Endpoints

#### A. Debug Progress

**Endpoint:** `GET /api/course-progress/debug-progress?courseSlug={slug}`

**M?c ?ích:** Xem chi ti?t t?t c? contents và progress records

**Response:**
```json
{
  "success": true,
  "courseId": 5,
  "courseName": "L?p trình Web",
  "allContents": [
    {
      "contentId": 10,
      "contentType": "Video",
      "lessonId": 1,
      "title": "Gi?i thi?u",
      "lessonTitle": "Bài 1"
    },
    {
      "contentId": 11,
      "contentType": "Theory",
      "lessonId": 1,
      "title": "Lý thuy?t HTML",
      "lessonTitle": "Bài 1"
    }
  ],
  "totalContents": 5,
  "progressRecords": [
    {
      "progressId": 1,
      "contentId": 10,
      "contentType": "Video",
      "lessonId": 1,
      "isCompleted": true,
      "completionAt": "2024-01-15T10:30:00Z"
    }
  ],
  "totalProgressRecords": 7,
  "completedRecords": 7,
  "duplicateContentIds": [
    { "contentId": 10, "count": 2 }
  ],
  "invalidContentIds": [99, 100]
}
```

**Gi?i thích:**
- `allContents`: T?t c? contents trong khóa h?c (5 items)
- `progressRecords`: T?t c? records trong CourseProgress (7 items - CÓ DUPLICATE!)
- `duplicateContentIds`: ContentId nào b? duplicate
- `invalidContentIds`: ContentId không còn t?n t?i trong course

#### B. Cleanup Progress

**Endpoint:** `POST /api/course-progress/cleanup-progress?courseSlug={slug}`

**M?c ?ích:** Xóa t?t c? invalid progress records

**Response:**
```json
{
  "success": true,
  "message": "Cleaned up 2 invalid records",
  "removedRecords": [
    {
      "progressId": 5,
      "contentId": 99,
      "contentType": "Video"
    },
    {
      "progressId": 6,
      "contentId": 100,
      "contentType": "Theory"
    }
  ]
}
```

## ?? Cách s? d?ng

### B??c 1: Debug ?? xem v?n ??

M? browser console và g?i:

```javascript
const courseSlug = 'lap-trinh-web'; // Thay b?ng slug c?a b?n

// Xem chi ti?t progress
fetch(`/api/course-progress/debug-progress?courseSlug=${courseSlug}`)
    .then(r => r.json())
    .then(data => {
        console.log('=== DEBUG PROGRESS ===');
        console.log('Total contents in course:', data.totalContents);
        console.log('Total progress records:', data.totalProgressRecords);
        console.log('Completed records:', data.completedRecords);
        console.log('Duplicate ContentIds:', data.duplicateContentIds);
        console.log('Invalid ContentIds:', data.invalidContentIds);
        
        if (data.duplicateContentIds.length > 0) {
            console.warn('? Found duplicate records!', data.duplicateContentIds);
        }
        
        if (data.invalidContentIds.length > 0) {
            console.warn('? Found invalid ContentIds!', data.invalidContentIds);
        }
    });
```

### B??c 2: Cleanup d? li?u

N?u có invalid records, cleanup:

```javascript
fetch(`/api/course-progress/cleanup-progress?courseSlug=${courseSlug}`, {
    method: 'POST'
})
    .then(r => r.json())
    .then(data => {
        console.log('? Cleanup result:', data);
        
        // Reload progress
        location.reload();
    });
```

### B??c 3: Verify

Sau khi cleanup, ki?m tra l?i:

```javascript
fetch(`/api/course-progress/get-progress?courseSlug=${courseSlug}`)
    .then(r => r.json())
    .then(data => {
        console.log('=== AFTER CLEANUP ===');
        console.log('Progress:', data.completionPercentage + '%');
        console.log('Completed:', data.completedContents);
        console.log('Total:', data.totalContents);
        
        if (data.completionPercentage > 100) {
            console.error('? Still wrong! Progress > 100%');
        } else {
            console.log('? Progress is correct!');
        }
    });
```

## ?? SQL Query ?? ki?m tra database

### Xem t?t c? progress records c?a m?t user trong m?t course

```sql
-- Thay @UserId và @CourseId
DECLARE @UserId INT = 1;
DECLARE @CourseId INT = 5;

-- Xem progress records
SELECT 
    cp.ProgressId,
    cp.ContentId,
    cp.ContentType,
    cp.LessonId,
    cp.IsCompleted,
    cp.CompletionAt,
    lc.Title AS ContentTitle,
    l.Title AS LessonTitle
FROM CourseProgress cp
LEFT JOIN LessonContent lc ON cp.ContentId = lc.ContentId
LEFT JOIN Lesson l ON cp.LessonId = l.LessonId
WHERE cp.UserId = @UserId 
  AND cp.CourseId = @CourseId
ORDER BY cp.ContentId;
```

### Tìm duplicate records

```sql
-- Tìm ContentId b? duplicate
SELECT 
    ContentId, 
    COUNT(*) AS DuplicateCount
FROM CourseProgress
WHERE UserId = @UserId 
  AND CourseId = @CourseId
GROUP BY ContentId
HAVING COUNT(*) > 1;
```

### Tìm invalid ContentIds

```sql
-- Tìm ContentId không t?n t?i trong LessonContent
SELECT DISTINCT cp.ContentId
FROM CourseProgress cp
WHERE cp.UserId = @UserId 
  AND cp.CourseId = @CourseId
  AND NOT EXISTS (
      SELECT 1 
      FROM LessonContent lc
      INNER JOIN Lesson l ON lc.LessonId = l.LessonId
      INNER JOIN CourseChapter ch ON l.ChapterId = ch.ChapterId
      WHERE ch.CourseId = @CourseId
        AND lc.ContentId = cp.ContentId
  );
```

### Xóa duplicate và invalid records (CAREFUL!)

```sql
-- ?? BACKUP DATABASE FIRST! ??

-- Xóa duplicate (gi? l?i record m?i nh?t)
WITH CTE AS (
    SELECT 
        ProgressId,
        ROW_NUMBER() OVER (
            PARTITION BY UserId, CourseId, ContentId 
            ORDER BY CompletionAt DESC, ProgressId DESC
        ) AS RowNum
    FROM CourseProgress
    WHERE UserId = @UserId AND CourseId = @CourseId
)
DELETE FROM CourseProgress
WHERE ProgressId IN (
    SELECT ProgressId FROM CTE WHERE RowNum > 1
);

-- Xóa invalid ContentIds
DELETE cp
FROM CourseProgress cp
WHERE cp.UserId = @UserId 
  AND cp.CourseId = @CourseId
  AND NOT EXISTS (
      SELECT 1 
      FROM LessonContent lc
      INNER JOIN Lesson l ON lc.LessonId = l.LessonId
      INNER JOIN CourseChapter ch ON l.ChapterId = ch.ChapterId
      WHERE ch.CourseId = @CourseId
        AND lc.ContentId = cp.ContentId
  );
```

## ?? Testing Scenarios

### Scenario 1: Fresh course with no progress

```
Expected:
- Total Contents: 5
- Completed: 0
- Progress: 0%

API Response:
{
  "completionPercentage": 0,
  "completedContents": 0,
  "totalContents": 5
}
```

### Scenario 2: Partially completed

```
Given:
- Total Contents: 5
- User completed: Video (ContentId=10), Theory (ContentId=11)

Expected:
- Completed: 2
- Progress: 40% (2/5)

API Response:
{
  "completionPercentage": 40,
  "completedContents": 2,
  "totalContents": 5
}
```

### Scenario 3: Add new content to course

```
Before:
- Total Contents: 5
- Completed: 3
- Progress: 60%

After adding 5 new contents:
- Total Contents: 10
- Completed: 3 (unchanged)
- Progress: 30% ? AUTO DECREASE!

API Response:
{
  "completionPercentage": 30,
  "completedContents": 3,
  "totalContents": 10
}
```

### Scenario 4: Remove content from course

```
Before:
- Total Contents: 10
- Completed: 8 (including ContentId=99)
- Progress: 80%

After removing ContentId=99:
- Total Contents: 9
- Completed: 7 (99 is invalid now)
- Progress: 77.78% ? AUTO ADJUST!

After cleanup:
- Invalid record removed from CourseProgress
```

## ??? Prevention

### ?? tránh duplicate records trong t??ng lai:

1. **Unique constraint trong database:**

```sql
CREATE UNIQUE INDEX IX_CourseProgress_Unique
ON CourseProgress (UserId, CourseId, ContentId, ContentType);
```

2. **Check exists tr??c khi insert:**

?ã implement trong `MarkContentComplete`:

```csharp
var progress = await _context.CourseProgresses
    .FirstOrDefaultAsync(p => 
        p.CourseId == course.CourseId && 
        p.UserId == userId && 
        p.LessonId == request.LessonId &&
        p.ContentType == request.ContentType &&
        p.ContentId == request.ContentId);

if (progress == null)
{
    // Create new
}
else
{
    // Update existing
}
```

## ?? Checklist khi có l?i progress

- [ ] G?i `/api/course-progress/debug-progress` ?? xem chi ti?t
- [ ] Ki?m tra `duplicateContentIds` - có duplicate không?
- [ ] Ki?m tra `invalidContentIds` - có ContentId không h?p l? không?
- [ ] G?i `/api/course-progress/cleanup-progress` ?? cleanup
- [ ] Reload page và verify progress bar
- [ ] N?u v?n sai, check database tr?c ti?p b?ng SQL

## ?? Common Issues

### Issue 1: Progress > 100%

**Nguyên nhân:** Duplicate ho?c invalid records

**Gi?i pháp:** G?i cleanup API

### Issue 2: Progress không t?ng sau khi complete content

**Nguyên nhân:** 
- API mark-complete failed
- JavaScript không call API
- ContentId không match

**Debug:**
```javascript
// Check console for errors
// Check Network tab for API calls
console.log('ContentId:', contentId);
console.log('API URL:', `/api/course-progress/mark-content-complete`);
```

### Issue 3: Progress gi?m ??t ng?t

**Nguyên nhân:** 
- Gi?ng viên thêm content m?i
- Cleanup xóa invalid records

**Expected behavior:** ?ây là behavior ?úng! Progress ph?i t? ??ng adjust.

---

**Note:** H? th?ng ?ã ???c fix ?? handle các case này t? ??ng. Ch? c?n cleanup data c? là OK!
