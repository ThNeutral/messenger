-- name: CreateUser :one
INSERT INTO Users (user_id, username, email, password)
VALUES ($1, $2, $3, $4)
RETURNING *;

-- name: GetUserByUsername :one
SELECT * FROM Users WHERE username = $1;

-- name: GetUserByEmail :one
SELECT * FROM Users WHERE email = $1;

-- name: GetUserByToken :one
SELECT Users.user_id, Users.username, Users.email, Users.password
FROM Users JOIN Tokens ON Users.user_id = Tokens.user_id WHERE Tokens.token = $1;

-- name: GetUsersByIDs :many
SELECT * FROM Users WHERE user_id = ANY($1::int[]);