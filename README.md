# TailorBlend AI Consultant - Frontend UI

**Blazor Server (.NET 8) + MudBlazor**

Standalone frontend web application for the TailorBlend supplement consultation platform. Communicates with the Python FastAPI backend via HTTP/SSE for real-time consultation sessions.

## Overview

This is the **independent frontend UI** that was split from the monorepo. It runs separately from the Python backend and connects via HTTP.

- **Framework**: Blazor Server (.NET 8)
- **UI Library**: MudBlazor (Material Design)
- **Real-time**: SignalR for UI updates + SSE for streaming
- **Backend API**: Python FastAPI (separate repository)
- **Port**: 8080 (both local dev and production)

## Architecture

```
Browser (http://localhost:8080)
    ↓ SignalR (UI updates)
    ↓ HTTP GET/POST (chat requests)
Blazor Server (this repo)
    ↓ HTTP SSE
Python FastAPI Backend (separate repo)
    ↓
OpenAI API
```

## Quick Start

### 1. Prerequisites

- .NET 8 SDK: https://dotnet.microsoft.com/en-us/download/dotnet/8.0
- Python backend running (see `tailorblend-backend-api` repo)

### 2. Installation

```bash
# Clone repository
git clone <frontend-repo-url>
cd tailorblend-frontend-ui

# Restore dependencies
dotnet restore BlazorConsultant/BlazorConsultant.csproj
```

### 3. Configuration

```bash
# The app uses appsettings.json (production) and
# appsettings.Development.json (local dev)
#
# Local dev: Points to http://localhost:5000 (backend)
# Production: Points to https://api.tailorblend.com (backend)
```

### 4. Run Locally

```bash
# Make sure Python backend is running on localhost:5000
# Then start the Blazor app
dotnet run --project BlazorConsultant

# Open browser: http://localhost:8080
```

### 5. Verify Connection

- Navigate to http://localhost:8080/chat
- Type a message in the chat
- Should receive response from Python backend
- Check browser console for any CORS or connection errors

## Project Structure

```
tailorblend-frontend-ui/
├── BlazorConsultant/           # Main Blazor project
│   ├── Pages/
│   │   ├── Chat.razor          # Main chat interface
│   │   ├── Index.razor         # Landing page
│   │   └── Configuration.razor # System instructions editor
│   ├── Services/
│   │   ├── ChatService.cs      # Python API communication
│   │   ├── SessionService.cs   # Client-side session
│   │   └── InstructionService.cs # Config management
│   ├── Components/
│   │   ├── ChatMessageComponent.razor   # Message renderer
│   │   └── SessionStatsComponent.razor  # Token usage display
│   ├── Models/                 # Request/response models
│   ├── Shared/
│   │   └── MainLayout.razor    # Page layout
│   ├── wwwroot/
│   │   ├── css/tailorblend.css # Custom styling
│   │   └── js/chat.js          # Client-side logic
│   ├── Program.cs              # App startup
│   ├── appsettings.json        # Production config
│   ├── appsettings.Development.json  # Dev config
│   └── BlazorConsultant.csproj # Project file
├── Dockerfile                  # Production image
├── fly.toml                    # fly.io deployment config
└── README.md                   # This file
```

## Configuration

### appsettings.json (Production)

```json
{
  "PythonApi": {
    "BaseUrl": "https://api.tailorblend.com"
  },
  "OAuth": {
    "Authority": "https://tailorblend-oauth.fly.dev",
    "ClientId": "blazor-consultant",
    "ClientSecret": "..."
  }
}
```

### appsettings.Development.json (Local Dev)

```json
{
  "PythonApi": {
    "BaseUrl": "http://localhost:5000"
  }
}
```

### Backend API URL Override

Set environment variable to override:

```bash
# Override backend URL at runtime
export PythonApi__BaseUrl=http://backend.local:5000
dotnet run --project BlazorConsultant
```

## API Integration

### ChatService.cs

Communicates with Python backend endpoints:

```csharp
// Stream chat with real-time tokens
await foreach (var token in chatService.StreamChatAsync(message))
{
    // Update UI with token
}

// Get session stats
var stats = await chatService.GetSessionStatsAsync();

// Reset conversation
await chatService.ResetSessionAsync();
```

### Request/Response

**POST /api/chat/stream**

```json
{
  "message": "I'm always tired",
  "session_id": "abc123",
  "custom_instructions": null,
  "model": "gpt-4.1-mini-2025-04-14",
  "attachments": [],
  "practitioner_mode": false
}
```

**Response (SSE Stream)**

```
data: "I'd"
data: " be"
data: " happy"
data: " to help"
data: " you"
data: " find"
data: " the"
data: " right"
data: " blend"
data: "!"
data: "[DONE]"
```

## Deployment

### Deploy to fly.io

```bash
# 1. Set backend API URL
fly secrets set PythonApi__BaseUrl=https://api.tailorblend.com

# 2. Set OAuth (if enabled)
fly secrets set OAuth__ClientSecret=your-secret

# 3. Deploy
fly deploy

# 4. Monitor
fly status
fly logs
```

### Deploy to Railway

```bash
# 1. Connect GitHub repository
# 2. Set environment variables in Railway dashboard:
#    - PythonApi__BaseUrl=https://api.tailorblend.com
#    - OAuth__ClientSecret=your-secret
# 3. Railway auto-deploys on git push
```

### Deploy to Google Cloud Run

```bash
# Build image
gcloud builds submit --tag gcr.io/PROJECT_ID/tailorblend-ui

# Deploy
gcloud run deploy tailorblend-ui \
  --image gcr.io/PROJECT_ID/tailorblend-ui \
  --set-env-vars="PythonApi__BaseUrl=https://api.tailorblend.com"
```

## Development

### Local Setup

```bash
# Install .NET 8
# macOS: brew install dotnet@8
# Linux/Windows: Download from https://dotnet.microsoft.com

# Clone this repo
git clone <url>
cd tailorblend-frontend-ui

# Install dependencies
dotnet restore BlazorConsultant

# Start Python backend in another terminal
cd ../tailorblend-backend-api
python -m backend.api

# Start Blazor frontend
cd ../tailorblend-frontend-ui
dotnet run --project BlazorConsultant

# Open http://localhost:8080
```

### Hot Reload

Blazor Server supports hot reload in development:

```bash
dotnet watch run --project BlazorConsultant
```

Changes to `.razor` and `.cs` files reload automatically.

### Debugging

```bash
# Run with verbose logging
dotnet run --project BlazorConsultant --verbosity diagnostic

# View logs
cat ~/.dotnet/debug.log
```

## Pages & Components

### Chat.razor (Main)

- Real-time chat interface
- Message rendering with markdown
- File upload support
- Session statistics display
- Reset conversation button

### Configuration.razor

- Edit system instructions
- Test prompts
- Settings panel

### ChatService.cs

- Communicates with Python backend
- Handles SSE streaming
- Manages sessions
- Tracks token usage

## Testing Scenarios

### Scenario 1: Basic Chat

```
User: "I'm tired all the time"
Expected: Agent asks follow-up questions, eventually recommends energy blend
```

### Scenario 2: File Upload

```
User: Uploads medical report + "I have these conditions"
Expected: Agent analyzes file and recommends appropriate blend
```

### Scenario 3: Multi-Agent Mode

```
User: Sends complex health request
Backend: Uses 2-agent system for formulation
Expected: Detailed blend with ingredients and dosages
```

## Troubleshooting

### "Connection refused" to backend

```bash
# Check backend is running
curl http://localhost:5000/api/health

# Check frontend config
cat BlazorConsultant/appsettings.Development.json | grep PythonApi

# Update config if needed
export PythonApi__BaseUrl=http://backend.local:5000
dotnet run --project BlazorConsultant
```

### CORS errors in browser console

```
Access to XMLHttpRequest blocked by CORS policy
```

Solution:
- Verify backend CORS allows frontend origin
- Check backend has `CORS_ALLOWED_ORIGINS` set correctly
- Restart backend after changing CORS config

### Slow responses / timeout

```bash
# Check backend logs
tail -f ../tailorblend-backend-api/logs/api.log

# Check OpenAI API status
curl https://status.openai.com

# Increase timeout in ChatService.cs if needed
```

### Build errors

```bash
# Clean build
dotnet clean BlazorConsultant
dotnet restore BlazorConsultant
dotnet build BlazorConsultant

# Check .NET version
dotnet --version  # Should be 8.0.x
```

## Features

### Real-Time Streaming

- Token-by-token display for natural feel
- SSE (Server-Sent Events) for efficient streaming
- No WebSocket overhead (uses HTTP)

### Session Management

- Client-side session ID generation
- Server-side conversation context
- Previous response tracking for continuity
- Reset button to clear history

### File Attachments

- Upload medical documents (PDF, images)
- Automatic MIME type detection
- Base64 encoding for transmission
- Server-side file handling

### Practitioner Mode

- Clinical-grade instructions
- Drug interaction analysis
- Evidence-based recommendations
- Professional formulation guidance

### Cost Tracking

- Token counting (input/output)
- ZAR (South African Rand) cost calculation
- Real-time display during consultation

## Security

- No secrets in code (use environment variables)
- HTTPS enforced in production
- CORS restricted to authorized origins
- OAuth support (currently optional)
- Non-root user in Docker

## Performance

- Minimal dependencies (MudBlazor, Markdig)
- Lazy component loading
- Virtual scrolling for long chats
- Client-side session management
- No database required

## Contributing

When modifying the frontend:

1. **UI changes**: Edit `.razor` files
2. **API integration**: Update `ChatService.cs`
3. **Styling**: Edit `wwwroot/css/tailorblend.css`
4. **Dependencies**: Update `.csproj` or run `dotnet add package`

## Related Repositories

- **Backend API**: https://github.com/tailorblend/backend-api
- **Main Project**: https://github.com/tailorblend/tailorblend

## Support

For issues or questions:

1. Check this README's Troubleshooting section
2. Verify backend is running and accessible
3. Check environment variables are set correctly
4. Review browser console for error messages
5. File an issue in the repository

## License

See main TailorBlend project license.
