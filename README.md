# OllamaYarpProxy

OllamaYarpProxy is an ASP.NET Core reverse proxy that emulates Ollama's API endpoints and transparently forwards requests to a configurable backend (default: http://localhost:4000). It uses YARP (Yet Another Reverse Proxy) and custom transforms to rewrite paths and adapt responses, enabling compatibility with clients expecting Ollama's API.

## Features

- **Ollama API Compatibility:** Exposes endpoints like `/api/tags`, `/api/show`, `/api/version`, and `/v1/chat/completions`, rewriting and transforming requests/responses as needed.
- **Configurable Backend:** Forwards requests to a backend server, configurable via `appsettings.json`.
- **Custom Transforms:** Uses YARP's transform pipeline to rewrite paths and adapt JSON schemas for Ollama compatibility.
- **Logging:** Logs incoming requests, proxy destinations, and errors for easier debugging.
- **Solves [vscode-copilot-release#7518](https://github.com/microsoft/vscode-copilot-release/issues/7518#issuecomment-3051433965):** Enables Copilot and similar tools to interact with Ollama-compatible endpoints even when the backend differs.

## How to Run

1. **Build and run the project:**
   ```pwsh
   dotnet run --project src/OllamaYarpProject
   ```
2. **Access the proxy:**  
   By default, it listens on `http://localhost:11434` and forwards requests to `http://localhost:4000`.

## Configuration

- **Backend URL:**  
  Change the backend target in `appsettings.json` under the `ReverseProxy` section.
- **Logging:**  
  Logging is controlled via `appsettings.json` or environment variables.

## Endpoints

- `/api/tags` → `/models` (rewritten and response schema adapted)
- `/api/show` → Returns model info in Ollama format
- `/api/version` → Returns Ollama-compatible version info
- `/v1/chat/completions` → `/chat/completions` (rewritten)
- All other endpoints are proxied as-is

## Why?

This proxy allows tools (like Copilot) that expect Ollama's API to work with alternative backends, solving integration issues such as [vscode-copilot-release#7518](https://github.com/microsoft/vscode-copilot-release/issues/7518#issuecomment-3051433965).

---
