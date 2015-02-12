DECLARE @sqlString nvarchar(MAX);

SET @sqlString ='';
SELECT @sqlString = @sqlString + N'ALTER TABLE ' + QUOTENAME(s.name) + '.' + QUOTENAME(u.name) + ' DROP CONSTRAINT ' + QUOTENAME(f.name) + '; ' 
	FROM sys.objects f 
	INNER JOIN sys.schemas s ON f.schema_id = s.schema_id
	INNER JOIN sys.objects u ON f.parent_object_id = u.object_id 
	WHERE f.type='F' AND u.type='U'
EXEC(@sqlString);

SET @sqlString ='';
SELECT @sqlString = @sqlString + N'DROP TABLE ' + QUOTENAME(s.name) + '.' +  QUOTENAME(t.name) + '; ' 
	FROM sys.objects t 
	INNER JOIN sys.schemas s ON s.schema_id = t.schema_id 
	WHERE t.type='U';
EXEC(@sqlString); 

SET @sqlString ='';
SELECT @sqlString = @sqlString + N'DROP PROC ' + QUOTENAME(s.name) + '.' +  QUOTENAME(t.name) + '; '
	FROM sys.objects t 
	INNER JOIN sys.schemas s ON s.schema_id = t.schema_id  
	WHERE t.type='P';
EXEC(@sqlString); 

SET @sqlString ='';
SELECT @sqlString = @sqlString + N'DROP FUNCTION ' + QUOTENAME(s.name) + '.' +  QUOTENAME(t.name) + '; ' 
	FROM sys.objects t 
	INNER JOIN sys.schemas s ON s.schema_id = t.schema_id 
	WHERE t.type='FN';
EXEC(@sqlString); 

SET @sqlString ='';
SELECT @sqlString = @sqlString + N'DROP VIEW ' + QUOTENAME(s.name) + '.' +  QUOTENAME(t.name) + '; '
	FROM sys.objects t 
	INNER JOIN sys.schemas s ON s.schema_id = t.schema_id  
	WHERE t.type='V';
EXEC(@sqlString); 