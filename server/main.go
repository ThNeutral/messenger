package main

import (
	"html/template"
	"log"
	"net/http"

	"github.com/go-chi/chi/v5"
	"github.com/go-chi/chi/v5/middleware"
	"github.com/go-chi/cors"
	"github.com/gorilla/websocket"
)

var hub *Hub
var upgrader *websocket.Upgrader

func init() {
	hub = GetNewHub()
	upgrader = &websocket.Upgrader{
		ReadBufferSize:    4096,
		WriteBufferSize:   4096,
		EnableCompression: true,
		CheckOrigin: func(r *http.Request) bool {
			return true
		},
	}
}

func main() {
	r := chi.NewRouter()

	r.Use(middleware.RequestID)
	r.Use(middleware.RealIP)
	r.Use(middleware.Logger)
	r.Use(middleware.Recoverer)

	r.Use(cors.Handler(cors.Options{
		AllowedOrigins:   []string{"https://*", "http://*"},
		AllowedMethods:   []string{"*"},
		AllowedHeaders:   []string{"*"},
		ExposedHeaders:   []string{"*"},
		AllowCredentials: false,
		MaxAge:           300, // Maximum value not ignored by any of major browsers
	}))

	r.Get("/chat/{id}", handleChat)
	r.Get("/static/", handleStatic)

	go hub.Run()

	log.Println("Listening on :3000")
	http.ListenAndServe(":3000", r)
}

func handleStatic(w http.ResponseWriter, r *http.Request) {
	templ := template.Must(template.ParseFiles("./html/index.html"))
	templ.Execute(w, nil)
}
