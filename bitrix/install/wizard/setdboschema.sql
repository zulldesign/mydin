IF (SELECT default_schema_name FROM sys.database_principals WHERE name = USER) <> 'dbo' BEGIN
	DECLARE @query nvarchar(MAX)
	SET @query = 'ALTER USER ' + QUOTENAME(USER) + ' WITH DEFAULT_SCHEMA = ' + QUOTENAME('dbo')
	EXEC(@query)
END