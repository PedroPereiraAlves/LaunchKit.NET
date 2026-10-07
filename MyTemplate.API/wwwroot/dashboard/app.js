const $ = (id) => document.getElementById(id);

function setMessage(text, kind) {
  const el = $("message");
  el.textContent = text;
  el.className = "msg" + (kind ? " " + kind : "");
}

function setBusy(busy) {
  $("loginBtn").disabled = busy;
  $("refreshBtn").disabled = busy;
  $("logoutBtn").disabled = busy;
}

function formatUptime(totalSeconds) {
  const seconds = Number(totalSeconds);
  if (!Number.isFinite(seconds) || seconds < 0) return "—";
  const whole = Math.floor(seconds);
  const hours = Math.floor(whole / 3600);
  const minutes = Math.floor((whole % 3600) / 60);
  const remainder = whole % 60;
  if (hours > 0) return hours + "h " + minutes + "m";
  if (minutes > 0) return minutes + "m " + remainder + "s";
  return remainder + "s";
}

function clearMetrics() {
  ["productsCount", "usersCount", "auditCount", "uptime"].forEach((id) => {
    $(id).textContent = "—";
  });
}

async function loadHealth() {
  const el = $("healthStatus");
  try {
    const res = await fetch("/health", { cache: "no-store" });
    const text = await res.text();
    let status = res.ok ? "Healthy" : "Unhealthy";
    let checks = text;
    try {
      const json = JSON.parse(text);
      status = json.status || status;
      checks = (json.checks || [])
        .map((check) => check.name + ": " + check.status)
        .join(" · ");
    } catch {
      checks = text || "Sem detalhe";
    }
    el.textContent = status;
    el.className = "value " + (res.ok ? "status-ok" : "status-bad");
    $("healthChecks").textContent = checks || "Sem checks";
  } catch {
    el.textContent = "Offline";
    el.className = "value status-bad";
    $("healthChecks").textContent = "Não foi possível consultar /health.";
  }
}

async function loadMetrics() {
  const token = $("token").value.trim();
  if (!token) {
    setMessage("Informe um token JWT ou entre com o admin.", "bad");
    return;
  }

  const res = await fetch("/api/metrics", {
    headers: { Authorization: "Bearer " + token },
    cache: "no-store"
  });
  const json = await res.json();
  $("raw").textContent = JSON.stringify(json, null, 2);

  if (!res.ok) {
    clearMetrics();
    setMessage(json.message || "Falha ao carregar métricas.", "bad");
    return;
  }

  const data = json.data || {};
  $("productsCount").textContent = data.productsCount ?? "—";
  $("usersCount").textContent = data.usersCount ?? "—";
  $("auditCount").textContent = data.auditLogsCount ?? "—";
  $("uptime").textContent = formatUptime(data.uptimeSeconds);
  const when = data.timestampUtc ? new Date(data.timestampUtc).toLocaleTimeString() : new Date().toLocaleTimeString();
  setMessage("Métricas atualizadas às " + when + ".", "ok");
}

async function login(event) {
  event.preventDefault();
  setBusy(true);
  setMessage("Entrando…");
  try {
    const res = await fetch("/api/auth/login", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({
        email: $("email").value,
        password: $("password").value
      })
    });
    const json = await res.json();
    $("raw").textContent = JSON.stringify({ ...json, data: json.data ? { ...json.data, token: json.data.token ? "[redacted]" : "" } : json.data }, null, 2);
    if (!res.ok) {
      setMessage(json.message || "Falha no login.", "bad");
      return;
    }
    const token = json.data?.token || "";
    $("token").value = token;
    localStorage.setItem("launchkit_token", token);
    setMessage("Login OK (" + (json.data?.role || "sem role") + ").", "ok");
    await loadMetrics();
  } catch {
    setMessage("Não foi possível contatar a API.", "bad");
  } finally {
    setBusy(false);
  }
}

function logout() {
  localStorage.removeItem("launchkit_token");
  $("token").value = "";
  $("password").value = "";
  clearMetrics();
  $("raw").textContent = "{}";
  setMessage("Sessão local encerrada.", "ok");
}

$("loginForm").addEventListener("submit", login);
$("refreshBtn").addEventListener("click", async () => {
  setBusy(true);
  try {
    await loadHealth();
    await loadMetrics();
  } finally {
    setBusy(false);
  }
});
$("logoutBtn").addEventListener("click", logout);

const saved = localStorage.getItem("launchkit_token");
if (saved) $("token").value = saved;

loadHealth();
if (saved) loadMetrics();
