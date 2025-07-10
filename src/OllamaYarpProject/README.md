# OllamaYarpProject

This project is an ASP.NET Core reverse proxy that exposes the same endpoint as Ollama and redirects requests to another remote URL (default: http://localhost:4000).

## How to Run

1. Build and run the project:
   ```pwsh
   dotnet run
   ```
2. The proxy will listen on the default port (e.g., http://localhost:11434) and forward requests to http://localhost:4000.

## Configuration

- You can change the target URL in the appsettings.json or in the code.

## Endpoints

- The proxy will respond to the same endpoint as Ollama (e.g., /api/chat or /api/generate) and forward requests accordingly.

---
