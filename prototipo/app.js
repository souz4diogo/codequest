/* CodeQuest — comportamento compartilhado do protótipo. */
(function () {
  const P = window.CQ.player;

  const NAV = [
    { id: "dashboard", label: "Dashboard", href: "index.html", icon: "layout-dashboard" },
    { id: "missoes", label: "Missões", href: "missoes.html", icon: "swords" },
    { id: "arvore", label: "Árvore", href: "arvore.html", icon: "network" },
    { id: "exercicio", label: "Exercícios", href: "exercicio.html", icon: "code-xml" },
    { id: "foco", label: "Foco", href: "foco.html", icon: "timer" },
    { id: "loja", label: "Loja", href: "loja.html", icon: "shopping-bag", secondary: true },
    { id: "mentor", label: "Mentor", href: "mentor.html", icon: "message-circle", secondary: true },
  ];

  /* ---------- montagem do chrome (sidebar + topbar) ---------- */
  function buildChrome() {
    const shell = document.getElementById("shell");
    const page = document.body.dataset.page || "dashboard";
    const title = document.body.dataset.title || "";
    const immersive = document.body.classList.contains("immersive");
    if (immersive) shell.classList.add("immersive");

    const sidebar = document.createElement("aside");
    sidebar.className = "sidebar";
    sidebar.innerHTML = `
      <div class="sidebar-logo">
        <span class="mark"><svg width="18" height="18" data-lucide="swords"></svg></span>
        <span class="name">CodeQuest</span>
      </div>
      <nav class="nav">
        ${NAV.map(n => `
          <a href="${n.href}" class="${n.id === page ? "active" : ""} ${n.secondary ? "secondary-nav" : ""}">
            <svg class="i" data-lucide="${n.icon}"></svg><span class="label">${n.label}</span>
          </a>`).join("")}
      </nav>
      <div class="mini-profile">
        <span class="avatar">${P.avatarIniciais}</span>
        <div class="meta">
          <div class="lv">Lv ${P.nivel}</div>
          <div class="mini-xp"><span style="width:${pct(P.xpAtual, P.xpProximo)}%"></span></div>
        </div>
      </div>`;

    const topbar = document.createElement("header");
    topbar.className = "topbar";
    topbar.innerHTML = `
      <button class="collapse-btn" id="collapseBtn" aria-label="Recolher menu"><svg class="i" data-lucide="panel-left"></svg></button>
      <h1>${title}</h1>
      <div class="spacer"></div>
      <div class="stats-cluster">
        <span class="stat-chip streak" title="Streak"><svg class="i-sm" data-lucide="flame"></svg><span class="val" id="statStreak">${P.streak}</span><span class="lbl-txt muted">dias</span></span>
        <span class="stat-chip gold" title="Gold"><svg class="i-sm" data-lucide="coins"></svg><span class="val" id="statGold">${P.gold}</span></span>
        <div class="topbar-xp">
          <div class="lbl"><span>XP</span><span><span id="xpNow">${P.xpAtual}</span> / ${P.xpProximo}</span></div>
          <div class="xp-bar thin"><span class="fill" id="topXpFill" data-pct="${pct(P.xpAtual, P.xpProximo)}"></span></div>
        </div>
      </div>`;

    const main = shell.querySelector(".main");
    shell.insertBefore(sidebar, main);
    shell.insertBefore(topbar, main);

    document.getElementById("collapseBtn").addEventListener("click", () => shell.classList.toggle("collapsed"));
  }

  /* ---------- utilidades ---------- */
  const pct = (a, b) => Math.max(0, Math.min(100, Math.round((a / b) * 100)));
  const reduceMotion = () => window.matchMedia("(prefers-reduced-motion: reduce)").matches;

  function ring(percent, opts = {}) {
    const size = opts.size || 48, stroke = opts.stroke || 5, cls = opts.cls || "";
    const r = (size - stroke) / 2, c = 2 * Math.PI * r, off = c * (1 - percent / 100);
    return `<svg class="ring ${cls}" width="${size}" height="${size}" viewBox="0 0 ${size} ${size}">
      <circle class="track" cx="${size/2}" cy="${size/2}" r="${r}" stroke-width="${stroke}"></circle>
      <circle class="value" cx="${size/2}" cy="${size/2}" r="${r}" stroke-width="${stroke}"
        stroke-dasharray="${c}" stroke-dashoffset="${reduceMotion() ? off : c}" data-off="${off}"></circle>
    </svg>`;
  }
  function activateRings(root = document) {
    root.querySelectorAll(".ring .value").forEach(v => {
      requestAnimationFrame(() => { v.style.strokeDashoffset = v.dataset.off; });
    });
  }

  function fillXpBars(root = document) {
    root.querySelectorAll(".xp-bar > .fill[data-pct]").forEach(f => {
      requestAnimationFrame(() => { f.style.width = f.dataset.pct + "%"; });
    });
  }

  function animateCounter(el, to, dur = 700) {
    const from = parseInt(el.textContent.replace(/\D/g, "")) || 0;
    if (from === to || reduceMotion()) { el.textContent = to; return; }
    const t0 = performance.now();
    (function step(t) {
      const k = Math.min(1, (t - t0) / dur);
      el.textContent = Math.round(from + (to - from) * (1 - Math.pow(1 - k, 3)));
      if (k < 1) requestAnimationFrame(step);
    })(performance.now());
  }

  function toast(text, type = "gold") {
    const root = document.getElementById("toast-root");
    const el = document.createElement("div");
    el.className = "toast " + type;
    const icon = type === "xp" ? "zap" : "coins";
    el.innerHTML = `<svg class="i-sm" data-lucide="${icon}"></svg>${text}`;
    root.appendChild(el);
    if (window.lucide) lucide.createIcons({ nameAttr: "data-lucide", root: el });
    setTimeout(() => el.remove(), 2200);
  }

  function flyXp(sourceEl, amount) {
    const target = document.getElementById("xpNow");
    if (!sourceEl || !target || reduceMotion()) return;
    const s = sourceEl.getBoundingClientRect(), t = target.getBoundingClientRect();
    const fly = document.createElement("div");
    fly.className = "xp-fly";
    fly.textContent = "+" + amount + " XP";
    fly.style.left = s.left + s.width / 2 + "px";
    fly.style.top = s.top + "px";
    document.body.appendChild(fly);
    requestAnimationFrame(() => {
      fly.style.transform = `translate(${t.left - s.left - s.width / 2}px, ${t.top - s.top}px) scale(.6)`;
      fly.style.opacity = "0";
    });
    setTimeout(() => fly.remove(), 950);
  }

  function confettiBurst(x, y) {
    if (reduceMotion()) return;
    const colors = ["#f0b429", "#7c5cff", "#3fb950", "#58a6ff"];
    for (let i = 0; i < 18; i++) {
      const c = document.createElement("div");
      c.className = "confetti";
      c.style.left = x + "px"; c.style.top = y + "px";
      c.style.background = colors[i % colors.length];
      document.body.appendChild(c);
      const ang = Math.random() * Math.PI * 2, dist = 40 + Math.random() * 80;
      c.animate([
        { transform: "translate(0,0) rotate(0)", opacity: 1 },
        { transform: `translate(${Math.cos(ang) * dist}px, ${Math.sin(ang) * dist + 60}px) rotate(${Math.random()*360}deg)`, opacity: 0 }
      ], { duration: 800 + Math.random() * 400, easing: "cubic-bezier(.2,.6,.3,1)" });
      setTimeout(() => c.remove(), 1300);
    }
  }

  /* ganho de XP/gold que reflete na topbar */
  function grantReward(xp, gold, sourceEl) {
    if (sourceEl) flyXp(sourceEl, xp);
    setTimeout(() => {
      const xpNow = document.getElementById("xpNow");
      const newXp = Math.min(P.xpProximo, (parseInt(xpNow.textContent) || 0) + xp);
      animateCounter(xpNow, newXp);
      const fill = document.getElementById("topXpFill");
      fill.style.width = pct(newXp, P.xpProximo) + "%";
      animateCounter(document.getElementById("statGold"), (parseInt(document.getElementById("statGold").textContent) || 0) + gold);
      toast(`+${xp} XP`, "xp");
      setTimeout(() => toast(`+${gold}`, "gold"), 250);
    }, sourceEl && !reduceMotion() ? 650 : 0);
  }

  /* ---------- level-up overlay ---------- */
  function levelUp(level, title) {
    let ov = document.getElementById("levelup");
    if (!ov) {
      ov = document.createElement("div");
      ov.className = "levelup"; ov.id = "levelup";
      document.body.appendChild(ov);
    }
    ov.innerHTML = `
      <div class="levelup-card">
        <div class="crest"><svg width="56" height="56" stroke="#f0b429" data-lucide="shield-check"></svg></div>
        <div class="kicker">LEVEL UP</div>
        <div class="lv-num display">LEVEL ${level}</div>
        <div class="lv-title">${title}</div>
        <button class="btn btn-primary btn-lg" id="luClose"><svg class="i" data-lucide="arrow-right"></svg> Continuar</button>
      </div>`;
    if (window.lucide) lucide.createIcons({ nameAttr: "data-lucide", root: ov });
    ov.classList.add("open");
    if (!reduceMotion()) {
      for (let i = 0; i < 30; i++) {
        const p = document.createElement("span");
        p.className = "particle";
        p.style.left = Math.random() * 100 + "%";
        p.style.animationDuration = 2 + Math.random() * 2.5 + "s";
        p.style.animationDelay = Math.random() * 1.5 + "s";
        p.style.opacity = .3 + Math.random() * .7;
        ov.appendChild(p);
      }
    }
    ov.querySelector("#luClose").addEventListener("click", () => ov.classList.remove("open"));
  }

  /* ---------- init ---------- */
  function init() {
    buildChrome();
    if (window.lucide) lucide.createIcons({ nameAttr: "data-lucide" });
    fillXpBars();
    activateRings();
    // expõe a API pras páginas
    window.CQUI = { ring, activateRings, fillXpBars, animateCounter, toast, flyXp, confettiBurst, grantReward, levelUp, pct, reduceMotion };
    document.dispatchEvent(new Event("cqui:ready"));
  }

  if (document.readyState === "loading") document.addEventListener("DOMContentLoaded", init);
  else init();
})();
