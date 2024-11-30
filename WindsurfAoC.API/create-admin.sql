INSERT INTO Users (Email, PasswordHash, IsAdmin)
VALUES (
    'domingo@intrasoft.es',
    -- Hash para la contraseña '123456' usando HMACSHA512
    '8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92',
    1 -- IsAdmin = true
);
