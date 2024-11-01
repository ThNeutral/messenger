CREATE TABLE Users (
    user_id UUID PRIMARY KEY,
    username VARCHAR(100) NOT NULL UNIQUE,
    email VARCHAR(100) NOT NULL UNIQUE,
    password VARCHAR(255) NOT NULL
);

CREATE TABLE ProfilePictures (
    user_id UUID PRIMARY KEY,
    base64_encoded_image TEXT NOT NULL,
    FOREIGN KEY (user_id) REFERENCES Users(user_id) ON DELETE CASCADE
);

CREATE TABLE Tokens (
    user_id UUID PRIMARY KEY,
    token VARCHAR(255) NOT NULL,
    expires_at TIMESTAMP NOT NULL,
    FOREIGN KEY (user_id) REFERENCES Users(user_id) ON DELETE CASCADE
);

CREATE TABLE Chats (
    chat_id UUID PRIMARY KEY,
    chatname VARCHAR(100) NOT NULL
);


CREATE TABLE ChatsToUsers (
    user_id UUID,
    chat_id UUID,
    PRIMARY KEY (user_id, chat_id),
    FOREIGN KEY (chat_id) REFERENCES Chats(chat_id) ON DELETE CASCADE,
    FOREIGN KEY (user_id) REFERENCES Users(user_id) ON DELETE CASCADE
);

CREATE TABLE Messages (
    message_id UUID PRIMARY KEY,
    content TEXT NOT NULL,
    send_time TIMESTAMP NOT NULL,

    chat_id UUID NOT NULL,
    user_id UUID NOT NULL,
    FOREIGN KEY (chat_id) REFERENCES Chats(chat_id) ON DELETE CASCADE,
    FOREIGN KEY (user_id) REFERENCES Users(user_id) ON DELETE CASCADE
);

CREATE TABLE WatchedBy (
    message_id UUID,
    user_id UUID,
    PRIMARY KEY (user_id, message_id),
    FOREIGN KEY (message_id) REFERENCES Messages(message_id) ON DELETE CASCADE,
    FOREIGN KEY (user_id) REFERENCES Users(user_id) ON DELETE CASCADE
);
