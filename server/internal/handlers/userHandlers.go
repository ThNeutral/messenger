package handlers

import (
	"net/http"
	"time"

	"github.com/google/uuid"
	"github.com/thneutral/messenger/server/internal/database"
	"golang.org/x/crypto/bcrypt"
)

func CreateUser(queries *database.Queries) func(w http.ResponseWriter, r *http.Request) {
	return func(w http.ResponseWriter, r *http.Request) {
		type RequestModel struct {
			Username           string `json:"username"`
			Email              string `json:"email"`
			Password           string `json:"password"`
			Base64EncodedImage string `json:"base64encodedimage"`
		}
		reqmodel, err := verifyModel[RequestModel](w, r)
		if err != nil {
			return
		}
		if len(reqmodel.Password) > 72 {
			writeError(w, "Password is too long", http.StatusBadRequest)
			return
		}
		hashed, err := bcrypt.GenerateFromPassword([]byte(reqmodel.Password), bcrypt.DefaultCost)
		if err != nil {
			writeError(w, "Failed to hash password", http.StatusInternalServerError)
			return
		}
		newUser, err := queries.CreateUser(r.Context(), database.CreateUserParams{
			UserID:   uuid.New(),
			Username: reqmodel.Username,
			Password: string(hashed),
			Email:    reqmodel.Email,
		})
		if err != nil {
			writeError(w, "Failed to create user", http.StatusInternalServerError)
			return
		}
		queries.SetProfilePicture(r.Context(), database.SetProfilePictureParams{
			UserID:             newUser.UserID,
			Base64EncodedImage: reqmodel.Base64EncodedImage,
		})
		newToken, err := queries.SetTokenForUser(r.Context(), database.SetTokenForUserParams{
			UserID:    newUser.UserID,
			Token:     uuid.NewString(),
			ExpiresAt: time.Now().Add(1 * time.Hour),
		})
		if err != nil {
			writeError(w, "Failed to generate token", http.StatusInternalServerError)
			return
		}
		type ResponseModel struct {
			Token string `json:"token"`
		}
		var respmodel ResponseModel
		respmodel.Token = newToken.Token
		writeResponse(w, respmodel, http.StatusCreated)
	}
}

func LoginByUsername(queries *database.Queries) func(w http.ResponseWriter, r *http.Request) {
	return func(w http.ResponseWriter, r *http.Request) {
		type RequestModel struct {
			Username string `json:"username"`
			Password string `json:"password"`
		}
		reqmodel, err := verifyModel[RequestModel](w, r)
		if err != nil {
			return
		}
		user, err := queries.GetUserByUsername(r.Context(), reqmodel.Username)
		if err != nil {
			writeError(w, "Failed to find user with that username", http.StatusNotFound)
			return
		}
		err = bcrypt.CompareHashAndPassword([]byte(reqmodel.Password), []byte(user.Password))
		if err != nil {
			writeError(w, "Wrong password", http.StatusForbidden)
			return
		}
		token, err := queries.GetTokenOfUser(r.Context(), user.UserID)
		if err != nil {
			writeError(w, "Failed to find token of given user", http.StatusInternalServerError)
			return
		}
		type ResponseModel struct {
			Token string `json:"token"`
		}
		var respmodel ResponseModel
		respmodel.Token = token.Token
		writeResponse(w, respmodel, http.StatusOK)
	}
}

func LoginByEmail(queries *database.Queries) func(w http.ResponseWriter, r *http.Request) {
	return func(w http.ResponseWriter, r *http.Request) {
		type RequestModel struct {
			Email    string `json:"email"`
			Password string `json:"password"`
		}
		reqmodel, err := verifyModel[RequestModel](w, r)
		if err != nil {
			return
		}
		user, err := queries.GetUserByEmail(r.Context(), reqmodel.Email)
		if err != nil {
			writeError(w, "Failed to find user with that email", http.StatusNotFound)
			return
		}
		err = bcrypt.CompareHashAndPassword([]byte(reqmodel.Password), []byte(user.Password))
		if err != nil {
			writeError(w, "Wrong password", http.StatusForbidden)
			return
		}
		token, err := queries.GetTokenOfUser(r.Context(), user.UserID)
		if err != nil {
			writeError(w, "Failed to find token of given user", http.StatusInternalServerError)
			return
		}
		type ResponseModel struct {
			Token string `json:"token"`
		}
		var respmodel ResponseModel
		respmodel.Token = token.Token
		writeResponse(w, respmodel, http.StatusOK)
	}
}

func ChangeProfilePicture(queries *database.Queries, user database.User) func(w http.ResponseWriter, r *http.Request) {
	return func(w http.ResponseWriter, r *http.Request) {
		type RequestModel struct {
			Base64EncodedImage string `json:"base64encodedimage"`
		}
		reqmodel, err := verifyModel[RequestModel](w, r)
		if err != nil {
			return
		}
		err = queries.SetProfilePicture(r.Context(), database.SetProfilePictureParams{
			UserID:             user.UserID,
			Base64EncodedImage: reqmodel.Base64EncodedImage,
		})
		if err != nil {
			writeError(w, "Failed to set profile picture", http.StatusInternalServerError)
			return
		}
		writeResponse(w, nil, http.StatusOK)
	}
}

func GetUserData(queries *database.Queries, user database.User) func(w http.ResponseWriter, r *http.Request) {
	return func(w http.ResponseWriter, r *http.Request) {
		type ResponseModel struct {
			Username           string `json:"username"`
			Email              string `json:"email"`
			Base64EncodedImage string `json:"base64encodedimage"`
		}
		var respmodel ResponseModel
		respmodel.Email = user.Email
		respmodel.Username = user.Username
		pp, err := queries.GetProfilePicture(r.Context(), user.UserID)
		if err != nil {
			respmodel.Base64EncodedImage = ""
		} else {
			respmodel.Base64EncodedImage = pp.Base64EncodedImage
		}
		writeResponse(w, respmodel, http.StatusOK)
	}
}
