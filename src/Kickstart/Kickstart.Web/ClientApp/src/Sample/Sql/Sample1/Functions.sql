
CREATE FUNCTION [sample1].[Function1](@Temp VARCHAR(200))
RETURNS VARCHAR(200)
AS
BEGIN

    DECLARE @KeepValues AS VARCHAR(50)
    SET @KeepValues = '%[^0-z]%'
    WHILE PatIndex(@KeepValues, @Temp) > 0
        SET @Temp = Stuff(@Temp, PatIndex(@KeepValues, @Temp), 1, '')

    RETURN @Temp
END
GO
