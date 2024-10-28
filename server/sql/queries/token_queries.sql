-- name: SetTokenForUser :one
INSERT INTO Tokens (user_id, token, expires_at) 
VALUES ($1, $2, $3) 
ON CONFLICT (user_id) 
DO UPDATE SET token = $2 AND expires_at = $3
RETURNING *;

-- name: GetTokenOfUser :one
SELECT * FROM Tokens WHERE user_id = $1;