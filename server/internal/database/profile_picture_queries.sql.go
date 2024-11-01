// Code generated by sqlc. DO NOT EDIT.
// versions:
//   sqlc v1.27.0
// source: profile_picture_queries.sql

package database

import (
	"context"

	"github.com/google/uuid"
)

const getProfilePicture = `-- name: GetProfilePicture :one
SELECT user_id, base64_encoded_image FROM ProfilePictures WHERE user_id = $1
`

func (q *Queries) GetProfilePicture(ctx context.Context, userID uuid.UUID) (Profilepicture, error) {
	row := q.db.QueryRow(ctx, getProfilePicture, userID)
	var i Profilepicture
	err := row.Scan(&i.UserID, &i.Base64EncodedImage)
	return i, err
}

const setProfilePicture = `-- name: SetProfilePicture :exec
INSERT INTO ProfilePictures (user_id, base64_encoded_image) 
VALUES ($1, $2) 
ON CONFLICT (user_id) 
DO UPDATE SET base64_encoded_image = $2
`

type SetProfilePictureParams struct {
	UserID             uuid.UUID
	Base64EncodedImage string
}

func (q *Queries) SetProfilePicture(ctx context.Context, arg SetProfilePictureParams) error {
	_, err := q.db.Exec(ctx, setProfilePicture, arg.UserID, arg.Base64EncodedImage)
	return err
}