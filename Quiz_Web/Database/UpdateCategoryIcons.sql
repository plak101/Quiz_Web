-- Update CourseCategories with Bootstrap Icons (temporary solution)
-- You can replace these with custom icon URLs later

UPDATE dbo.CourseCategories
SET IconUrl = CASE 
    WHEN Name = N'L?p trình' THEN 'https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/icons/code-slash.svg'
    WHEN Name = N'C? s? d? li?u' THEN 'https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/icons/database.svg'
    WHEN Name = N'Phân tích d? li?u' THEN 'https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/icons/graph-up.svg'
    WHEN Name = N'Trí tu? nhân t?o' THEN 'https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/icons/robot.svg'
    ELSE IconUrl
END
WHERE Name IN (N'L?p trình', N'C? s? d? li?u', N'Phân tích d? li?u', N'Trí tu? nhân t?o');

-- Verify update
SELECT CategoryId, Name, IconUrl, DisplayOrder
FROM dbo.CourseCategories
ORDER BY DisplayOrder, Name;

-- If you want to add more categories with icons:
-- INSERT INTO dbo.CourseCategories (Name, Slug, Description, IconUrl, DisplayOrder)
-- VALUES 
-- (N'Thi?t k?', 'thiet-ke', N'H?c thi?t k? ?? h?a, UI/UX', 'https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/icons/palette.svg', 5),
-- (N'Marketing', 'marketing', N'H?c digital marketing, SEO', 'https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/icons/megaphone.svg', 6);
