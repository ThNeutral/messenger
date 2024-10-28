-- name: SetProfilePicture :exec
INSERT INTO ProfilePictures (user_id, base64_encoded_image) 
VALUES ($1, $2) 
ON CONFLICT (user_id) 
DO UPDATE SET base64_encoded_image = $2;

-- name: GetProfilePicture :one
SELECT * FROM ProfilePictures WHERE user_id = $1;