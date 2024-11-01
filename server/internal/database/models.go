// Code generated by sqlc. DO NOT EDIT.
// versions:
//   sqlc v1.27.0

package database

import (
	"time"

	"github.com/google/uuid"
)

type Chat struct {
	ChatID   uuid.UUID
	Chatname string
}

type Chatstouser struct {
	UserID uuid.UUID
	ChatID uuid.UUID
}

type Message struct {
	MessageID uuid.UUID
	Content   string
	SendTime  time.Time
	ChatID    uuid.UUID
	UserID    uuid.UUID
}

type Profilepicture struct {
	UserID             uuid.UUID
	Base64EncodedImage string
}

type Token struct {
	UserID    uuid.UUID
	Token     string
	ExpiresAt time.Time
}

type User struct {
	UserID   uuid.UUID
	Username string
	Email    string
	Password string
}

type Watchedby struct {
	MessageID uuid.UUID
	UserID    uuid.UUID
}