# Game Session Hub

A full stack portfolio project for creating and managing multiplayer game lobbies.

## Stack

- React and TypeScript client
- ASP.NET Core Minimal API
- Entity Framework Core and SQLite
- REST, CORS and server-side lobby validation

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
