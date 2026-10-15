# Game Session Hub

A full stack portfolio project for creating and managing multiplayer game lobbies. It demonstrates a typed React client, a server-owned ruleset in ASP.NET Core, persistent SQLite data and an automated delivery workflow.

## Stack

- React and TypeScript client
- ASP.NET Core Minimal API
- Entity Framework Core and SQLite
- REST, CORS and server-side lobby validation
- Docker Compose development environment and GitHub Actions validation

## Features

- Create and browse game lobbies
- Join a lobby with duplicate-player and capacity protection
- Start a lobby after two players have joined
- Persistent local SQLite session data

## Architecture

```text
React + TypeScript client
        │ HTTP / JSON
        ▼
ASP.NET Core Minimal API
        │ EF Core
        ▼
SQLite lobby database
```

The API owns lobby validation and state transitions. This prevents clients from bypassing player limits, adding duplicate players or starting a session before two players have joined.

## API overview

| Method | Route | Purpose |
| --- | --- | --- |
| `GET` | `/health` | Service health check |
| `GET` | `/api/lobbies/` | List lobbies and members |
| `POST` | `/api/lobbies/` | Create an open lobby |
| `POST` | `/api/lobbies/{id}/members` | Join a lobby with a player name |
| `POST` | `/api/lobbies/{id}/start` | Start a lobby with at least two players |

## Run locally

Open two terminals.

```powershell
cd server
dotnet run
```

```powershell
cd client
npm.cmd install
npm.cmd run dev
```

The React client runs at `http://localhost:5173` and connects to the API at `http://localhost:5180`.

## Run with Docker

With Docker Desktop running, launch the whole stack from the repository root:

```powershell
docker compose up --build
```

Open `http://localhost:5173`. The API is available at `http://localhost:5180`, including the health endpoint at `/health`. Lobby data is stored in a named Docker volume between restarts. Stop the stack with `Ctrl+C`; use `docker compose down` to remove the containers.

## Validate the API

The integration test creates a lobby, joins two players and starts the session against an in-process test host with an isolated SQLite database.

```powershell
dotnet test tests/GameSessionHub.Api.Tests/GameSessionHub.Api.Tests.csproj
```

## Quality checks

GitHub Actions runs on every push and pull request to `main`:

- Client TypeScript production build
- Client lint check
- API Release build
- Lobby lifecycle integration test

## What this project demonstrates

- Designing a client/server boundary for multiplayer lobby state
- Validating business rules at the API layer instead of trusting the browser
- Persisting relational lobby and member data with Entity Framework Core
- Packaging a two-service application with Docker Compose
- Testing an ASP.NET Core Minimal API through its public HTTP endpoints
