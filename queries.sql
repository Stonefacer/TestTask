
use NasaHttpServerStatistic;

DECLARE @Offset AS BIGINT;
DECLARE @Count AS BIGINT;

SET @Offset = 10;
SET @Count = 10;

SELECT Route,
	QueryString,
	ClientHostname,
	ClientLocation,
	StatusCode,
	DataSize,
	Datetime
FROM Requests
ORDER BY Datetime ASC
OFFSET @Offset ROWS
FETCH NEXT @Count ROWS ONLY


/*
41187

8 : 06:41
16: 03:35
*/