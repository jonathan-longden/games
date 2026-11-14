/* ============================== STATE ============================== */
const KEY="coldcase.v2", BW=1000, BH=720, ZOOMS=[0,0.62,1], ZNAME=["Whole board","Medium","Close up"];
let S={players:["Detective 1","Detective 2"],sound:true,screen:"home",caseId:null,tab:"leads",doc:null,docFrom:"leads",pending:null,vote:null,prog:{},best:{},ask:null};
const newProg=()=>({opened:[],marks:{},notes:"",pos:{},links:[],threads:6,sel:null,zoom:1,route:[],routeTries:0,routeDone:false,routeOK:false,routePts:0,routeMsg:"",pin:null,anchor:null,anchorDone:false,anchorPts:0,result:null});
function save(){try{localStorage.setItem(KEY,JSON.stringify(S))}catch(e){}}
function load(){try{const d=JSON.parse(localStorage.getItem(KEY)||"null");if(d&&d.players)S=Object.assign(S,d)}catch(e){}}
const $=s=>document.querySelector(s);
const esc=s=>String(s).replace(/[&<>"]/g,c=>({"&":"&amp;","<":"&lt;",">":"&gt;",'"':"&quot;"}[c]));
const clamp=(v,a,b)=>Math.max(a,Math.min(b,v));
const C=()=>CASES.find(c=>c.id===S.caseId);
const P=()=>S.prog[S.caseId]||(S.prog[S.caseId]=newProg());
const lead=id=>C().leads.find(l=>l.id===id);
const left=()=>C().leads.length-P().opened.length;
const turnName=()=>S.players[P().opened.length%S.players.length]||"Detective";
let scrollKeep={};
function go(screen,extra){S.screen=screen;Object.assign(S,extra||{});render();window.scrollTo(0,0)}

/* ---------- small effects ---------- */
let AC=null,toastT=null;
function sfx(n){
  if(!S.sound)return;
  try{AC=AC||new (window.AudioContext||window.webkitAudioContext)();if(AC.state==="suspended")AC.resume();
    const t=AC.currentTime,o=AC.createOscillator(),g=AC.createGain();o.connect(g);g.connect(AC.destination);
    const set=(type,f0,f1,d,v)=>{o.type=type;o.frequency.setValueAtTime(f0,t);o.frequency.exponentialRampToValueAtTime(f1,t+d);g.gain.setValueAtTime(v,t);g.gain.exponentialRampToValueAtTime(.001,t+d);o.start(t);o.stop(t+d)};
    if(n==="pin")set("sine",190,55,.12,.5);else if(n==="link")set("triangle",392,880,.4,.22);else if(n==="bad")set("sawtooth",150,70,.25,.12);
    else if(n==="stamp")set("square",95,38,.2,.35);else if(n==="paper")set("triangle",900,300,.07,.06);else set("triangle",620,320,.05,.06);
  }catch(e){}
}
function toast(msg,kind){const t=$("#toast");if(!t)return;t.textContent=msg;t.className="show "+(kind||"");clearTimeout(toastT);toastT=setTimeout(()=>{t.className=""},3200)}
const IC={
  leads:'<path d="M3 6a2 2 0 0 1 2-2h4l2 2h8a2 2 0 0 1 2 2v9a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z"/>',
  board:'<path d="M9 3h6l-1 6 3 3v2H7v-2l3-3z"/><path d="M12 14v7"/>',
  map:'<path d="M9 4 3 6v14l6-2 6 2 6-2V4l-6 2z"/><path d="M9 4v14M15 6v14"/>',
  suspects:'<circle cx="12" cy="8" r="4"/><path d="M4 21c0-4.4 3.6-7 8-7s8 2.600 8 7"/>',
  notes:'<path d="M4 20h4L20 8l-4-4L4 16z"/><path d="M14 6l4 4"/>'
};
const icon=k=>`<svg viewBox="0 0 24 24" width="22" height="22" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">${IC[k]}</svg>`;

/* ---------- progression ---------- */
const RANKS=[[0,"Probationer"],[60,"Detective Constable"],[140,"Detective Sergeant"],[230,"Detective Inspector"],[310,"Chief Inspector"],[370,"Detective Chief Superintendent"]];
const COMMS={links:"No stone unturned",route:"Cartographer",all:"Unanimous",lean:"Lean inquiry",solved:"Case closed"};
function totals(){let xp=0,closed=0,comms=0;for(const k in S.best){xp+=S.best[k].score;if(S.best[k].solved)closed++;comms+=(S.best[k].comms||[]).length}
  let r=0;RANKS.forEach((x,i)=>{if(xp>=x[0])r=i});return {xp,closed,comms,rank:RANKS[r][1],next:RANKS[r+1]||null,base:RANKS[r][0]}}
function stars(b){if(!b)return"";const n=b.score>=85?3:b.score>=60?2:b.solved?1:0;return "★".repeat(n)+"☆".repeat(3-n)}
function caseRank(n){return n>=90?"Outstanding":n>=75?"Commended":n>=55?"Sound":n>=30?"Shaky":"Back to training school"}

/* ============================== HOME ============================== */
function vHome(){
  const t=totals();
  const pct=t.next?Math.round(100*(t.xp-t.base)/(t.next[0]-t.base)):100;
  const cases=CASES.map((c,i)=>{
    const b=S.best[c.id],p=S.prog[c.id],active=p&&p.opened.length&&!p.result;
    const faces=c.victims.slice(0,2).map(v=>portrait(v.k)).join("");
    const status=active?`<span class="pill hot">In progress · ${p.opened.length} of ${c.leads.length} leads read</span>`:b?`<span class="pill ${b.solved?"green":"red"}">${b.solved?"Solved":"Unsolved"} · ${b.score}</span><span class="stars">${stars(b)}</span>`:`<span class="pill">Unopened</span>`;
    const reset=(p&&(p.opened.length||p.result))?(S.ask===c.id?`<span class="askrow">Wipe this file and start again? <button class="link" data-act="resetyes" data-id="${c.id}">Yes, wipe it</button><button class="link" data-act="resetno">Keep it</button></span>`:`<button class="link" data-act="reset" data-id="${c.id}">Reopen from scratch</button>`):"";
    return `<div class="ccard">
      <button class="cmain" data-act="open" data-id="${c.id}" style="background-image:url(img/cover_${c.id}.jpg)">
        <span class="cnum">Case ${String(i+1).padStart(2,"0")} · File ${c.ref}</span><span class="ctit">${c.title}</span><span class="cstamp">${c.place} · ${c.date}</span>
      </button>
      <div class="cbody"><div class="cfaces">${c.victims.map(v=>portrait(v.k)).join("")}</div><p class="fblurb">${c.blurb}</p>
      <div class="frow"><span class="pill fill">${c.level}</span><span class="pill">${c.mins}</span>${status}</div>${reset}</div></div>`}).join("");
  const chips=S.players.map((p,i)=>`<span class="chip">${esc(p)}<button data-act="rm" data-i="${i}" aria-label="Remove ${esc(p)}">×</button></span>`).join("");
  return `<header class="hero cine">
    <div class="heroTop"><span class="eyebrow">Cold Case Review Unit · East Midlands</span><button class="link" data-act="sound">Sound: ${S.sound?"on":"off"}</button></div>
    <h1>Cold Case Night</h1>
    <p>Four cases nobody closed. Read the files, pin the evidence to the board, run string between what connects, trace the map. Then name the killer.</p>
  </header>
  ${installCard()}
  <section class="warrant">
    <div class="wtop"><div><span class="lab">Team rank</span><div class="wrank">${t.rank}</div></div><div class="wxp">${t.xp}<small> pts</small></div></div>
    <div class="xpbar" role="progressbar" aria-valuenow="${pct}" aria-valuemin="0" aria-valuemax="100"><i style="width:${pct}%"></i></div>
    <div class="wstats"><span><b>${t.closed}</b> of ${CASES.length} cases closed</span><span><b>${t.comms}</b> commendations</span>${t.next?`<span>${t.next[0]-t.xp} pts to ${t.next[1]}</span>`:`<span>Top rank reached</span>`}</div>
  </section>
  <section class="teambox">
    <label class="lab" for="pname">Detectives on the team</label>
    <div class="team">${chips||'<span class="note">Add at least one detective.</span>'}</div>
    <form class="addrow" id="addform"><input id="pname" maxlength="16" placeholder="Add a name" autocomplete="off"><button class="btn small" type="submit">Add</button></form>
  </section>
  <span class="cat">Open files</span>
  <div class="list">${cases}</div>
  <p class="note">Every case, person and place here is invented. The files are written like real police paperwork but involve no real victims, and there is no graphic detail. Photographs and footage are reconstructions.</p>`;
}

/* ---------- install ---------- */
let DEFER=null;
window.addEventListener("beforeinstallprompt",e=>{e.preventDefault();DEFER=e;if(S.screen==="home")render()});
window.addEventListener("appinstalled",()=>{DEFER=null;if(S.screen==="home")render()});
function installCard(){
  const standalone=window.matchMedia("(display-mode: standalone)").matches||navigator.standalone;
  if(standalone||!("serviceWorker" in navigator))return"";
  const ios=/iphone|ipad|ipod/i.test(navigator.userAgent);
  if(DEFER)return `<section class="install"><span><b>Install the game</b><br>Adds Cold Case Night to your home screen and lets it work offline.</span><button class="btn small" data-act="install">Install</button></section>`;
  if(ios)return `<section class="install"><span><b>Install on iPhone</b><br>In Safari, tap the Share button, then <b>Add to Home Screen</b>.</span></section>`;
  return `<section class="install"><span><b>Install the game</b><br>Open your browser menu (⋮) and choose <b>Install app</b> or <b>Add to Home screen</b>.</span></section>`;
}
/* ============================== BRIEF ============================== */
function vBrief(){
  const c=C();
  return `<button class="back" data-act="home">‹ All files</button>
  <div class="titlecard" style="background-image:url(img/cover_${c.id}.jpg)"><span class="cnum">Case ${String(CASES.indexOf(c)+1).padStart(2,"0")}</span><span class="ctit">${c.title}</span><span class="cstamp">${c.place}<br>${c.date}</span></div>
  <article class="sheet">
    <div class="dochead"><span class="meta">File ${c.ref}</span><span class="stamp">Cold case</span></div>
    <div class="victims">${c.victims.map(v=>`<div class="victim">${portrait(v.k)}<span class="meta"><b>${v.name}</b><br>${v.sub}</span></div>`).join("")}</div>
    <div class="docbody">${c.brief.map(p=>`<p>${p}</p>`).join("")}</div><hr>
    <h3>How the inquiry runs</h3>
    <ul class="plain">
      <li><b>Leads.</b> The file holds ${c.leads.length} and you can read them all. Take turns: the detective named at the top picks the next one and reads it aloud.</li>
      <li><b>Board.</b> Everything you open is pinned to the cork board. Tap two items to run string between them. If they really connect, the board confirms it. A wrong string costs one of six spare threads.</li>
      <li><b>Map.</b> Pin and string the route. You get three checks.</li>
      <li><b>Accuse.</b> Each detective names a suspect in secret, then the team answers two questions together.</li>
    </ul>
  </article>
  <button class="btn" data-act="start">Open the investigation</button>`;
}

/* ============================== CASE SCREEN ============================== */
function vCase(){
  const c=C(),p=P();
  const body={leads:tLeads,board:tBoard,map:tMap,suspects:tSuspects,notes:tNotes}[S.tab]();
  const tabs=[["leads","Leads"],["board","Board"],["map","Map"],["suspects","Suspects"],["notes","Notes"]].map(([k,t])=>{
    const badge=k==="board"?`${p.links.length}/${c.links.length}`:k==="leads"?`${left()}`:k==="map"&&(p.routeOK)?"✓":"";
    return `<button role="tab" aria-selected="${S.tab===k}" data-act="tab" data-id="${k}">${icon(k)}<span>${t}</span>${badge?`<em>${badge}</em>`:""}</button>`}).join("");
  return `<header class="chead">
    <div class="ctop"><button class="back" data-act="home">‹ Files</button><span class="ctitle">${c.title}</span><button class="btn red small" data-act="accuse">Accuse</button></div>
    <div class="turn">${left()>0?`<span>Lead detective <b>${esc(turnName())}</b></span>`:`<span><b>Every lead read</b></span>`}<span>${p.opened.length} of ${c.leads.length} leads read</span></div>
  </header>${body}<nav class="tabs" role="tablist">${tabs}</nav>`;
}
function tLeads(){
  const c=C(),p=P();
  const cats=[...new Set(c.leads.map(l=>l.cat))];
  return cats.map(cat=>`<span class="cat">${cat}</span><div class="list">${c.leads.filter(l=>l.cat===cat).map(l=>{
    const open=p.opened.includes(l.id), locked=l.requires&&!p.opened.includes(l.requires);
    if(locked) return `<div class="lead locked"><span class="lt">Sealed lead</span><span class="ls">Opens after you read “${lead(l.requires).title}”.</span></div>`;
    return `<button class="lead ${open?"open":""}" data-act="${open?"read":"spend"}" data-id="${l.id}" data-from="leads">
      <span class="lt">${l.title}</span><span class="ls">${l.sub}</span>
      <span class="lf">${mediaPills(l)}${open?'<span class="pill green">Read · on the board</span>':'<span class="pill">Unread</span>'}</span></button>`;
  }).join("")}</div>`).join("");
}
function tSuspects(){
  const c=C(),p=P();
  return `<p class="note">Mark people as you go. A confirmed string that clears someone rules them out for you.</p><div class="list">${c.suspects.map(s=>{
    const m=p.marks[s.id]||"";
    return `<div class="sus ${m}"><div class="hd">${portrait(s.id)}<div><span class="name">${s.name}</span><br><span class="facts">${s.facts}</span></div></div><p>${s.note}</p>
    <div class="marks"><button aria-pressed="${m==="prime"}" data-act="mark" data-id="${s.id}" data-m="prime">Prime suspect</button><button aria-pressed="${m==="cleared"}" data-act="mark" data-id="${s.id}" data-m="cleared">Ruled out</button></div></div>`}).join("")}</div>`;
}
function tNotes(){
  return `<label class="lab" for="notes">Team notebook</label><textarea id="notes" placeholder="Times, lies, things that do not add up…">${esc(P().notes)}</textarea>
  <p class="note">Write down every time anyone gives, then look for two that cannot both be true.</p>`;
}

/* ============================== EVIDENCE BOARD ============================== */
function hash(s){let h=7;for(let i=0;i<s.length;i++)h=(h*31+s.charCodeAt(i))%9973;return h}
const rot=id=>((hash(id)%70)/10-3.5).toFixed(1);
function boardItems(){
  const c=C(),p=P(),it=[];
  c.victims.forEach((v,i)=>it.push({id:"v:"+i,kind:"victim",k:v.k,name:v.name,w:110,h:152}));
  c.suspects.forEach(s=>it.push({id:"s:"+s.id,kind:"suspect",k:s.id,sid:s.id,name:s.name,w:110,h:152}));
  p.opened.forEach(id=>{const l=lead(id);if(l)it.push({id:l.id,kind:"ev",lead:l,name:l.title,w:150,h:92})});
  return it;
}
function ensurePos(){
  const c=C(),p=P();
  const nv=c.victims.length;
  c.victims.forEach((v,i)=>{if(!p.pos["v:"+i])p.pos["v:"+i]=[(BW-nv*128)/2+i*128+9,22]});
  c.suspects.forEach((s,i)=>{if(!p.pos["s:"+s.id])p.pos["s:"+s.id]=[48+i*190,558]});
  p.opened.forEach((id,i)=>{if(!p.pos[id]){const col=i%5,row=Math.floor(i/5);p.pos[id]=[36+col*190+(row%2?22:0),196+row*118]}});
}
function zoomScale(){const p=P();const z=ZOOMS[p.zoom];if(z)return z;return clamp((Math.min(window.innerWidth,720)-60)/BW,.25,1)}
function tBoard(){
  const c=C(),p=P();ensurePos();
  const items=boardItems(),z=zoomScale();
  const cards=items.map(it=>{
    const q=p.pos[it.id],sel=p.sel===it.id?" sel":"";
    const st=`left:${q[0]}px;top:${q[1]}px;--r:${rot(it.id)}deg`;
    if(it.kind==="ev"){const l=it.lead;return `<div class="card ev${sel}" data-id="${it.id}" data-w="${it.w}" data-h="${it.h}" style="${st}"><i class="pin"></i><span class="et">${l.type}</span><b>${l.title}</b><span class="er">${l.ref}</span></div>`}
    const m=it.sid?(p.marks[it.sid]||""):"";
    return `<div class="card pol ${it.kind} ${m}${sel}" data-id="${it.id}" data-w="${it.w}" data-h="${it.h}" style="${st}"><i class="pin ${it.kind==="victim"?"blk":""}"></i>${portrait(it.k)}<span class="hw" style="font-size:${it.name.length>15?13:17}px">${it.name}</span>${it.kind==="victim"?'<span class="tagv">Victim</span>':""}${m==="cleared"?'<span class="ro">Ruled out</span>':""}${m==="prime"?'<span class="pr">Prime</span>':""}</div>`}).join("");
  const selIt=items.find(i=>i.id===p.sel);
  const spools=Array.from({length:6},(_,i)=>`<i class="${i<p.threads?"":"gone"}"></i>`).join("");
  const bar=selIt?`<div class="selbar"><span><b>${selIt.name}</b> selected. Tap a second item to run a string to it.</span><span class="rowbtns">${selIt.kind==="ev"?`<button class="btn small dark" data-act="read" data-id="${selIt.id}" data-from="board">Read file</button>`:""}<button class="btn small line" data-act="desel">Cancel</button></span></div>`
    :`<p class="note">Drag to rearrange. Tap one item, then another, to run string between two things that connect: a statement and the record that breaks it, a suspect and the evidence that clears them.</p>`;
  const found=p.links.map(l=>`<li>${c.links[l.i].t}</li>`).join("");
  const missing=c.links.length-p.links.length;
  return `<div class="boardbar"><span>Connections <b>${p.links.length}/${c.links.length}</b></span><span class="spools" aria-label="${p.threads} spare threads">${spools}</span><button class="link" data-act="zoom">View: ${ZNAME[p.zoom]}</button></div>
  <div class="boardframe"><div class="boardscroll" id="bscroll"><div class="bsizer" style="width:${BW*z}px;height:${BH*z}px"><div class="board" id="board" style="transform:scale(${z})">
    <svg class="strings" id="strings" viewBox="0 0 ${BW} ${BH}" width="${BW}" height="${BH}"></svg>${cards}
    ${p.opened.length?"":'<div class="emptyhint">Open a lead and it will be pinned here.</div>'}
  </div></div></div></div>
  ${bar}
  <div class="found"><span class="lab">Connections on the board</span><ul class="plain">${found}${missing?`<li class="dim">${missing} still to find</li>`:""}</ul></div>`;
}
function pinPt(id){const p=P(),q=p.pos[id];const w=id.includes(":")?110:150;return [q[0]+w/2,q[1]+5]}
function drawStrings(){
  const svg=$("#strings");if(!svg)return;const c=C(),p=P();
  svg.innerHTML=p.links.map(l=>{
    if(!p.pos[l.a]||!p.pos[l.b])return"";
    const [x1,y1]=pinPt(l.a),[x2,y2]=pinPt(l.b),mx=(x1+x2)/2,my=(y1+y2)/2+Math.min(40,Math.hypot(x2-x1,y2-y1)*.12);
    const t=c.links[l.i].t,tw=Math.min(260,t.length*6.3+16),lx=mx,ly=(y1+y2)/4+my/2;
    return `<path d="M${x1} ${y1} Q${mx} ${my} ${x2} ${y2}" class="sh"/><path d="M${x1} ${y1} Q${mx} ${my} ${x2} ${y2}" class="st"/>
      <g transform="translate(${lx} ${ly}) rotate(${(hash(t)%7)-3})"><rect x="${-tw/2}" y="-11" width="${tw}" height="22" rx="2" class="tg"/><text y="5" text-anchor="middle" class="tt" textLength="${tw-14}" lengthAdjust="spacingAndGlyphs">${esc(t)}</text></g>`}).join("");
}
let SC=1;
function initBoard(){
  const b=$("#board");if(!b)return;const p=P();SC=zoomScale();
  const sc=$("#bscroll"),k=S.caseId+p.zoom;if(scrollKeep[k]){sc.scrollLeft=scrollKeep[k][0];sc.scrollTop=scrollKeep[k][1]}
  sc.addEventListener("scroll",()=>{scrollKeep[k]=[sc.scrollLeft,sc.scrollTop]},{passive:true});
  drawStrings();
  if(p.shake){p.shake.forEach(id=>{const el=b.querySelector(`[data-id="${id}"]`);if(el)el.classList.add("shake")});p.shake=null}
  b.querySelectorAll(".card").forEach(el=>{
    const id=el.dataset.id;let sx=0,sy=0,ox=0,oy=0,drag=false,pid=null;
    el.addEventListener("pointerdown",e=>{pid=e.pointerId;try{el.setPointerCapture(pid)}catch(_){}sx=e.clientX;sy=e.clientY;ox=p.pos[id][0];oy=p.pos[id][1];drag=false});
    el.addEventListener("pointermove",e=>{
      if(pid!==e.pointerId)return;const dx=e.clientX-sx,dy=e.clientY-sy;
      if(!drag&&Math.hypot(dx,dy)<8)return;drag=true;el.classList.add("drag");
      const x=clamp(ox+dx/SC,0,BW-(+el.dataset.w)),y=clamp(oy+dy/SC,0,BH-(+el.dataset.h));
      p.pos[id]=[Math.round(x),Math.round(y)];el.style.left=x+"px";el.style.top=y+"px";drawStrings()});
    el.addEventListener("pointerup",e=>{if(pid!==e.pointerId)return;pid=null;el.classList.remove("drag");if(drag){sfx("pin");save()}else tapCard(id)});
    el.addEventListener("pointercancel",()=>{pid=null;el.classList.remove("drag")});
  });
}
function tapCard(id){
  const p=P();
  if(!p.sel){p.sel=id;sfx("tap");render();return}
  if(p.sel===id){p.sel=null;render();return}
  const a=p.sel;p.sel=null;tryLink(a,id);render();
}
function tryLink(a,b){
  const c=C(),p=P();
  if(a.startsWith("v:")||b.startsWith("v:")){toast("Everything connects to the victim. Try two pieces of evidence, or a suspect and what clears or catches them.");return}
  if(p.links.some(l=>(l.a===a&&l.b===b)||(l.a===b&&l.b===a))){toast("Those two are already strung together.");return}
  if(p.threads<=0){toast("You are out of spare thread. No more strings can be run.","bad");return}
  const i=c.links.findIndex(L=>(L.a.includes(a)&&L.b.includes(b))||(L.a.includes(b)&&L.b.includes(a)));
  if(i>=0&&p.links.some(l=>l.i===i)){toast("You already have that connection on the board.");return}
  if(i>=0){p.links.push({a,b,i});const L=c.links[i];if(L.clears)p.marks[L.clears]="cleared";sfx("link");toast("Connection confirmed. "+L.t+(L.clears?" One suspect ruled out.":""),"ok")}
  else{p.threads--;p.shake=[a,b];sfx("bad");toast("No link there. "+p.threads+" spare thread"+(p.threads===1?"":"s")+" left.","bad")}
}

/* ============================== MAP ============================== */
function rnd(seed){let s=seed;return()=>(s=(s*16807)%2147483647)/2147483647}
function mapSVG(m,p,reveal){
  const r=rnd(m.seed);
  const grid=[];for(let x=50;x<400;x+=50)grid.push(`<path d="M${x} 0V500" class="g"/><text x="${x+2}" y="8" class="gn">${40+x/50}</text>`);
  for(let y=50;y<500;y+=50)grid.push(`<path d="M0 ${y}H400" class="g"/><text x="2" y="${y-2}" class="gn">${70-y/50}</text>`);
  const cont=[];for(let i=0;i<7;i++){const cx=r()*400,cy=r()*500,a=30+r()*60;for(let k=1;k<=3;k++)cont.push(`<ellipse cx="${cx.toFixed(0)}" cy="${cy.toFixed(0)}" rx="${(a*k*.6).toFixed(0)}" ry="${(a*k*.42).toFixed(0)}" transform="rotate(${(r()*180).toFixed(0)} ${cx.toFixed(0)} ${cy.toFixed(0)})" class="ct"/>`)}
  const built=m.built.map(([cx,cy,n])=>{let h="";const sp=4+Math.sqrt(n)*5.5;for(let i=0;i<n;i++){const a=r()*6.28,d=Math.sqrt(r())*sp,x=cx+Math.cos(a)*d,y=cy+Math.sin(a)*d*.85;h+=`<rect x="${x.toFixed(1)}" y="${y.toFixed(1)}" width="${(3+r()*5).toFixed(1)}" height="${(2.5+r()*4).toFixed(1)}" transform="rotate(${(r()*90).toFixed(0)} ${x.toFixed(1)} ${y.toFixed(1)})" class="bd"/>`}return h}).join("");
  const roads=["a","b","m"].map(c=>m.roads.filter(x=>x.c===c)).reverse();
  const casing=m.roads.map(x=>`<path d="${x.d}" class="rc ${x.c}"/>`).join("");
  const fill=roads.map(g=>g.map(x=>`<path d="${x.d}" class="rf ${x.c}"/>`).join("")).join("");
  const pt=id=>m.pins.find(q=>q.id===id);
  const seq=reveal?m.route.order:p.route;
  const visits={};seq.forEach((id,i)=>{(visits[id]=visits[id]||[]).push(i+1)});
  let str="";
  for(let i=1;i<seq.length;i++){const a=pt(seq[i-1]),b=pt(seq[i]);if(!a||!b)continue;const off=(i%2?1:-1)*4*Math.floor(i/2+.5);
    const mx=(a.x+b.x)/2+off,my=(a.y+b.y)/2-17+off;
    str+=`<path d="M${a.x} ${a.y-17} Q${mx} ${my+10} ${b.x} ${b.y-17}" class="msh"/><path d="M${a.x} ${a.y-17} Q${mx} ${my+10} ${b.x} ${b.y-17}" class="mst ${p.routeOK||reveal?"ok":""}"/>`}
  const pins=m.pins.map(q=>`<g class="mpin ${p.pin===q.id?"on":""}" data-act="pin" data-id="${q.id}" transform="translate(${q.x} ${q.y})" role="button" aria-label="${esc(q.name)}">
    <circle cx="0" cy="-9" r="19" fill="transparent"/><ellipse cx="4" cy="1" rx="6" ry="2.2" class="psh"/><path d="M0 0V-14" class="pn"/><circle cx="0" cy="-17" r="6.5" fill="url(#pinred)"/><circle cx="-2" cy="-19" r="1.8" fill="#fff" opacity=".7"/>
    <text y="12" text-anchor="middle" class="pl">${esc(q.name)}</text>${visits[q.id]?`<g transform="translate(13 -24)"><rect x="-3" y="-9" width="${visits[q.id].length*9+10}" height="14" rx="2" class="pnb"/><text x="${(visits[q.id].length*9+4)/2}" y="2" text-anchor="middle" class="pnt">${visits[q.id].join(",")}</text></g>`:""}</g>`).join("");
  let anc="";
  if(m.anchor){
    if(p.anchor)anc+=`<g transform="translate(${p.anchor[0]} ${p.anchor[1]})"><ellipse cx="4" cy="1" rx="6" ry="2.2" class="psh"/><path d="M0 0V-14" class="pn"/><circle cx="0" cy="-17" r="7" fill="url(#pinblue)"/><text y="12" text-anchor="middle" class="pl">Your pin</text></g>`;
    if(reveal)anc+=`<circle cx="${m.anchor.x}" cy="${m.anchor.y}" r="${m.anchor.r}" class="anc"/><text x="${m.anchor.x}" y="${m.anchor.y-m.anchor.r-5}" text-anchor="middle" class="pl">${m.anchor.name}</text>`;
  }
  return `<svg class="map" viewBox="0 0 400 500" data-act="maptap" role="img" aria-label="Map of ${esc(m.name)}">
  <defs><radialGradient id="pinred" cx=".35" cy=".3" r=".8"><stop offset="0" stop-color="#ff7b6b"/><stop offset=".5" stop-color="#c8281c"/><stop offset="1" stop-color="#6e120c"/></radialGradient>
  <radialGradient id="pinblue" cx=".35" cy=".3" r=".8"><stop offset="0" stop-color="#8ab4ff"/><stop offset=".5" stop-color="#1f4fb0"/><stop offset="1" stop-color="#0c2460"/></radialGradient>
  <linearGradient id="fold" x1="0" x2="1"><stop offset="0" stop-color="#000" stop-opacity=".10"/><stop offset=".5" stop-color="#fff" stop-opacity=".12"/><stop offset="1" stop-color="#000" stop-opacity="0"/></linearGradient>
  <filter id="mpaper"><feTurbulence type="fractalNoise" baseFrequency=".8" numOctaves="2" seed="4"/><feColorMatrix values="0 0 0 0 .4 0 0 0 0 .32 0 0 0 0 .2 0 0 0 .1 0"/></filter></defs>
  <rect width="400" height="500" fill="#ece6d2"/>${cont.join("")}
  ${m.woods.map(d=>`<path d="${d}" class="wd"/>`).join("")}
  ${m.water.map(w=>`<path d="${w.d}" class="wc" stroke-width="${w.w+1.6}"/><path d="${w.d}" class="wf" stroke-width="${w.w}"/>`).join("")}
  ${grid.join("")}
  ${m.rail.map(d=>`<path d="${d}" class="rl"/><path d="${d}" class="rt"/>`).join("")}
  ${casing}${fill}${built}
  ${m.water.map(w=>`<text x="${w.lx}" y="${w.ly}" transform="rotate(${w.rot} ${w.lx} ${w.ly})" class="wl">${w.label}</text>`).join("")}
  ${m.labels.map(l=>`<text x="${l.x}" y="${l.y}" class="lb ${l.c}">${l.t}</text>`).join("")}
  <g transform="translate(286 478)"><path d="M0 0H90M0 -4V4M45 -3V3M90 -4V4" class="sc"/><text x="45" y="-7" text-anchor="middle" class="gn">${m.scale}</text></g>
  <g transform="translate(374 30)"><path d="M0 12 L5 -10 L10 12 L5 7Z" fill="#2a2a2a"/><text x="5" y="-13" text-anchor="middle" class="gn" style="font-size:8px;font-weight:700">N</text></g>
  <rect width="400" height="500" filter="url(#mpaper)"/>
  <rect x="130" width="8" height="500" fill="url(#fold)"/><rect x="264" width="8" height="500" fill="url(#fold)"/><rect y="246" width="400" height="1.2" fill="#000" opacity=".08"/>
  ${str}${anc}${pins}</svg>`;
}
function tMap(){
  const c=C(),m=c.map,p=P(),n=m.route.order.length;
  const pin=m.pins.find(q=>q.id===p.pin);
  const seq=p.route.map((id,i)=>`<span class="stop">${i+1}. ${m.pins.find(q=>q.id===id).name}</span>`).join("")||'<span class="note">Tap pins on the map in order.</span>';
  let ctl;
  if(!p.routeDone){
    ctl=`<div class="mq"><span class="lab">String the route · ${n} stops · ${3-p.routeTries} check${3-p.routeTries===1?"":"s"} left</span><p>${m.route.q}</p>
    <div class="stops">${seq}</div>${p.routeMsg?`<p class="msg">${p.routeMsg}</p>`:""}
    <div class="rowbtns"><button class="btn small line" data-act="rundo" ${p.route.length?"":"disabled"}>Undo</button><button class="btn small line" data-act="rclear" ${p.route.length?"":"disabled"}>Clear</button><button class="btn small" data-act="rcheck" ${p.route.length===n?"":"disabled"}>Check route</button></div></div>`;
  } else {
    ctl=`<div class="mq"><span class="lab">Route</span><p class="msg ${p.routeOK?"ok":""}">${p.routeOK?"Route confirmed.":"Out of checks. The true route is shown when the case closes."}</p>
    ${p.routeOK?`<ol class="plain">${m.route.order.map((id,i)=>`<li><b>${m.pins.find(q=>q.id===id).name}.</b> ${m.route.notes[i]}</li>`).join("")}</ol>`:""}</div>`;
    if(m.anchor){
      ctl+=p.anchorDone?`<div class="mq"><span class="lab">Base pin</span><p class="msg">Pin locked in. It is scored when the case closes.</p></div>`
        :`<div class="mq"><span class="lab">Geographic profile</span><p>${m.anchor.q}</p><div class="rowbtns"><button class="btn small" data-act="alock" ${p.anchor?"":"disabled"}>Lock in the pin</button></div></div>`;
    }
  }
  return `<div class="mapframe">${mapSVG(m,p,false)}</div>
  ${pin?`<div class="pininfo"><b>${pin.name}</b><span>${pin.note}</span></div>`:`<p class="note">${m.name}. Tap a pin to see what happened there.</p>`}
  ${ctl}`;
}
function mapTap(e){
  const c=C(),m=c.map,p=P();
  if(!m.anchor||!p.routeDone||p.anchorDone)return;
  const svg=e.target.closest("svg"),r=svg.getBoundingClientRect();
  p.anchor=[Math.round((e.clientX-r.left)/r.width*400),Math.round((e.clientY-r.top)/r.height*500)];sfx("pin");render();
}
function pinTap(id,e){
  const c=C(),m=c.map,p=P();p.pin=id;
  if(!p.routeDone){
    if(p.route.length<m.route.order.length&&p.route[p.route.length-1]!==id){p.route.push(id);p.routeMsg="";sfx("pin")}
  } else if(m.anchor&&!p.anchorDone){const q=m.pins.find(x=>x.id===id);p.anchor=[q.x,q.y];sfx("pin")}
  render();
}
function checkRoute(){
  const m=C().map,p=P(),o=m.route.order;
  const right=p.route.filter((id,i)=>id===o[i]).length;
  if(right===o.length){p.routeDone=true;p.routeOK=true;const full=m.route.half?[5,4,2]:[10,7,4];p.routePts=full[p.routeTries];sfx("link");toast("Route confirmed.","ok")}
  else{p.routeTries++;sfx("bad");
    if(p.routeTries>=3){p.routeDone=true;p.routeMsg=""}
    else p.routeMsg=`${right} of ${o.length} stops are in the right place. ${3-p.routeTries} check${3-p.routeTries===1?"":"s"} left.`}
}

/* ============================== DOCUMENT ============================== */
function vDoc(){
  const l=lead(S.doc);
  return `<button class="back" data-act="tocase">‹ Back to the ${S.docFrom==="board"?"board":"leads"}</button>
  <article class="sheet doc">
    <div class="dochead"><span class="meta">${l.type}<br>Ref ${l.ref}<br>${l.date}</span><span class="pill">${l.cat}</span></div>
    <h2>${l.title}</h2><hr>
    ${media(l)}<div class="docbody">${l.body}</div>
  </article>
  <p class="note">Read it aloud, then talk before the next detective chooses. This file is now pinned to your board.</p>
  <div class="rowbtns"><button class="btn" data-act="tocase">Back to the ${S.docFrom==="board"?"board":"leads"}</button><button class="btn line" data-act="toboard">See it on the board</button></div>`;
}

/* ============================== ACCUSATION ============================== */
function vAccuse(){
  const c=C(),v=S.vote;
  if(v.step==="who"){
    const p=S.players[v.i];
    if(!v.ready) return `<button class="back" data-act="tocase">‹ Keep investigating</button>
      <article class="sheet"><span class="meta">Secret ballot ${v.i+1} of ${S.players.length}</span><h2>Pass the phone to ${esc(p)}</h2>
      <p>Everyone else look away. Each detective names a suspect without seeing the others' answers.</p></article>
      <button class="btn" data-act="vready">I am ${esc(p)}</button>`;
    return `<article class="sheet"><span class="meta">${esc(p)}'s accusation</span><h2>Who is responsible?</h2></article>
      <div class="list">${c.suspects.map(s=>`<button class="opt" aria-pressed="${v.picks[v.i]===s.id}" data-act="pick" data-id="${s.id}"><span class="hd">${portrait(s.id)}<span><b>${s.name}</b><span>${s.facts}</span></span></span></button>`).join("")}</div>
      <button class="btn" data-act="vnext" ${v.picks[v.i]?"":"disabled"}>Lock it in</button>`;
  }
  const isM=v.step==="motive",opts=isM?c.motives:c.hows,sel=isM?v.motive:v.how;
  return `<article class="sheet"><span class="meta">Team decision ${isM?"1":"2"} of 2</span><h2>${isM?(c.whyQ||"Why was the victim killed?"):c.howQ}</h2><p>Phone in the middle. Agree one answer together.</p></article>
    <div class="list">${opts.map(o=>`<button class="opt" aria-pressed="${sel===o.id}" data-act="topt" data-id="${o.id}"><b>${o.t}</b></button>`).join("")}</div>
    <button class="btn red" data-act="tnext" ${sel?"":"disabled"}>${isM?"Next question":"Close the case"}</button>`;
}
function finish(){
  const c=C(),p=P(),v=S.vote,n=S.players.length;
  const right=v.picks.filter(x=>x===c.killer).length;
  const mOK=!!c.motives.find(o=>o.id===v.motive&&o.ok),hOK=!!c.hows.find(o=>o.id===v.how&&o.ok);
  if(c.map.anchor&&p.anchor&&!p.anchorDone)lockAnchor();
  const who=Math.round(46*right/n),links=Math.round(20*p.links.length/c.links.length),mp=p.routePts+p.anchorPts,saved=0;
  const score=Math.min(100,who+(mOK?12:0)+(hOK?12:0)+links+mp+saved);
  const solved=right*2>=n&&(mOK||hOK);
  const comms=[];if(solved)comms.push("solved");if(p.links.length===c.links.length)comms.push("links");if(p.routeOK&&p.routeTries===0)comms.push("route");if(right===n&&n>1)comms.push("all");
  p.result={picks:v.picks.slice(),mOK,hOK,who,links,mp,saved,score,solved,comms};
  const b=S.best[c.id];if(!b||score>b.score)S.best[c.id]={score,solved,comms};
  sfx("stamp");go("reveal");
}
function lockAnchor(){const m=C().map,p=P();if(!p.anchor)return;const d=Math.hypot(p.anchor[0]-m.anchor.x,p.anchor[1]-m.anchor.y);p.anchorPts=d<=m.anchor.r?5:d<=m.anchor.r*2?3:0;p.anchorDone=true}
function vReveal(){
  const c=C(),p=P(),r=p.result,k=c.suspects.find(s=>s.id===c.killer),m=c.map;
  const rows=S.players.map((pl,i)=>{const s=c.suspects.find(x=>x.id===r.picks[i]);const ok=r.picks[i]===c.killer;
    return `<li><b>${esc(pl)}</b> accused ${s?s.name:"nobody"} <span class="${ok?"tick":"cross"}">${ok?"✓ correct":"✗ wrong"}</span></li>`}).join("");
  const missedL=c.leads.filter(l=>!p.opened.includes(l.id));
  const missedC=c.links.filter((L,i)=>!p.links.some(l=>l.i===i));
  return `<article class="sheet">
    <div class="dochead"><span class="meta">File ${c.ref}<br>Review outcome</span><span class="stamp thump ${r.solved?"ok":""}">${r.solved?"Case solved":"Still unsolved"}</span></div>
    <div class="scorerow"><span class="score">${r.score}<small> / 100</small></span><span class="stars big">${stars({score:r.score,solved:r.solved})}</span></div>
    <h3>Inquiry rated: ${caseRank(r.score)}</h3>
    ${r.comms.length?`<div class="comms">${r.comms.map(x=>`<span class="comm">${COMMS[x]}</span>`).join("")}</div>`:""}<hr>
    <ul class="plain">${rows}</ul>
    <div class="tw"><table class="rec"><tr><th>Scored for</th><th>Points</th></tr>
    <tr><td>Naming the right person</td><td>${r.who} / 46</td></tr>
    <tr><td>First team question ${r.mOK?"✓":"✗"}</td><td>${r.mOK?12:0} / 12</td></tr>
    <tr><td>Second team question ${r.hOK?"✓":"✗"}</td><td>${r.hOK?12:0} / 12</td></tr>
    <tr><td>Connections on the board (${p.links.length} of ${c.links.length})</td><td>${r.links} / 20</td></tr>
    <tr><td>Map work</td><td>${r.mp} / 10</td></tr></table></div>
  </article>
  <article class="sheet"><div class="hd">${portrait(c.killer)}<div><span class="meta">What really happened</span><h2>${k.name}</h2></div></div><hr>
    <div class="docbody">${c.solution.map(x=>`<p>${x}</p>`).join("")}</div></article>
  <article class="sheet"><span class="meta">The map: ${m.name}</span><div class="mapframe in">${mapSVG(m,p,true)}</div>
    <ol class="plain">${m.route.order.map((id,i)=>`<li><b>${m.pins.find(q=>q.id===id).name}.</b> ${m.route.notes[i]}</li>`).join("")}</ol></article>
  ${missedC.length?`<article class="sheet"><span class="meta">Connections you did not string</span><ul class="plain">${missedC.map(L=>`<li>${L.t}</li>`).join("")}</ul></article>`:""}
  ${missedL.length?`<article class="sheet"><span class="meta">Leads you never opened</span><ul class="plain">${missedL.map(l=>`<li>${l.title}</li>`).join("")}</ul></article>`:""}
  <div class="rowbtns"><button class="btn" data-act="home">Back to the files</button><button class="btn line" data-act="viewboard">Look at your board</button></div>`;
}

/* ============================== RENDER + ACTIONS ============================== */
function render(){
  const v={home:vHome,brief:vBrief,case:vCase,doc:vDoc,accuse:vAccuse,reveal:vReveal}[S.screen]||vHome;
  $("#app").innerHTML=v();
  $("#app").className="scr-"+S.screen+(S.screen==="case"?" tab-"+S.tab:"");
  initMedia();initBoard();save();
}
document.addEventListener("click",e=>{
  const b=e.target.closest("[data-act]");if(!b)return;
  const a=b.dataset.act,id=b.dataset.id;
  if(a==="maptap"){mapTap(e);return}
  if(a==="pin"){e.stopPropagation();pinTap(id,e);return}
  if(a==="rm"){S.players.splice(+b.dataset.i,1);render()}
  else if(a==="install"){if(DEFER){DEFER.prompt();DEFER.userChoice.finally(()=>{DEFER=null;render()})}}
  else if(a==="sound"){S.sound=!S.sound;render();sfx("tap")}
  else if(a==="home")go("home",{pending:null,ask:null})
  else if(a==="reset"){S.ask=id;render()}
  else if(a==="resetno"){S.ask=null;render()}
  else if(a==="resetyes"){delete S.prog[id];S.ask=null;render()}
  else if(a==="open"){
    if(!S.players.length){toast("Add at least one detective first.");$("#pname").focus();return}
    S.caseId=id;const p=S.prog[id];sfx("paper");
    if(p&&p.result)go("reveal");else if(p&&p.opened.length)go("case",{tab:"leads"});else go("brief")}
  else if(a==="start"){P();go("case",{tab:"leads"})}
  else if(a==="tocase")go("case",{tab:S.screen==="doc"?S.docFrom:S.tab,doc:null})
  else if(a==="toboard")go("case",{tab:"board",doc:null})
  else if(a==="viewboard")go("case",{tab:"board"})
  else if(a==="tab"){S.tab=id;S.pending=null;sfx("tap");render();window.scrollTo(0,0)}
  else if(a==="ask"){if(left()>0){S.pending=id;render()}}
  else if(a==="cancel"){S.pending=null;render()}
  else if(a==="spend"){const p=P();if(!p.opened.includes(id)){p.opened.push(id);sfx("pin")}go("doc",{doc:id,docFrom:"leads",pending:null})}
  else if(a==="read"){sfx("paper");go("doc",{doc:id,docFrom:b.dataset.from||"leads"})}
  else if(a==="mark"){const p=P();p.marks[id]=p.marks[id]===b.dataset.m?"":b.dataset.m;render()}
  else if(a==="desel"){P().sel=null;render()}
  else if(a==="zoom"){const p=P();p.zoom=(p.zoom+1)%3;render()}
  else if(a==="rundo"){const p=P();p.route.pop();p.routeMsg="";render()}
  else if(a==="rclear"){const p=P();p.route=[];p.routeMsg="";render()}
  else if(a==="rcheck"){checkRoute();render()}
  else if(a==="alock"){lockAnchor();sfx("pin");render()}
  else if(a==="accuse"){if(P().result){go("reveal");return}go("accuse",{vote:{i:0,ready:false,picks:[],motive:null,how:null,step:"who"}})}
  else if(a==="vready"){S.vote.ready=true;render()}
  else if(a==="pick"){S.vote.picks[S.vote.i]=id;sfx("tap");render()}
  else if(a==="vnext"){const v=S.vote;if(v.i<S.players.length-1){v.i++;v.ready=false}else v.step="motive";render();window.scrollTo(0,0)}
  else if(a==="topt"){if(S.vote.step==="motive")S.vote.motive=id;else S.vote.how=id;sfx("tap");render()}
  else if(a==="tnext"){if(S.vote.step==="motive"){S.vote.step="how";render();window.scrollTo(0,0)}else finish()}
});
document.addEventListener("submit",e=>{
  if(e.target.id==="addform"){e.preventDefault();const n=$("#pname").value.trim();if(n&&S.players.length<8){S.players.push(n);render();$("#pname").focus()}}
});
document.addEventListener("input",e=>{if(e.target.id==="notes"){P().notes=e.target.value;save()}});

function start(data){
  load();
  if(data&&data.players)S=Object.assign(S,data);
  if(window.__PRE)window.__PRE(S);
  if(S.screen!=="home"&&!C())S.screen="home";
  if(S.screen==="reveal"&&!P().result)S.screen="case";
  if(S.screen==="doc"&&(!S.doc||!lead(S.doc)))S.screen="case";
  if(S.screen==="accuse"&&!S.vote)S.screen="case";
  render();
}
window.claude?.hot?.snapshot?.(()=>S);
if(window.claude?.hot?.ready)window.claude.hot.ready(start);else start(window.claude?.hot?.data??{});
