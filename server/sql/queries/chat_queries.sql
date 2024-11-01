-- name: CreateChat :one
INSERT INTO Chats (chat_id, chatname)
VALUES ($1, $2)
RETURNING *;

-- name: AddUserToChat :exec
INSERT INTO ChatsToUsers (chat_id, user_id) VALUES ($1, $2);

-- name: GetUsersFromChat :many
SELECT Users.user_id FROM Users 
JOIN ChatsToUsers ON Users.user_id = ChatsToUsers.user_id
WHERE ChatsToUsers.chat_id = $1;

-- name: GetChatByChatID :one
SELECT * FROM Chats WHERE Chats.chat_id = $1;

-- name: DeleteChat :exec
DELETE FROM Chats WHERE Chats.chat_id = $1;

-- name: DeleteUserFromChat :exec
DELETE FROM ChatsToUsers WHERE ChatsToUsers.chat_id = $1 AND ChatsToUsers.user_id = $2;

-- name: GetChatsOfUser :many
SELECT Chats.chat_id FROM Chats 
JOIN ChatsToUsers ON Chats.chat_id = ChatsToUsers.chat_id
WHERE ChatsToUsers.user_id = $1;
