# Game Session Hub

A full stack portfolio project for creating and managing multiplayer game lobbies.

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
