package main

import (
	"log"
	"net/http"
	"strings"

	"github.com/gorilla/websocket"
)

func handleChat(w http.ResponseWriter, r *http.Request) {
	splittedPath := strings.Split(r.URL.Path, "/")
	chatID := splittedPath[len(splittedPath)-1]
	ws, err := upgrader.Upgrade(w, r, nil)
	if err != nil {
		log.Println("Failed to upgrade http to websocket")
		return
	}
	var sr SubscribeRequest
	sr.ChatID = chatID
	sr.User = ws
	hub.Subscribe <- sr
	retries := 0
	for {
		messageType, bytes, err := ws.ReadMessage()
		if err != nil {
			log.Printf("Failed to process WS message from %v\n", ws.RemoteAddr().String())
			retries += 1
			if retries == 3 {
				var usr UnsubscribeRequest
				usr.ChatID = chatID
				usr.User = ws
				hub.Unsubscribe <- usr
				break
			}
			continue
		}
		if messageType != websocket.TextMessage {
			log.Printf("Recieved unknown message type %v\n", messageType)
			continue
		}
		var br BroadcastRequest
		br.ChatID = chatID
		br.From = ws
		br.Message = bytes
		hub.Broadcast <- br
	}
}
