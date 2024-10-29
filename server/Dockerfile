FROM golang:1.22-bookworm
WORKDIR /app
COPY go.mod go.sum ./
RUN go mod download
COPY *.go ./
COPY ../.env ./.env
COPY internal ./internal
RUN CGO_ENABLED=0 GOOS=linux go build -o /server


EXPOSE 3000
CMD [ "/server" ]