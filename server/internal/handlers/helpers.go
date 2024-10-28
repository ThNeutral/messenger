package handlers

import (
	"encoding/json"
	"errors"
	"io"
	"net/http"
	"reflect"
	"strings"

	"github.com/thneutral/messenger/server/internal/database"
)

func writeResponse(w http.ResponseWriter, payload interface{}, code int) {
	w.WriteHeader(code)
	if payload == nil {
		return
	}
	bytes, _ := json.Marshal(payload)
	w.Write(bytes)
}

func writeError(w http.ResponseWriter, msg string, code int) {
	type Model struct {
		Message string `json:"message"`
	}
	var m Model
	m.Message = msg
	bytes, _ := json.Marshal(m)
	w.Write(bytes)
	w.WriteHeader(code)
}

func AuthMiddleware(queries *database.Queries, handler func(queries *database.Queries, user database.User) func(w http.ResponseWriter, r *http.Request)) func(w http.ResponseWriter, r *http.Request) {
	return func(w http.ResponseWriter, r *http.Request) {
		user, err := queries.GetUserByToken(r.Context(), strings.Replace(r.Header["Authorization"][0], "Bearer ", "", -1))
		if err != nil {
			writeError(w, "Failed to authorize user", http.StatusForbidden)
			return
		}
		handler(queries, user)(w, r)
	}
}

func verifyModel[T any](w http.ResponseWriter, r *http.Request) (T, error) {
	var payload T
	data, _ := io.ReadAll(r.Body)
	err := json.Unmarshal(data, payload)
	if err != nil {
		writeError(w, "Failed to unmarshal fileds", http.StatusBadRequest)
		return payload, err
	}
	val := reflect.ValueOf(payload)
	typ := reflect.TypeOf(payload)
	isError := false
	msg := "Failed to find field(s):"
	for i := 0; i < val.NumField(); i++ {
		fieldValue := val.Field(i)
		if fieldValue.IsZero() {
			fieldName := typ.Field(i).Tag.Get("json")
			msg += "\t" + fieldName
			isError = true
		}
	}
	if isError {
		writeError(w, msg, http.StatusBadRequest)
		return payload, errors.New("failed to unmarhsal fields")
	}
	return payload, nil
}
