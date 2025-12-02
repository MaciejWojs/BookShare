
INSERT INTO UserBook (UserId, BookId)
VALUES (
    (SELECT Id FROM AspNetUsers WHERE NormalizedUserName = 'USER1'),
    18
);