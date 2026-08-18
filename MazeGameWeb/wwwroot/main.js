
    "use strict";

    /* ============================================================
       Enum maps (backend serializes enums as integers)
       PlayerType: 0=Hider, 1=Seeker
       GameStatus: 0=WaitingForPlayer, 1=Active, 2=Completed
       Direction:  0=North, 1=South, 2=West, 3=East
       Cell:       0=Empty, 1=Wall
       ============================================================ */
    const PlayerType = {0: "hider", 1: "seeker" };
    const GameStatus = {0: "waiting-for-player", 1: "active", 2: "completed" };
    const CELL_WALL = 1;

    const DIR = {NORTH: 0, SOUTH: 1, WEST: 2, EAST: 3 };
    // Movement geometry must match backend Move.cs:
    //   Rows are vertical (North -> row-1, South -> row+1)
    //   Columns are horizontal (West -> column-1, East -> column+1)
    function applyDirection(pos, dir) {
    switch (dir) {
        case DIR.NORTH: return {row: pos.row - 1, column: pos.column };
    case DIR.SOUTH: return {row: pos.row + 1, column: pos.column };
    case DIR.WEST:  return {row: pos.row, column: pos.column - 1 };
    case DIR.EAST:  return {row: pos.row, column: pos.column + 1 };
    }
    return {...pos};
}

    const STORAGE_KEY = "hideAndSeekGame";
    const AUTH_USERNAME_KEY = "hideAndSeekUsername";

    /* ============================================================
       State
       ============================================================ */
    const state = {
        gameId: null,
    playerToken: null,
    role: null,           // "hider" | "seeker"
    gameStatus: null,     // "waiting-for-player" | "active" | "completed"
    maze: null,           // number[][]  (maze[row][col])
    yourPosition: null,   // {row, column}
    opponentPosition: null,
    currentPlayer: null,  // "hider" | "seeker"
    turnNumber: 0,
    movesUntilReveal: null,
    winner: null,         // "hider" | "seeker" | null
    connected: false,
    moveInProgress: false,
};


    /* ============================================================
       API utility layer
       ============================================================ */
    async function apiRequest(path, body) {
    const res = await fetch(path, {
        method: "POST",
    headers: {"Content-Type": "application/json" },
    credentials: "same-origin",
    body: JSON.stringify(body || { }),
    });

    return handleApiResponse(res);
}

    async function apiGetRequest(path, queryParams) {
    const query = new URLSearchParams(queryParams || { }).toString();
    const res = await fetch(query ? `${path}?${query}` : path, {
        method: "GET",
    credentials: "same-origin",
    });

    return handleApiResponse(res);
}

    async function handleApiResponse(res) {
        let data = null;
    const text = await res.text();
    if (text) {
        try {data = JSON.parse(text); } catch {data = null; }
    }

    if (!res.ok) {
        const err = new Error((data && data.error) || `Request failed (${res.status})`);
    err.status = res.status;
    err.data = data;
    throw err;
    }
    return data;
}

    function createGame() {
    return apiRequest("/create", { });
}
    function joinGame(gameId) {
    return apiRequest("/join", {gameId});
}
    function pollGame() {
    return apiGetRequest("/poll", {gameId: state.gameId, playerToken: state.playerToken });
}
    function move(direction) {
    return apiRequest("/move", {
        gameId: state.gameId,
    playerToken: state.playerToken,
    direction,
    });
}

    function registerUser(username, password) {
    return apiRequest("/auth/register", {username, password});
}
    function fetchMyGames() {
    return apiGetRequest("/me/games");
}
    function loginUser(username, password) {
    return apiRequest("/auth/login", {username, password});
}
    function logoutUser() {
    return apiRequest("/auth/logout", { });
}
    function fetchCurrentUser() {
    return apiGetRequest("/auth/me");
}

    /* ============================================================
       Persistence
       ============================================================ */
    function saveGame() {
        localStorage.setItem(STORAGE_KEY, JSON.stringify({
            gameId: state.gameId,
            playerToken: state.playerToken,
            role: state.role,
            gameStatus: state.gameStatus,
        }));
}
    function loadSavedGame() {
    try { return JSON.parse(localStorage.getItem(STORAGE_KEY)); }
    catch { return null; }
}
    function clearSavedGame() {
        localStorage.removeItem(STORAGE_KEY);
}

    /* ============================================================
       Screen management
       ============================================================ */
    function showScreen(id) {
        document.querySelectorAll(".screen").forEach(s => s.classList.remove("active"));
    document.getElementById(id).classList.add("active");

    // Keep the open-games list fresh only while the start screen is visible.
    if (id === "screen-start") {
        startWaitingPolling();
    } else {
        stopWaitingPolling();
    }
}

    /* ============================================================
       Toasts & banners
       ============================================================ */
    function toast(msg) {
    const el = document.createElement("div");
    el.className = "toast";
    el.textContent = msg;
    document.getElementById("toasts").appendChild(el);
    setTimeout(() => el.remove(), 2500);
}
    function showGameError(msg) {
    const b = document.getElementById("game-error");
    b.textContent = msg;
    b.classList.remove("hidden");
}
    function clearGameError() {
        document.getElementById("game-error").classList.add("hidden");
}

   /* ============================================================
      Polling socket
      ============================================================ */

    function peerIdFor(gameId, role) {
        // role: "Hider" or "Seeker"
        return `maze-${gameId}-${role.toLowerCase()}`;
    }

    function otherRole(role) {
        return role === "hider" ? "seeker" : "hider";
}

let peer = null;
let peerConn = null;

function wireConnection(conn) {
    if (peerConn && peerConn.open) {
        // Already have a live connection; ignore a duplicate.
        return;
    }
    peerConn = conn;
    conn.on("open", () => {
        doPoll(); // opponent is now connected - resync immediately (e.g. lobby -> active)
    });
    conn.on("data", (msg) => {
        if (msg === "move") doPoll();
    });
    conn.on("close", () => {
        if (peerConn === conn) peerConn = null;
    });
}

function setupPeerConnection() {
    if (peer || !state.gameId || !state.role) return;

    const myPeerId = peerIdFor(state.gameId, state.role);
    const remotePeerId = peerIdFor(state.gameId, otherRole(state.role));

    peer = new Peer(myPeerId);

    peer.on("open", () => {
        wireConnection(peer.connect(remotePeerId));
    });
    peer.on("connection", (conn) => wireConnection(conn));
    peer.on("error", (err) => {
        // e.g. "peer-unavailable" if the opponent hasn't opened their peer yet;
        // harmless - they will connect to us once ready.
        console.warn("PeerJS error:", err.type);
    });
}

function notifyOpponentOfMove() {
    if (peerConn && peerConn.open) {
        peerConn.send("move");
    }
}




    


    /* ============================================================
       Polling loop (single interval, no overlap)
       ============================================================ */


    let pollInFlight = false;
    async function doPoll() {
    if (pollInFlight) return;
    pollInFlight = true;
    try {
        const data = await pollGame();
    state.connected = true;
    clearGameError();
    applyPollResponse(data);
    } catch (err) {
        state.connected = false;
    setConnection(false);
    showGameError("Connection lost. Retrying...");
    } finally {
        pollInFlight = false;
    }
}

    let gamePollTimer = null;
    function startGamePolling() {
        if (gamePollTimer) return;
        gamePollTimer = setInterval(() => {
            if (state.gameId && state.gameStatus !== "completed") {
                doPoll();
            }
        }, 1000);
    }
    function stopGamePolling() {
        if (gamePollTimer) {
            clearInterval(gamePollTimer);
            gamePollTimer = null;
        }
    }
    startGamePolling();

    function applyPollResponse(data) {
        // Normalize enum ints -> strings
        state.gameStatus = GameStatus[data.status];

    if (state.gameStatus === "waiting-for-player") {
        renderLobby();
    return;
    }

    if (state.gameStatus === "completed") {
        state.winner = data.winner != null ? PlayerType[data.winner] : state.winner;
    if (data.maze) state.maze = data.maze;
    state.yourPosition = data.yourPosition || state.yourPosition || null;
    state.opponentPosition = data.opponentPosition || state.opponentPosition || null;
    saveGame();
    setConnection(true);
    renderGameOver();
    return;
    }

    // active
    if (data.maze) state.maze = data.maze;
    state.yourPosition = data.yourPosition || null;
    state.opponentPosition = data.opponentPosition || null;
    state.currentPlayer = PlayerType[data.currentPlayer];
    state.turnNumber = data.turnNumber;
    state.movesUntilReveal = data.movesUntilReveal;
    state.winner = data.winner != null ? PlayerType[data.winner] : null;

    saveGame();

    if (document.getElementById("screen-game").classList.contains("active") === false) {
        showScreen("screen-game");
    }
    setConnection(true);
    renderGame();
}

    /* ============================================================
       Lobby rendering
       ============================================================ */
    function renderLobby() {
        showScreen("screen-lobby");
    document.getElementById("lobby-gameid").textContent = state.gameId ?? "—";
    document.getElementById("lobby-role").textContent = state.role ?? "—";
    document.getElementById("lobby-status").textContent = "Waiting for player";
}

    /* ============================================================
       Open games (join online game)
       ============================================================ */
    function shortId(id) {
        const s = String(id);
    return s.length > 8 ? s.slice(0, 8) + "…" : s;
}

    async function listWaitingGames() {
        try {
        const games = await apiGetRequest("/games/waiting");
    renderWaitingGames(games);
        } catch {
        toast("Unable to load open games.");
        }
}

    function renderWaitingGames(games) {
        const list = document.getElementById("waiting-games-list");
    const empty = document.getElementById("waiting-empty");
    list.innerHTML = "";

    const open = Array.isArray(games) ? games.filter(g => g && g.gameId != null) : [];
    if (open.length === 0) {
        empty.classList.remove("hidden");
    return;
    }
    empty.classList.add("hidden");

    for (const g of open) {
        const id = String(g.gameId);
    const li = document.createElement("li");
    li.className = "row";
    li.style.justifyContent = "space-between";
    li.style.padding = "8px 0";

    const label = document.createElement("span");
    label.className = "chip";
    label.innerHTML = `Game: <strong>${shortId(id)}</strong>`;

    const btn = document.createElement("button");
    btn.className = "accent";
    btn.textContent = "Join";
    btn.addEventListener("click", () => joinWaitingGame(id, btn));

    li.appendChild(label);
    li.appendChild(btn);
    list.appendChild(li);
    }
}

    async function joinWaitingGame(gameId, btn) {
        if (btn) btn.disabled = true;
    try {
        const data = await joinGame(gameId);
    state.gameId = String(data.gameId);
    state.playerToken = String(data.playerToken);
    state.role = PlayerType[data.role];
    state.gameStatus = GameStatus[data.status];
    saveGame();
    doPoll();
    setupPeerConnection();
    if (state.gameStatus === "active") {
        showScreen("screen-game");
        } else {
        renderLobby();
        }
    } catch (err) {
        if (err.status === 404) {
        toast("That game is no longer available.");
    listWaitingGames();
        } else if (err.status === 409) {
        toast((err.data && err.data.error) || "That game is already full or started.");
    listWaitingGames();
        } else {
        toast("Unable to join game. Please try again.");
        }
    } finally {
        if (btn) btn.disabled = false;
    }
}

    let waitingPollTimer = null;
    function startWaitingPolling() {
        listWaitingGames();
    if (waitingPollTimer) return;
    waitingPollTimer = setInterval(listWaitingGames, 4000);
}
    function stopWaitingPolling() {
        if (waitingPollTimer) {
        clearInterval(waitingPollTimer);
    waitingPollTimer = null;
    }
}

    /* ============================================================
       Profile (my games)
       ============================================================ */
    async function listMyGames() {
        const btn = document.getElementById("btn-refresh-profile");
    const spin = document.getElementById("profile-spin");
    setButtonLoading(btn, spin, true);
    try {
        const games = await fetchMyGames();
    renderMyGames(games);
    } catch {
        toast("Unable to load your games.");
    } finally {
        setButtonLoading(btn, spin, false);
    }
}

    function renderMyGames(games) {
        const list = document.getElementById("profile-games-list");
    const empty = document.getElementById("profile-empty");
    list.innerHTML = "";

    const myGames = Array.isArray(games) ? games : [];
    if (myGames.length === 0) {
        empty.classList.remove("hidden");
    return;
    }
    empty.classList.add("hidden");

    for (const g of myGames) {
        const li = document.createElement("li");
    li.className = "row";
    li.style.justifyContent = "space-between";
    li.style.padding = "8px 0";

    const role = PlayerType[g.role] ?? "—";
    const status = GameStatus[g.gameStatus] ?? "—";
    const winner = (g.winner === null || g.winner === undefined) ? "—" : PlayerType[g.winner];
    const updated = g.updatedAt ? new Date(g.updatedAt).toLocaleString() : "—";

    const label = document.createElement("span");
    label.className = "chip";
    label.innerHTML = `Game: <strong>${shortId(g.gameId)}</strong> · Role: <strong>${role}</strong> · Status: <strong>${status}</strong> · Winner: <strong>${winner}</strong> · Updated: <strong>${updated}</strong>`;

    li.appendChild(label);

    if (g.playerToken) {
        const actionBtn = document.createElement("button");
        actionBtn.className = "secondary";
        actionBtn.textContent = status === "completed" ? "View Final State" : "Join";
        actionBtn.addEventListener("click", () => viewOrJoinGame(g));
        li.appendChild(actionBtn);
    }

    list.appendChild(li);
    }
}

    function viewOrJoinGame(g) {
        state.gameId = String(g.gameId);
        state.playerToken = String(g.playerToken);
        state.role = PlayerType[g.role];
        state.gameStatus = GameStatus[g.gameStatus];
        state.winner = (g.winner === null || g.winner === undefined) ? null : PlayerType[g.winner];
        doPoll();
        if (state.gameStatus !== "completed") {
            setupPeerConnection();
        }
    }

    /* ============================================================
       Game rendering
       ============================================================ */
    function isMyTurn() {
    return state.gameStatus === "active" && state.currentPlayer === state.role && !state.moveInProgress;
}

    function setConnection(ok) {
    const el = document.getElementById("hdr-conn");
    el.innerHTML = ok
    ? '<strong class="conn-ok">🟢 Connected</strong>'
    : '<strong class="conn-bad">🔴 Disconnected</strong>';
}

    function cellIsWall(row, col) {
    if (!state.maze) return false;
    if (row < 0 || col < 0) return true;
    if (row >= state.maze.length || col >= state.maze[row].length) return true;
    return state.maze[row][col] === CELL_WALL;
}

    function isValidMove(dir) {
    if (!state.yourPosition) return false;
    const t = applyDirection(state.yourPosition, dir);
    return !cellIsWall(t.row, t.column);
}

    function renderGame() {
        document.getElementById("hdr-gameid").textContent = state.gameId ?? "—";
    document.getElementById("hdr-role").textContent = state.role ?? "—";
    document.getElementById("hdr-status").textContent = state.gameStatus ?? "—";

    document.getElementById("st-role").textContent = state.role ?? "—";
    document.getElementById("st-current").textContent = state.currentPlayer ?? "—";
    document.getElementById("st-turn").textContent = state.turnNumber ?? "—";
    document.getElementById("st-reveal").textContent =
    state.movesUntilReveal != null ? `${state.movesUntilReveal} turn(s)` : "—";
    document.getElementById("st-status").textContent = state.gameStatus ?? "—";

    // Turn banner
    const banner = document.getElementById("turn-banner");
    if (isMyTurn()) {
        banner.textContent = "Your turn";
    banner.className = "turn-banner mine";
    } else {
        banner.textContent = "Waiting for opponent...";
    banner.className = "turn-banner theirs";
    }

    // Opponent visibility note
    document.getElementById("opp-note").textContent = state.opponentPosition
    ? "Opponent revealed!"
    : "Opponent hidden until reveal turn.";

    renderMaze();
    updateControls();
}

    function renderMaze() {
    const grid = document.getElementById("maze");
    if (!state.maze) {
        grid.innerHTML = "";
    grid.style.setProperty("--rows", 20);
    grid.style.setProperty("--cols", 20);
    return;
    }

    const rows = state.maze.length;
    const cols = state.maze[0].length;
    grid.style.setProperty("--rows", rows);
    grid.style.setProperty("--cols", cols);

    // Available moves (only on my turn)
    const moves = new Set();
    if (isMyTurn() && state.yourPosition) {
        for (const dir of [DIR.NORTH, DIR.SOUTH, DIR.WEST, DIR.EAST]) {
            if (isValidMove(dir)) {
                const t = applyDirection(state.yourPosition, dir);
    moves.add(t.row + "," + t.column);
            }
        }
    }

    const me = state.yourPosition;
    const opp = state.opponentPosition;

    const frag = document.createDocumentFragment();
    for (let r = 0; r < rows; r++) {
        for (let c = 0; c < cols; c++) {
            const cell = document.createElement("div");
    cell.className = "cell";
    if (state.maze[r][c] === CELL_WALL) cell.classList.add("wall");

    if (me && me.row === r && me.column === c) {
        cell.classList.add("me");
            } else if (opp && opp.row === r && opp.column === c) {
        cell.classList.add("opp");
    if (moves.has(r + "," + c)) {
        cell.classList.add("move");
    cell.title = "Move here";
    cell.dataset.row = r;
    cell.dataset.col = c;
                }
            } else if (moves.has(r + "," + c)) {
        cell.classList.add("move");
    cell.title = "Move here";
    cell.dataset.row = r;
    cell.dataset.col = c;
            }
    frag.appendChild(cell);
        }
    }
    grid.innerHTML = "";
    grid.appendChild(frag);
}

    function updateControls() {
        // No-op: movement controls are keyboard/click-only (d-pad removed).
    }

/* ============================================================
    Movement
    ============================================================ */
    async function submitMove(dir) {
    if (!isMyTurn() || !isValidMove(dir)) return;

    state.moveInProgress = true;
    updateControls();
    try {
        await move(dir);
        toast("Move submitted.");
        notifyOpponentOfMove();
    } catch (err) {
        handleMoveError(err);
    } finally {
        state.moveInProgress = false;
    // Immediately resync with backend
    doPoll();
    }
}

    function handleMoveError(err) {
        let msg;
    switch (err.status) {
        case 400: msg = "Invalid or illegal move."; break;
    case 401: msg = "Your player token is invalid. Please rejoin the game."; break;
    case 403: msg = "It is not your turn."; break;
    case 404: msg = "Game not found."; break;
    case 409: msg = "This game has already completed."; break;
    default:  msg = "Unable to submit move. Please try again.";
    }
    showGameError(msg);
    toast(msg);
}

    /* ============================================================
       Game over
       ============================================================ */
    function renderGameOver() {
        showScreen("screen-game");
    document.getElementById("hdr-status").textContent = "completed";
    document.getElementById("st-status").textContent = "completed";
    const banner = document.getElementById("turn-banner");
    banner.textContent = "Game over";
    banner.className = "turn-banner theirs";
    updateControls();
    renderMaze();

    const winner = state.winner;
    document.getElementById("modal-winner").textContent =
    "Winner: " + (winner ? winner.charAt(0).toUpperCase() + winner.slice(1) : "—");

    const outcome = document.getElementById("modal-outcome");
    if (winner === state.role) {
        outcome.textContent = "You won!";
    outcome.className = "result win";
    } else {
        outcome.textContent = "You lost.";
    outcome.className = "result lose";
    }
    document.getElementById("modal").classList.add("active");
}

    /* ============================================================
       Reset to start
       ============================================================ */
    function returnToStart() {
    document.getElementById("modal").classList.remove("active");
    Object.assign(state, {
        gameId: null, playerToken: null, role: null, gameStatus: null,
    maze: null, yourPosition: null, opponentPosition: null,
    currentPlayer: null, turnNumber: 0, movesUntilReveal: null,
    winner: null, connected: false, moveInProgress: false,
    });
    showScreen("screen-start");
    checkRecovery();
}

    /* ============================================================
       Recovery panel
       ============================================================ */
    function checkRecovery() {
    const saved = loadSavedGame();
    const panel = document.getElementById("recovery");
    if (saved && saved.gameId && saved.playerToken) {
        panel.classList.remove("hidden");
    } else {
        panel.classList.add("hidden");
    }
}

    /* ============================================================
       Event wiring
       ============================================================ */
    function setButtonLoading(btn, spin, loading, label) {
        btn.disabled = loading;
    spin.classList.toggle("hidden", !loading);
}

function enterApp(username) {
    localStorage.setItem(AUTH_USERNAME_KEY, username);
    document.getElementById("start-username").textContent = username;
    showScreen("screen-start");
    checkRecovery();
}

document.getElementById("btn-show-register").addEventListener("click", () => {
    document.getElementById("register-error").textContent = "";
    showScreen("screen-register");
});

document.getElementById("btn-show-login").addEventListener("click", () => {
    document.getElementById("login-error").textContent = "";
    showScreen("screen-login");
});

document.getElementById("btn-register").addEventListener("click", async () => {
    const usernameEl = document.getElementById("register-username");
    const passwordEl = document.getElementById("register-password");
    const errEl = document.getElementById("register-error");
    const btn = document.getElementById("btn-register");
    const spin = document.getElementById("register-spin");
    errEl.textContent = "";

    const username = usernameEl.value.trim();
    const password = passwordEl.value;
    if (!username || !password) {
        errEl.textContent = "Please enter a username and password.";
        return;
    }

    setButtonLoading(btn, spin, true);
    try {
        await registerUser(username, password);
        await loginUser(username, password);
        passwordEl.value = "";
        enterApp(username);
    } catch (err) {
        errEl.textContent = (err.data && err.data.error) || "Unable to create account. Please try again.";
    } finally {
        setButtonLoading(btn, spin, false);
    }
});

document.getElementById("btn-login").addEventListener("click", async () => {
    const usernameEl = document.getElementById("login-username");
    const passwordEl = document.getElementById("login-password");
    const errEl = document.getElementById("login-error");
    const btn = document.getElementById("btn-login");
    const spin = document.getElementById("login-spin");
    errEl.textContent = "";

    const username = usernameEl.value.trim();
    const password = passwordEl.value;
    if (!username || !password) {
        errEl.textContent = "Please enter your username and password.";
        return;
    }

    setButtonLoading(btn, spin, true);
    try {
        const data = await loginUser(username, password);
        passwordEl.value = "";
        enterApp(data.username || username);
    } catch (err) {
        errEl.textContent = err.status === 401
            ? "Invalid username or password."
            : (err.data && err.data.error) || "Unable to log in. Please try again.";
    } finally {
        setButtonLoading(btn, spin, false);
    }
});

document.getElementById("login-password").addEventListener("keydown", (e) => {
    if (e.key === "Enter") document.getElementById("btn-login").click();
});

document.getElementById("register-password").addEventListener("keydown", (e) => {
    if (e.key === "Enter") document.getElementById("btn-register").click();
});

document.getElementById("btn-logout").addEventListener("click", async () => {
    try { await logoutUser(); } catch { /* ignore, still clear client state */ }
    localStorage.removeItem(AUTH_USERNAME_KEY);
    showScreen("screen-login");
});

document.getElementById("btn-profile").addEventListener("click", () => {
    showScreen("screen-profile");
    listMyGames();
});

document.getElementById("btn-refresh-profile").addEventListener("click", listMyGames);

document.getElementById("btn-profile-back").addEventListener("click", () => {
    showScreen("screen-start");
});

document.getElementById("btn-create").addEventListener("click", async () => {
    const btn = document.getElementById("btn-create");
    const spin = document.getElementById("create-spin");
    setButtonLoading(btn, spin, true);
    try {
        const data = await createGame();
    state.gameId = String(data.gameId);
    state.playerToken = String(data.playerToken);
    state.role = PlayerType[data.role];
    state.gameStatus = GameStatus[data.status];
    saveGame();
    renderLobby();
        doPoll();
        setupPeerConnection();
    } catch {
        toast("Unable to create game. Please try again.");
    } finally {
        setButtonLoading(btn, spin, false);
    }
});

document.getElementById("btn-join").addEventListener("click", async () => {
    const input = document.getElementById("join-id");
    const errEl = document.getElementById("join-error");
    const btn = document.getElementById("btn-join");
    const spin = document.getElementById("join-spin");
    errEl.textContent = "";

    const gameId = input.value.trim();
    if (!gameId) {
        errEl.textContent = "Please enter a Game ID.";
    return;
    }

    setButtonLoading(btn, spin, true);
    try {
        const data = await joinGame(gameId);
    state.gameId = String(data.gameId);
    state.playerToken = String(data.playerToken);
    state.role = PlayerType[data.role];
    state.gameStatus = GameStatus[data.status];
    saveGame();
        doPoll();
        setupPeerConnection();
    if (state.gameStatus === "active") {
        showScreen("screen-game");
        } else {
        renderLobby();
        }
    } catch (err) {
        if (err.status === 404) {
        errEl.textContent = "Game not found. Please check the Game ID and try again.";
        } else if (err.status === 409) {
        errEl.textContent = (err.data && err.data.error)
            ? err.data.error
            : "This game is already full, active, or completed.";
        } else {
        errEl.textContent = "Unable to join game. Please try again.";
        }
    } finally {
        setButtonLoading(btn, spin, false);
    }
});

document.getElementById("join-id").addEventListener("keydown", (e) => {
    if (e.key === "Enter") document.getElementById("btn-join").click();
});

document.getElementById("btn-copy-lobby").addEventListener("click", async () => {
    try {
        await navigator.clipboard.writeText(state.gameId || "");
    toast("Game ID copied.");
    } catch {
        toast("Copy failed.");
    }
});

    document.getElementById("btn-leave-lobby").addEventListener("click", returnToStart);

document.getElementById("btn-refresh-waiting").addEventListener("click", async () => {
    const btn = document.getElementById("btn-refresh-waiting");
    const spin = document.getElementById("waiting-spin");
    setButtonLoading(btn, spin, true);
    try {
        await listWaitingGames();
    } finally {
        setButtonLoading(btn, spin, false);
    }
});

document.getElementById("btn-resume").addEventListener("click", () => {
    const saved = loadSavedGame();
    if (!saved) return;
    state.gameId = saved.gameId;
    state.playerToken = saved.playerToken;
    state.role = saved.role;
    state.gameStatus = saved.gameStatus;
    doPoll();
    setupPeerConnection();
});

document.getElementById("btn-clear-recovery").addEventListener("click", () => {
        clearSavedGame();
    checkRecovery();
});

    document.getElementById("btn-return-start").addEventListener("click", returnToStart);
document.getElementById("btn-clear-game").addEventListener("click", () => {
        clearSavedGame();
    returnToStart();
});

// Maze click (available move squares)
document.getElementById("maze").addEventListener("click", (e) => {
    const cell = e.target.closest(".cell.move");
    if (!cell || !state.yourPosition) return;
    const r = Number(cell.dataset.row);
    const c = Number(cell.dataset.col);
    for (const dir of [DIR.NORTH, DIR.SOUTH, DIR.WEST, DIR.EAST]) {
        const t = applyDirection(state.yourPosition, dir);
    if (t.row === r && t.column === c) {submitMove(dir); return; }
    }
});

// Keyboard controls
document.addEventListener("keydown", (e) => {
    if (!document.getElementById("screen-game").classList.contains("active")) return;
    let dir = null;
    switch (e.key) {
        case "ArrowUp": dir = DIR.NORTH; break;
    case "ArrowDown": dir = DIR.SOUTH; break;
    case "ArrowLeft": dir = DIR.WEST; break;
    case "ArrowRight": dir = DIR.EAST; break;
    default: return;
    }
    e.preventDefault();
    submitMove(dir);
});

    // Init
    (async function initAuth() {
        try {
            const me = await fetchCurrentUser();
            enterApp(me.username);
        } catch {
            localStorage.removeItem(AUTH_USERNAME_KEY);
            showScreen("screen-login");
        }
    })();
    checkRecovery();