docker build -t eshopservice:latest -f Dockerfile .
docker build -t userservice:latest -f UserService/Dockerfile .
docker compose up

HTTP: http://localhost:8080
Plant Service Swagger UI: http://localhost:8080/swagger
User Service Swagger UI: http://localhost:8081/swagger