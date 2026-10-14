import { useEffect, useState } from 'react'
import type { FormEvent } from 'react'
import './App.css'

const api = 'http://localhost:5180/api/lobbies'
type Lobby = { id: string; name: string; region: string; status: 'Open' | 'InProgress'; maxPlayers: number; members: string[]; createdAtUtc: string }

function App() {
  const [lobbies, setLobbies] = useState<Lobby[]>([])
  const [name, setName] = useState('Northern Lights Crew')
  const [region, setRegion] = useState('EU-West')
  const [maxPlayers, setMaxPlayers] = useState(4)
  const [playerName, setPlayerName] = useState('Make4T')
  const [message, setMessage] = useState('')
  const [loading, setLoading] = useState(true)

  const loadLobbies = async () => {
    setLoading(true)
    try { const response = await fetch(api); if (!response.ok) throw new Error('Unable to load lobbies.'); setLobbies(await response.json()); setMessage('') }
    catch (error) { setMessage(error instanceof Error ? error.message : 'Connection failed.') }
    finally { setLoading(false) }
  }
  useEffect(() => { void loadLobbies() }, [])

  const createLobby = async (event: FormEvent) => {
    event.preventDefault()
    const response = await fetch(api, { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ name, region, maxPlayers }) })
    if (!response.ok) { setMessage('Check the lobby name and player limit.'); return }
    setMessage('Lobby created. Invite a player below.'); await loadLobbies()
  }
  const joinLobby = async (id: string) => {
    const response = await fetch(`${api}/${id}/members`, { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ playerName }) })
    if (!response.ok) { const data = await response.json().catch(() => null); setMessage(data?.message ?? 'Could not join this lobby.'); return }
    setMessage(`${playerName} joined the lobby.`); await loadLobbies()
  }
  const startLobby = async (id: string) => {
    const response = await fetch(`${api}/${id}/start`, { method: 'POST' })
    if (!response.ok) { const data = await response.json().catch(() => null); setMessage(data?.message ?? 'Lobby could not start.'); return }
    setMessage('Session started.'); await loadLobbies()
  }

  return <main><header><a className="brand" href="#top"><i /> SESSION<span>HUB</span></a><span className="api-status">● API CONNECTED</span></header><section className="hero" id="top"><div><p className="eyebrow">Full stack game services</p><h1>Find your squad.<br /><em>Start the run.</em></h1><p className="intro">A React and TypeScript client connected to an ASP.NET Core API with SQLite persistence. Create co-op lobbies, join sessions and launch when your party is ready.</p></div><div className="stack-card"><span>TECH STACK</span><b>React + TypeScript</b><b>ASP.NET Core + SQLite</b><small>REST API / CORS / EF Core</small></div></section><section className="workspace"><form onSubmit={createLobby} className="create-card"><p className="eyebrow">Create a session</p><h2>Open a new lobby</h2><label>Lobby name<input value={name} onChange={(event) => setName(event.target.value)} required minLength={3} maxLength={40} /></label><div className="form-row"><label>Region<select value={region} onChange={(event) => setRegion(event.target.value)}><option>EU-West</option><option>EU-North</option><option>US-East</option></select></label><label>Players<select value={maxPlayers} onChange={(event) => setMaxPlayers(Number(event.target.value))}>{[2,3,4,5,6,7,8].map(value => <option key={value}>{value}</option>)}</select></label></div><button>Create lobby <span>→</span></button></form><section className="lobby-list"><div className="list-heading"><div><p className="eyebrow">Live lobby browser</p><h2>Available sessions</h2></div><button className="refresh" onClick={() => void loadLobbies()}>Refresh</button></div><label className="player-name">Joining as<input value={playerName} onChange={(event) => setPlayerName(event.target.value)} maxLength={32} /></label>{message && <p className="message">{message}</p>}{loading ? <p className="empty">Loading sessions...</p> : lobbies.length === 0 ? <p className="empty">No active lobbies yet. Create the first one.</p> : <div className="cards">{lobbies.map(lobby => <article className="lobby" key={lobby.id}><div><span className={lobby.status === 'Open' ? 'status open' : 'status active'}>{lobby.status === 'Open' ? 'OPEN' : 'IN PROGRESS'}</span><h3>{lobby.name}</h3><p>{lobby.region} · {lobby.members.length}/{lobby.maxPlayers} players</p><div className="members">{lobby.members.length ? lobby.members.map(member => <span key={member}>{member[0]}</span>) : <small>Awaiting squad members</small>}</div></div><div className="actions">{lobby.status === 'Open' && <button onClick={() => void joinLobby(lobby.id)}>Join</button>}{lobby.status === 'Open' && <button className="secondary" onClick={() => void startLobby(lobby.id)}>Start</button>}</div></article>)}</div>}</section></section><footer>GAME SESSION HUB · React / TypeScript · ASP.NET Core · SQLite</footer></main>
}
export default App
