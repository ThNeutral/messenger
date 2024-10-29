package main

import (
	"context"
	"fmt"
	"html/template"
	"log"
	"net/http"
	"os"

	"github.com/go-chi/chi/v5"
	"github.com/go-chi/chi/v5/middleware"
	"github.com/go-chi/cors"
	"github.com/gorilla/websocket"
	"github.com/jackc/pgx/v5"
	"github.com/joho/godotenv"

	_ "github.com/lib/pq"
	"github.com/thneutral/messenger/server/internal/database"
	"github.com/thneutral/messenger/server/internal/handlers"
	"github.com/thneutral/messenger/server/internal/hub"
)

var h *hub.Hub
var upgrader *websocket.Upgrader

func init() {
	h = hub.GetNewHub()
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
	err := godotenv.Load(".env")
	if err != nil {
		fmt.Println("Failed to load env")
	}

	pg_user := os.Getenv("POSTGRES_USER")
	shouldStop := false
	if pg_user == "" {
		fmt.Println("Environment variable POSTGRES_USER is not found")
		shouldStop = true
	}

	pg_db := os.Getenv("POSTGRES_DB")
	if pg_db == "" {
		fmt.Println("Environment variable POSTGRES_DB is not found")
		shouldStop = true
	}

	pg_password := os.Getenv("POSTGRES_PASSWORD")
	if pg_password == "" {
		fmt.Println("Environment variable POSTGRES_PASSWORD is not found")
		shouldStop = true
	}

	pg_host := os.Getenv("POSTGRES_HOST")
	if pg_host == "" {
		fmt.Println("Environment variable POSTGRES_HOST is not found")
		shouldStop = true
	}

	pg_port := os.Getenv("POSTGRES_PORT")
	if pg_port == "" {
		fmt.Println("Environment variable POSTGRES_PORT is not found")
		shouldStop = true
	}

	if shouldStop {
		return
	}

	db_url := fmt.Sprintf("host=%v port=%v database=%v user=%v password=%v sslmode=disable", pg_host, pg_port, pg_db, pg_user, pg_password)

	ctx := context.Background()

	conn, err := pgx.Connect(ctx, db_url)
	if err != nil {
		log.Println(err)
		return
	}
	defer conn.Close(ctx)

	queries := database.New(conn)

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

	r.Post("/register", handlers.CreateUser(queries))
	r.Post("/login-by-email", handlers.LoginByEmail(queries))
	r.Post("/login-by-username", handlers.LoginByUsername(queries))
	r.Post("/change-profile-picture", handlers.AuthMiddleware(queries, handlers.ChangeProfilePicture))
	r.Get("/me", handlers.AuthMiddleware(queries, handlers.GetUserData))

	r.Post("/create-chat", handlers.AuthMiddleware(queries, handlers.HandleCreateChat))
	r.Post("/add-users-to-chat", handlers.AuthMiddleware(queries, handlers.HandleAddUsersToChat))
	r.Post("/get-members-of-chat", handlers.AuthMiddleware(queries, handlers.HandleGetMembersOfChat))
	r.Post("/delete-members-from-chat", handlers.AuthMiddleware(queries, handlers.HandleDeleteMembersFromChat))
	r.Post("/delete-chat", handlers.AuthMiddleware(queries, handlers.HandleDeleteChat))
	r.Post("/get-my-chats", handlers.AuthMiddleware(queries, handlers.HandleGetMyChats))

	r.Get("/chat/{id}", handlers.HandleChat(h, upgrader))

	r.Get("/html/", serveTestFile)

	go h.Run()

	httpPort := os.Getenv("PORT")
	if httpPort == "" {
		httpPort = "3000"
	}

	fmt.Println("Listening on :" + httpPort)
	http.ListenAndServe(":"+httpPort, r)
}

func serveTestFile(w http.ResponseWriter, r *http.Request) {
	templ := template.Must(template.ParseFiles("./html/index.html"))
	templ.Execute(w, nil)
}
