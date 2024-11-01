package handlers

import (
	"log"
	"net/http"
	"slices"
	"strings"

	"github.com/google/uuid"
	"github.com/gorilla/websocket"
	"github.com/thneutral/messenger/server/internal/database"
	"github.com/thneutral/messenger/server/internal/hub"
)

func HandleCreateChat(queries *database.Queries, user database.User) func(w http.ResponseWriter, r *http.Request) {
	return func(w http.ResponseWriter, r *http.Request) {
		type RequestModel struct {
			Chatname string `json:"chatname"`
		}
		reqmodel, err := verifyModel[RequestModel](w, r)
		if err != nil {
			return
		}
		newChat, err := queries.CreateChat(r.Context(), database.CreateChatParams{
			ChatID:   uuid.New(),
			Chatname: reqmodel.Chatname,
		})
		if err != nil {
			writeError(w, "Failed to create chat", http.StatusInternalServerError)
			return
		}
		err = queries.AddUserToChat(r.Context(), database.AddUserToChatParams{
			ChatID: newChat.ChatID,
			UserID: user.UserID,
		})
		if err != nil {
			writeError(w, "Failed to add user to chat", http.StatusInternalServerError)
			return
		}
		type ResponseModel struct {
			ChatID string `json:"chat_id"`
		}
		var respmodel ResponseModel
		respmodel.ChatID = newChat.ChatID.String()
		writeResponse(w, respmodel, http.StatusCreated)
	}
}

func HandleAddUsersToChat(queries *database.Queries, addingUser database.User) func(w http.ResponseWriter, r *http.Request) {
	return func(w http.ResponseWriter, r *http.Request) {
		type RequestModel struct {
			ChatID  string   `json:"chat_id"`
			UserIDs []string `json:"user_ids"`
		}
		reqmodel, err := verifyModel[RequestModel](w, r)
		if err != nil {
			return
		}
		supposedChatID, err := uuid.FromBytes([]byte(reqmodel.ChatID))
		if err != nil {
			writeError(w, "Invalid chatID", http.StatusBadRequest)
			return
		}
		chat, err := queries.GetChatByChatID(r.Context(), supposedChatID)
		if err != nil {
			writeError(w, "Invalid chatID", http.StatusBadRequest)
			return
		}
		chatUserIDs, err := queries.GetUsersFromChat(r.Context(), chat.ChatID)
		if err != nil {
			writeError(w, "Internal server error", http.StatusInternalServerError)
			return
		}
		if !slices.ContainsFunc(chatUserIDs, func(userID uuid.UUID) bool {
			return userID.String() == addingUser.UserID.String()
		}) {
			writeError(w, "User doesn`t have access to that chat", http.StatusForbidden)
			return
		}
		msg := "Failed to add next userIDs:"
		isError := false
		for _, user := range reqmodel.UserIDs {
			userID, err := uuid.FromBytes([]byte(user))
			if err != nil {
				msg += "\t" + user
				isError = true
				continue
			}
			err = queries.AddUserToChat(r.Context(), database.AddUserToChatParams{
				ChatID: chat.ChatID,
				UserID: userID,
			})
			if err != nil {
				msg += "\t" + user
				isError = true
				continue
			}
		}
		if isError {
			writeError(w, msg, http.StatusOK)
		}
		writeResponse(w, nil, http.StatusOK)
	}
}

func HandleGetMembersOfChat(queries *database.Queries, addingUser database.User) func(w http.ResponseWriter, r *http.Request) {
	return func(w http.ResponseWriter, r *http.Request) {
		type RequestModel struct {
			ChatID string `json:"chat_id"`
		}
		reqmodel, err := verifyModel[RequestModel](w, r)
		if err != nil {
			return
		}
		supposedChatID, err := uuid.FromBytes([]byte(reqmodel.ChatID))
		if err != nil {
			writeError(w, "Invalid chatID", http.StatusBadRequest)
			return
		}
		chat, err := queries.GetChatByChatID(r.Context(), supposedChatID)
		if err != nil {
			writeError(w, "Invalid chatID", http.StatusBadRequest)
			return
		}
		chatUserIDs, err := queries.GetUsersFromChat(r.Context(), chat.ChatID)
		if err != nil {
			writeError(w, "Internal server error", http.StatusInternalServerError)
			return
		}
		if !slices.ContainsFunc(chatUserIDs, func(userID uuid.UUID) bool {
			return userID.String() == addingUser.UserID.String()
		}) {
			writeError(w, "User doesn`t have access to that chat", http.StatusForbidden)
			return
		}
		var stringChatUserIDs []string
		for _, userID := range chatUserIDs {
			stringChatUserIDs = append(stringChatUserIDs, userID.String())
		}
		type ResponseModel struct {
			UserIDs []string `json:"user_ids"`
		}
		var respmodel ResponseModel
		respmodel.UserIDs = stringChatUserIDs
		writeResponse(w, respmodel, http.StatusOK)
	}
}

func HandleDeleteMembersFromChat(queries *database.Queries, deletingUser database.User) func(w http.ResponseWriter, r *http.Request) {
	return func(w http.ResponseWriter, r *http.Request) {
		type RequestModel struct {
			ChatID  string   `json:"chat_id"`
			UserIDs []string `json:"user_ids"`
		}
		reqmodel, err := verifyModel[RequestModel](w, r)
		if err != nil {
			return
		}
		supposedChatID, err := uuid.FromBytes([]byte(reqmodel.ChatID))
		if err != nil {
			writeError(w, "Invalid chatID", http.StatusBadRequest)
			return
		}
		chat, err := queries.GetChatByChatID(r.Context(), supposedChatID)
		if err != nil {
			writeError(w, "Invalid chatID", http.StatusBadRequest)
			return
		}
		chatUserIDs, err := queries.GetUsersFromChat(r.Context(), chat.ChatID)
		if err != nil {
			writeError(w, "Internal server error", http.StatusInternalServerError)
			return
		}
		if !slices.ContainsFunc(chatUserIDs, func(userID uuid.UUID) bool {
			return userID.String() == deletingUser.UserID.String()
		}) {
			writeError(w, "User doesn`t have access to that chat", http.StatusForbidden)
			return
		}
		msg := "Failed to delete those user_ids:"
		isError := false
		for _, user := range reqmodel.UserIDs {
			userID, err := uuid.FromBytes([]byte(user))
			if err != nil {
				msg += "\t" + user
				isError = true
				continue
			}
			err = queries.DeleteUserFromChat(r.Context(), database.DeleteUserFromChatParams{
				ChatID: chat.ChatID,
				UserID: userID,
			})
			if err != nil {
				msg += "\t" + user
				isError = true
				continue
			}
		}
		if isError {
			writeError(w, msg, http.StatusNoContent)
			return
		}
		writeResponse(w, nil, http.StatusNoContent)
	}
}

func HandleDeleteChat(queries *database.Queries, deletingUser database.User) func(w http.ResponseWriter, r *http.Request) {
	return func(w http.ResponseWriter, r *http.Request) {
		type RequestModel struct {
			ChatID  string   `json:"chat_id"`
			UserIDs []string `json:"user_ids"`
		}
		reqmodel, err := verifyModel[RequestModel](w, r)
		if err != nil {
			return
		}
		supposedChatID, err := uuid.FromBytes([]byte(reqmodel.ChatID))
		if err != nil {
			writeError(w, "Invalid chatID", http.StatusBadRequest)
			return
		}
		chat, err := queries.GetChatByChatID(r.Context(), supposedChatID)
		if err != nil {
			writeError(w, "Invalid chatID", http.StatusBadRequest)
			return
		}
		chatUserIDs, err := queries.GetUsersFromChat(r.Context(), chat.ChatID)
		if err != nil {
			writeError(w, "Internal server error", http.StatusInternalServerError)
			return
		}
		if !slices.ContainsFunc(chatUserIDs, func(userID uuid.UUID) bool {
			return userID.String() == deletingUser.UserID.String()
		}) {
			writeError(w, "User doesn`t have access to that chat", http.StatusForbidden)
			return
		}
		err = queries.DeleteChat(r.Context(), chat.ChatID)
		if err != nil {
			writeError(w, "Failed to delete chat", http.StatusInternalServerError)
			return
		}
		writeResponse(w, nil, http.StatusNoContent)
	}
}

func HandleGetMyChats(queries *database.Queries, user database.User) func(w http.ResponseWriter, r *http.Request) {
	return func(w http.ResponseWriter, r *http.Request) {
		chats, err := queries.GetChatsOfUser(r.Context(), user.UserID)
		if err != nil {
			writeError(w, "Failed to fetch chats", http.StatusInternalServerError)
			return
		}
		type ResponseModel struct {
			ChatIDs []string `json:"chat_ids"`
		}
		var respmodel ResponseModel
		for _, chat := range chats {
			respmodel.ChatIDs = append(respmodel.ChatIDs, chat.String())
		}
		writeResponse(w, respmodel, http.StatusOK)
	}
}

func HandleChat(h *hub.Hub, upgrader *websocket.Upgrader) func(w http.ResponseWriter, r *http.Request) {
	return func(w http.ResponseWriter, r *http.Request) {
		splittedPath := strings.Split(r.URL.Path, "/")
		chatID := splittedPath[len(splittedPath)-1]
		ws, err := upgrader.Upgrade(w, r, nil)
		if err != nil {
			log.Println("Failed to upgrade http to websocket")
			return
		}
		var sr hub.SubscribeRequest
		sr.ChatID = chatID
		sr.User = ws
		h.Subscribe <- sr
		for {
			messageType, bytes, err := ws.ReadMessage()
			if strings.Contains(err.Error(), "100") {
				break
			}
			if err != nil {
				log.Printf("Failed to process WS message from %v\t Error: %v\n", ws.RemoteAddr().String(), err)
				break
			}
			if messageType != websocket.TextMessage {
				log.Printf("Recieved unknown message type %v\n", messageType)
				continue
			}
			var br hub.BroadcastRequest
			br.ChatID = chatID
			br.From = ws
			br.Message = bytes
			h.Broadcast <- br
		}
		var usr hub.UnsubscribeRequest
		usr.ChatID = chatID
		usr.User = ws
		h.Unsubscribe <- usr
	}
}
