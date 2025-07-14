# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is an ASP.NET Core reverse proxy project that mimics the Ollama API while redirecting requests to a remote server. It uses YARP (Yet Another Reverse Proxy) to handle request forwarding and includes custom transforms to adapt different API schemas.

## Build and Run Commands

- **Build and run**: `dotnet run`
- **Build**: `dotnet build`
- **Run in development**: `dotnet run --launch-profile "Development"`

The application listens on http://localhost:11434 by default and forwards requests to http://localhost:4000.

## Architecture

### Core Components

- **Program.cs**: Main application entry point, configures YARP reverse proxy and logging middleware
- **StandardTransform.cs**: Custom YARP transform provider that handles API endpoint mapping and response transformation
- **ModelData.cs**: Data models for JSON serialization/deserialization between different API schemas

### Key Features

1. **API Translation**: Converts between different API schemas (Ollama ↔ remote server)
2. **Endpoint Mapping**: 
   - `/api/tags` → `/models`
   - `/v1/chat/completions` → `/chat/completions`
   - `/api/show` → returns mock model information
   - `/api/version` → returns static version info
3. **Response Transformation**: Transforms `/models` responses from source schema to Ollama schema
4. **Request Logging**: Comprehensive logging of all proxy requests and responses

### Configuration

- **appsettings.json**: Contains YARP configuration, logging levels, and Kestrel settings
- **ReverseProxy section**: Defines routes and clusters for request forwarding
- **Kestrel section**: Configures server limits and endpoints

### Data Flow

1. Client makes request to proxy (port 11434)
2. StandardTransform processes request (path rewriting, body modification)
3. YARP forwards to destination server (port 4000)
4. StandardTransform processes response (schema transformation)
5. Response returned to client

## Dependencies

- **.NET 9.0**: Target framework
- **Yarp.ReverseProxy 2.3.0**: Core reverse proxy functionality
- **Newtonsoft.Json 13.0.3**: JSON serialization/deserialization