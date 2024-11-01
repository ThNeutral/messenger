package hub

import (
	"log"
	"sync"

	"github.com/gorilla/websocket"
)

type Hub struct {
	Chats       map[string]*Chat
	Broadcast   chan BroadcastRequest
	Subscribe   chan SubscribeRequest
	Unsubscribe chan UnsubscribeRequest
	StopSignal  chan bool
}

type Chat struct {
	Users map[*websocket.Conn]bool
}

type SubscribeRequest struct {
	ChatID string
	User   *websocket.Conn
}

type UnsubscribeRequest SubscribeRequest

type BroadcastRequest struct {
	ChatID  string
	From    *websocket.Conn
	Message []byte
}

func GetNewHub() *Hub {
	return &Hub{
		Chats:       make(map[string]*Chat),
		Broadcast:   make(chan BroadcastRequest),
		Subscribe:   make(chan SubscribeRequest),
		Unsubscribe: make(chan UnsubscribeRequest),
		StopSignal:  make(chan bool),
	}
}

func (h *Hub) Run() {
	for {
		select {
		case request := <-h.Subscribe:
			{
				chat, ok := h.Chats[request.ChatID]
				if !ok {
					chat = &Chat{Users: make(map[*websocket.Conn]bool)}
					h.Chats[request.ChatID] = chat
				}
				chat.Users[request.User] = true
			}
		case request := <-h.Unsubscribe:
			{
				chat, ok := h.Chats[request.ChatID]
				if !ok {
					log.Printf("Recieved request to delete user from non-existent chat %s\n", request.ChatID)
					continue
				}
				delete(chat.Users, request.User)
				request.User.Close()
			}
		case request := <-h.Broadcast:
			{
				chat, ok := h.Chats[request.ChatID]
				if !ok {
					log.Printf("Recieved request to broadcast data to non-existent chat %s\n", request.ChatID)
					continue
				}
				if _, ok := chat.Users[request.From]; !ok {
					log.Printf("Tried to write message to a chat which user does not belong to. ChatID: %s\n", request.ChatID)
					continue
				}
				var wg sync.WaitGroup
				for user, _ := range chat.Users {
					if user == request.From {
						continue
					}
					wg.Add(1)
					go func(u *websocket.Conn) {
						defer wg.Done()
						u.WriteMessage(websocket.TextMessage, request.Message)
					}(user)
					wg.Wait()
				}
			}
		case <-h.StopSignal:
			{
				log.Println("Recieved request to stop hub")
				for _, chat := range h.Chats {
					for user, _ := range chat.Users {
						user.Close()
					}
				}
				break
			}
		}
	}
}
