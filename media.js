/* ============================== MEDIA ============================== */
const PEOPLE={
 v_lock:{n:"Alan Pryce",s:"#d9b08c",h:"#8d8d86",st:"short",top:"#27303d",mo:1,old:1,e:"94"},
 hallam:{s:"#e0b592",h:"#5a4030",st:"balding",top:"#3f5a44",e:"94"},
 maureen:{s:"#e6bfa0",h:"#3a2a20",st:"bob",top:"#dfe7ee",e:"94"},
 tony:{s:"#c99a74",h:"#1c1a18",st:"short",top:"#4a6488",e:"94"},
 sheila:{s:"#e8c4a8",h:"#c9b27a",st:"curly",top:"#7a2f3a",old:1,e:"94"},
 colin:{s:"#d6a987",h:"#9a9a94",st:"cap",top:"#5b5a3c",gl:1,old:1,e:"94"},
 v_bowls:{n:"Margaret Ellwood",s:"#e8c9b0",h:"#b9b9b4",st:"bun",top:"#9a86b0",gl:1,old:1,e:"87"},
 kemp:{s:"#dcb596",h:"#a9a9a2",st:"balding",top:"#232c44",gl:1,tie:1,old:1,e:"87"},
 paula:{s:"#e2bb9c",h:"#4a3020",st:"long",top:"#8a5a3a",e:"87"},
 farrow:{s:"#d8ae8e",h:"#4a4a48",st:"short",top:"#6b5a40",gl:1,tie:1,e:"87"},
 jean:{s:"#c89870",h:"#3a2418",st:"curly",top:"#e9e9e4",e:"87"},
 doyle:{s:"#e0bca2",h:"#d8d8d2",st:"bald",top:"#6a5a4a",mo:1,old:1,e:"87"},
 v_platform:{n:"Daniel Reeve",s:"#dfb694",h:"#5a3a22",st:"short",top:"#a8231c",e:"03"},
 bannister:{s:"#e2b99c",h:"#b0b0aa",st:"short",top:"#2b2f38",tie:1,old:1,e:"03"},
 mark:{s:"#d2a47e",h:"#161514",st:"short",top:"#7b8088",e:"03"},
 leah:{s:"#ecc8ac",h:"#d8c07a",st:"long",top:"#2a2a2e",e:"03"},
 pete:{s:"#a8744e",h:"#1d1b1a",st:"balding",top:"#e07a1f",e:"03"},
 susan:{s:"#e4bea2",h:"#8a3d24",st:"bob",top:"#2f5d5a",gl:1,e:"03"}
};
function portrait(k){
  const o=PEOPLE[k];if(!o)return"";
  if(typeof PHOTOS!=="undefined"&&PHOTOS.has(k))return `<img class="ph photo e${o.e}" src="img/${k}.jpg" alt="File photo" draggable="false">`;
  const cap="M33 60 Q29 25 60 26 Q91 25 87 60 Q85 44 73 39 Q56 35 38 44 Q35 50 33 60Z";
  const face="M35 56 Q35 30 60 30 Q85 30 85 56 Q85 76 75 88 Q67 96 60 96 Q53 96 45 88 Q35 76 35 56Z";
  let hair="";
  if(o.st==="short")hair=`<path d="${cap}"/>`;
  if(o.st==="balding")hair=`<path d="M33 66 Q30 44 41 38 Q37 50 38 66Z"/><path d="M87 66 Q90 44 79 38 Q83 50 82 66Z"/>`;
  if(o.st==="long")hair=`<path d="${cap}"/><path d="M33 54 Q25 92 34 112 L46 105 Q38 84 38 54Z"/><path d="M87 54 Q95 92 86 112 L74 105 Q82 84 82 54Z"/>`;
  if(o.st==="bob")hair=`<path d="${cap}"/><path d="M33 54 Q28 80 38 90 L42 80 Q38 70 38 54Z"/><path d="M87 54 Q92 80 82 90 L78 80 Q82 70 82 54Z"/>`;
  if(o.st==="curly")hair=[[38,44],[46,34],[60,30],[74,34],[82,44],[34,57],[86,57],[52,30],[68,30]].map(p=>`<circle cx="${p[0]}" cy="${p[1]}" r="9.5"/>`).join("");
  if(o.st==="bun")hair=`<path d="${cap}"/><circle cx="60" cy="22" r="9"/>`;
  if(o.st==="cap")hair=`<path d="M33 66 Q31 50 38 46 L38 66Z"/><path d="M87 66 Q89 50 82 46 L82 66Z"/><path d="M31 47 Q33 22 60 22 Q87 22 89 47 L100 51 L29 51Z" fill="#45463c"/>`;
  const sh="M4 150 Q6 114 44 104 L76 104 Q114 114 116 150Z";
  return `<svg class="ph e${o.e}" viewBox="0 0 120 150" role="img" aria-label="File photo"><g filter="url(#photo)">
  <rect width="120" height="150" fill="#aab0aa"/><rect width="120" height="150" fill="url(#bgv)"/>
  <ellipse cx="72" cy="70" rx="34" ry="44" fill="#000" opacity=".18"/>
  <path d="${sh}" fill="${o.top}"/><path d="${sh}" fill="url(#topsh)"/>
  <rect x="49" y="82" width="22" height="28" fill="${o.s}"/><ellipse cx="60" cy="97" rx="15" ry="9" fill="#000" opacity=".4"/>
  ${o.tie?`<path d="M45 104 L60 126 L75 104Z" fill="#e6e6e0"/><path d="M57 109h6l2 28-5 6-5-6z" fill="#4a1a20"/>`:`<path d="M47 104 Q60 116 73 104Z" fill="${o.s}"/><path d="M47 104 Q60 116 73 104Z" fill="#000" opacity=".25"/>`}
  <ellipse cx="34.5" cy="64" rx="4" ry="6.5" fill="${o.s}"/><ellipse cx="85.5" cy="64" rx="4" ry="6.5" fill="${o.s}"/><ellipse cx="85.5" cy="64" rx="4" ry="6.5" fill="#000" opacity=".35"/>
  <path d="${face}" fill="${o.s}"/><path d="${face}" fill="url(#fshade)"/>
  <g fill="${o.h}">${hair}</g><g fill="url(#hairsh)">${hair.replace(/ fill="#45463c"/,"")}</g>
  <ellipse cx="50" cy="60" rx="8" ry="5" fill="#000" opacity=".3"/><ellipse cx="70" cy="60" rx="8" ry="5" fill="#000" opacity=".34"/>
  <ellipse cx="50" cy="60.5" rx="3.6" ry="1.9" fill="#d8d3ca"/><ellipse cx="70" cy="60.5" rx="3.6" ry="1.9" fill="#d8d3ca"/>
  <circle cx="50" cy="60.5" r="2.1" fill="#191310"/><circle cx="70" cy="60.5" r="2.1" fill="#191310"/>
  <g fill="none" stroke-linecap="round">
  <path d="M44 54.5 Q50 52 56 54M64 54 Q70 52 76 54.5" stroke="${o.old?"#6a6862":o.h}" stroke-width="2.6" opacity=".9"/>
  <path d="M57 63 Q54.5 72 56 76" stroke="#000" stroke-width="2.4" opacity=".25"/>
  <path d="M51.5 84 Q60 86.5 68.5 84" stroke="#000" stroke-width="1.6" opacity=".55"/>
  ${o.old?`<path d="M48 44 Q60 42 72 44M47 73 Q44 80 47 87M73 73 Q76 80 73 87" stroke="#000" stroke-width="1.8" opacity=".25"/>`:""}</g>
  <ellipse cx="60" cy="77" rx="6" ry="2.2" fill="#000" opacity=".3"/>
  <path d="M52 84 Q56 81.5 60 83 Q64 81.5 68 84 Q60 90 52 84Z" fill="#6a2525" opacity=".5"/>
  <ellipse cx="60" cy="92" rx="8" ry="2.4" fill="#000" opacity=".16"/>
  ${o.gl?`<g stroke="#101010" stroke-width="2" fill="#fff" fill-opacity=".14"><rect x="41" y="54" width="17" height="12.5" rx="3.5"/><rect x="62" y="54" width="17" height="12.5" rx="3.5"/><path d="M58 59h4M41 58l-6-1M79 58l6-1" fill="none"/></g>`:""}
  ${o.mo?`<path d="M48 80 Q60 72 72 80 Q60 85 48 80Z" fill="${o.h}"/>`:""}</g><rect width="120" height="150" fill="url(#ht)"/></svg>`;
}
const EXTRA={
 lock:{
  fuel:{clip:"garage",cap:"Forecourt camera, Marlow Road garage, 15 Oct 1994, 00:12. A man arrives on foot carrying two fuel cans.",
    rc:[{head:"MARLOW ROAD SERVICE STN",lines:["15-10-94    00:12","TILL 1   OP 03","--------------------",["DERV  2 CANS","10.00"],["TOTAL","£10.00"],["CHEQUE","10.00"],"A/C NAME  D HALLAM","--------------------","THANK YOU. DRIVE SAFELY"]}]},
  casino:{clip:"casino",cap:"Door camera, Regal Casino, 14 Oct 1994, 22:05. Member 4471 signs the door book."}
 },
 bowls:{
  hobbs:{clip:"camcorder",cap:"Camcorder tape filmed by the club secretary at the reception, found in 2019. Guests fetch their own sherry. One man in a dark suit carries a glass to a woman seated at Table 2."},
  paula_int:{rc:[{kind:"ticket",head:"BRITISH RAIL",lines:[["STD SINGLE","ADULT"],["LEEDS","→ STANTON ROAD"],["20 NOV 87","dep 17:10"],["PRICE","£6.40"]]}]},
  accounts:{rc:[{head:"MIDSHIRE BANK  PAYING-IN SLIP",lines:["16 NOV 1987","A/C  LITTLE STANTON BOWLS CLUB","--------------------",["CASH  £20 NOTES","1,840.00"],["TOTAL","£1,840.00"],"PAID IN BY  R KEMP"]}]}
 },
 platform:{
  cctv:{clip:"platform",cap:"Wexborough station, 6 Mar 2003. Ticket hall 21:47, Platform 2 at 21:50, boarding at 21:58. The face is never shown."},
  height:{clip:"height",cap:"Still from Platform 2 at 21:50 with the 2023 measurement. Solid line: the figure. Dashed line: Daniel Reeve's height."},
  receipt:{rc:[{head:"GOLDEN BOWL",lines:["2 TANNER'S ROW  WEXBOROUGH","06/03/03    17:22","--------------------",["1 x CHK CHOW MEIN","4.20"],["TOTAL","£4.20"],"DEBIT CARD  ************4410","MR D REEVE","--------------------","THANK YOU"]}]},
  bank:{rc:[{head:"CASH WITHDRAWAL",lines:["ATM 0417  TANNER'S ROW","06/03/03    23:20","CARD ************4410",["AMOUNT","£200.00"],"PIN VERIFIED"]}]},
  bannister_f:{rc:[{head:"WEXBOROUGH CABS",lines:["06/03/03    22:40","FROM  TOWN HALL","TO    ELM GROVE",["FARE","£6.80"],"DRIVER 22"]}]},
  jacket:{rc:[{kind:"ticket",head:"RAIL TICKET",lines:[["STD SINGLE","ADULT"],["LINFIELD","→ WEXBOROUGH"],["06 MAR 03","issued 22:24"],["PRICE  £3.10","CASH"]]}]}
 }
};
const xtra=l=>(EXTRA[S.caseId]||{})[l.id]||{};
function mediaPills(l){const x=xtra(l);return (x.clip?'<span class="pill">Footage</span>':"")+(x.rc?'<span class="pill">Receipt</span>':"")+(x.news?'<span class="pill">Cuttings</span>':"")+(x.photo?'<span class="pill">Photo</span>':"")}
function media(l){const x=xtra(l);let h="";
  if(x.clip)h+=`<figure class="cctv"><canvas data-clip="${x.clip}" width="640" height="480" role="img" aria-label="${esc(x.cap)}"></canvas>${CLIPS[x.clip].dur?'<button class="btn small" type="button">Play footage</button>':""}<figcaption>${x.cap}</figcaption></figure>`;
  if(x.rc)h+=x.rc.map(r=>`<div class="receipt ${r.kind||""}"><b>${r.head}</b>${r.lines.map(n=>Array.isArray(n)?`<span class="rl"><span>${n[0]}</span><span>${n[1]}</span></span>`:`<span>${n}</span>`).join("")}</div>`).join("");
  if(x.photo)h+=`<figure class="print"><img src="img/${x.photo.src}" alt="${esc(x.photo.cap)}"><span>${x.photo.cap}</span></figure>`;
  if(x.news)h+=x.news.map(n=>`<div class="clip"><span class="cp">${n.paper}</span><span class="cd">${n.date}</span><b>${n.head}</b>${n.by?`<i>By ${n.by}</i>`:""}<p>${n.body}</p></div>`).join("");
  return h}

const W=320,H=240;
const lerp=(a,b,t)=>a+(b-a)*Math.max(0,Math.min(1,t));
function box(c,x,y,w,h,col){c.fillStyle=col;c.fillRect(x,y,w,h)}
function person(c,x,y,h,o){
  const sw=o.walk?Math.sin(o.ph*9)*h*0.1:h*0.03;
  c.fillStyle="rgba(0,0,0,.4)";c.beginPath();c.ellipse(x+h*0.08,y,h*0.22,h*0.04,0,0,7);c.fill();
  c.lineCap="round";c.strokeStyle=o.col;c.lineWidth=h*0.07;c.beginPath();c.moveTo(x-h*0.14,y-h*0.78);c.lineTo(x-h*0.17+sw*.6,y-h*0.45);c.moveTo(x+h*0.14,y-h*0.78);c.lineTo(x+h*0.17-sw*.6,y-h*0.45);c.stroke();
  c.strokeStyle=o.legs||"#1c1f24";c.lineWidth=h*0.09;
  c.beginPath();c.moveTo(x,y-h*0.45);c.lineTo(x-sw,y-2);c.moveTo(x,y-h*0.45);c.lineTo(x+sw,y-2);c.stroke();
  c.fillStyle=o.col;c.fillRect(x-h*0.13,y-h*0.82,h*0.26,h*0.42);
  c.fillStyle=o.hood?o.col:(o.head||"#c9b4a0");c.beginPath();c.arc(x,y-h*0.91,h*0.09,0,7);c.fill();
  if(o.hair){c.fillStyle=o.hair;c.beginPath();c.arc(x,y-h*0.93,h*0.09,Math.PI,0);c.fill()}
  if(o.carry==="cans"){box(c,x-h*0.24,y-h*0.42,h*0.1,h*0.14,o.item||"#8a8a84");box(c,x+h*0.14,y-h*0.42,h*0.1,h*0.14,o.item||"#8a8a84")}
  if(o.carry==="glass")box(c,x+h*0.15,y-h*0.6,3,6,"#b5651d");
}
function seated(c,x,y,o){box(c,x-9,y-26,18,26,o.col);c.fillStyle=o.head||"#d8bfa8";c.beginPath();c.arc(x,y-33,7,0,7);c.fill();
  if(o.hair){c.fillStyle=o.hair;c.beginPath();c.arc(x,y-35,7,Math.PI,0);c.fill()}}
function clock(base,t){const s=base+Math.floor(t),p=n=>String(n).padStart(2,"0");return p(Math.floor(s/3600)%24)+":"+p(Math.floor(s/60)%60)+":"+p(s%60)}
let OSDQ=null;function osd(c,label,time,col,right){OSDQ={label,time,col,right}}
const RED={col:"#a8231c",hood:1,legs:"#1c1f24"};
function platBg(c){
  box(c,0,0,W,H,"#33393c");box(c,0,40,W,160,"#4b5356");box(c,0,200,W,40,"#272c2e");box(c,0,224,W,3,"#b9a23a");
  box(c,176,80,48,84,"#d8d4c4");c.strokeStyle="#15181a";c.lineWidth=3;c.strokeRect(176,80,48,84);
  for(let i=0;i<9;i++)box(c,181,88+i*8,38,3,"#6a7a80");
  box(c,20,52,70,16,"#1d2b4a");c.fillStyle="#fff";c.font="9px sans-serif";c.fillText("Platform 2",30,63);
  box(c,250,178,56,5,"#1f2325");box(c,254,183,4,17,"#1f2325");box(c,298,183,4,17,"#1f2325");
}
const CLIPS={
 platform:{dur:15,poster:7,draw(c,t){
   if(t<5){
     box(c,0,0,W,H,"#2f3538");box(c,0,36,W,168,"#565e61");box(c,0,204,W,36,"#3a4043");
     box(c,0,70,34,134,"#0e1113");box(c,236,112,30,92,"#22272a");box(c,241,120,20,16,"#9fd0e0");box(c,241,142,20,6,"#3a4043");
     box(c,110,58,80,14,"#1d2b4a");c.fillStyle="#fff";c.font="9px sans-serif";c.fillText("Tickets",134,68);
     person(c,lerp(-12,220,t/3),204,104,Object.assign({walk:t<3,ph:t},RED));
     osd(c,"CAM 1  TICKET HALL","06-03-03  "+clock(21*3600+47*60+8,t));
   } else if(t<10){
     platBg(c);person(c,150+Math.sin(t*1.3)*1.5,200,104,RED);
     osd(c,"CAM 4  PLATFORM 2","06-03-03  "+clock(21*3600+50*60+31,t-5));
   } else {
     platBg(c);const tx=lerp(330,236,(t-10)/2);
     const fx=lerp(150,tx+22,(t-12)/1.8);
     if(t<13.9)person(c,fx,200,104,Object.assign({walk:t>12&&t<13.8,ph:t},RED));
     box(c,tx,58,140,146,"#c9cdd0");box(c,tx,150,140,10,"#2a4a8a");
     for(let i=0;i<3;i++)box(c,tx+44+i*30,78,22,34,"#e9e6b8");
     box(c,tx+10,74,26,126,t>12&&t<14.3?"#15181a":"#aeb3b6");
     if(t>=12&&t<13.9&&fx>tx+6){c.globalAlpha=lerp(1,0,(t-13.4)/.5);person(c,fx,200,104,Object.assign({walk:1,ph:t},RED));c.globalAlpha=1}
     osd(c,"CAM 4  PLATFORM 2","06-03-03  "+clock(21*3600+57*60+52,t-10));
   }}},
 height:{dur:0,poster:0,draw(c){platBg(c);person(c,150,200,104,RED);osd(c,"CAM 4  PLATFORM 2  STILL","06-03-03  21:50:33")},
  over(c){
   c.strokeStyle="#f0d64a";c.lineWidth=1.5;c.beginPath();c.moveTo(96,96);c.lineTo(232,96);c.stroke();
   c.setLineDash([5,4]);c.strokeStyle="#fff";c.beginPath();c.moveTo(96,87);c.lineTo(232,87);c.stroke();c.setLineDash([]);
   c.font="bold 10px 'Courier New',monospace";c.fillStyle="#fff";c.fillText("REEVE 188cm",238,84);c.fillStyle="#f0d64a";c.fillText("FIGURE 173cm",234,108);
   c.beginPath();c.moveTo(170,80);c.lineTo(170,164);c.stroke();c.fillText("200",148,84);c.fillText("60",154,168)}},
 garage:{dur:9,poster:6.5,mono:1,draw(c,t){
   box(c,0,0,W,H,"#161817");box(c,20,34,230,16,"#5c5f5d");for(let i=0;i<4;i++)box(c,44+i*56,50,22,3,"#d5d8d2");
   box(c,0,196,W,44,"#252726");for(let i=0;i<6;i++)box(c,30+i*50,204+(i%2)*8,26,2,"#3c3f3d");
   box(c,74,150,18,46,"#7c7f7c");box(c,78,156,10,10,"#c9ccc6");box(c,154,150,18,46,"#7c7f7c");box(c,158,156,10,10,"#c9ccc6");
   box(c,256,84,64,112,"#3c3f3e");box(c,264,116,44,36,"#c4c8c2");box(c,256,76,64,8,"#6a6d6a");
   c.strokeStyle="#ffffff22";c.lineWidth=1;for(let i=0;i<40;i++){const x=(i*53+t*140)%W,y=(i*97+t*300)%H;c.beginPath();c.moveTo(x,y);c.lineTo(x-2,y+7);c.stroke()}
   person(c,lerp(-14,246,t/6),198,100,{col:"#5f6260",legs:"#303231",head:"#a3a39d",walk:t<6,ph:t,carry:"cans"});
   osd(c,"FORECOURT","15-10-94  "+clock(12*60+4,t));
 }},
 casino:{dur:8,poster:5.5,mono:1,draw(c,t){
   box(c,0,0,W,H,"#262827");box(c,0,200,W,40,"#171817");box(c,40,56,62,144,"#aeb2ac");box(c,46,62,50,138,"#8d918c");
   box(c,150,60,50,30,"#3a3c3b");c.fillStyle="#b9bcb6";c.font="10px sans-serif";c.fillText("REGAL",158,79);
   seated(c,266,152,{col:"#4f5150",head:"#a9a9a3",hair:"#333"});
   box(c,222,150,90,50,"#4a4c4a");box(c,232,144,26,6,"#d5d8d2");
   person(c,lerp(72,206,t/5),200,100,{col:"#6a6d6b",legs:"#2c2e2d",head:"#a5a59f",hair:"#1e1e1e",walk:t<5,ph:t});
   osd(c,"DOOR CAM","14-10-94  "+clock(22*3600+5*60+12,t));
 }},
 camcorder:{dur:12,poster:6,cam:1,draw(c,t){
   c.translate(Math.sin(t*2.1)*2,Math.cos(t*1.7)*1.5);
   box(c,-5,-5,W+10,H+10,"#c9b99a");box(c,-5,190,W+10,60,"#6b4a35");box(c,-5,120,W+10,4,"#a8946f");
   const fl=["#b3261e","#2f5d8a","#d9b23a","#2e6b45"];for(let i=0;i<14;i++){c.fillStyle=fl[i%4];c.beginPath();c.moveTo(i*24,30);c.lineTo(i*24+16,30);c.lineTo(i*24+8,46);c.fill()}
   box(c,14,150,64,44,"#eee9dd");for(let i=0;i<7;i++)box(c,20+i*8,143,3,7,"#b5651d");
   box(c,196,166,92,28,"#eee9dd");
   seated(c,208,166,{col:"#3f6a5a",hair:"#e2e2dc"});seated(c,238,166,{col:"#9a86b0",hair:"#cfcfca"});
   box(c,196,166,92,6,"#eee9dd");
   const gx=t<4?lerp(330,84,t/4):lerp(84,340,(t-4.6)/3.6);
   person(c,gx,196,84,{col:"#7a3f3f",head:"#d8bfa8",hair:"#5a3a22",walk:t<4||t>4.6,ph:t,carry:t>4.3?"glass":0});
   if(t<9.2)person(c,lerp(70,258,(t-2)/5),196,90,{col:"#1e2026",head:"#d8bfa8",hair:"#4a4a48",walk:t>2&&t<7,ph:t,carry:t<7?"glass":0});
   else seated(c,268,166,{col:"#1e2026",hair:"#4a4a48"});
   if(t>=7)box(c,246,159,3,7,"#b5651d");
   osd(c,"PLAY \u25B6","20.11.1987  19:"+(t<6?"14":"15"),"#ffb347",1);
 }}
};
function post(c,clip,t){
  if(clip.cam){c.fillStyle="rgba(255,170,90,.10)";c.fillRect(0,0,W,H)}
  else{c.globalCompositeOperation="saturation";c.fillStyle=clip.mono?"#808080":"rgba(128,128,128,.62)";c.fillRect(0,0,W,H);c.globalCompositeOperation="source-over";
    if(clip.mono){c.fillStyle="rgba(70,100,85,.10)";c.fillRect(0,0,W,H)}
    c.fillStyle="rgba(0,0,0,.10)";for(let y=0;y<H;y+=2)c.fillRect(0,y,W,1);
    c.fillStyle="rgba(255,255,255,.045)";c.fillRect(0,(t*37)%(H+30)-30,W,26)}
  for(let i=0;i<520;i++){c.fillStyle=i%2?"rgba(255,255,255,.08)":"rgba(0,0,0,.10)";c.fillRect(Math.random()*W,Math.random()*H,1+Math.random()*2,1)}
  c.fillStyle="rgba(0,0,0,"+(Math.random()*.05)+")";c.fillRect(0,0,W,H);
  const g=c.createRadialGradient(W/2,H/2,H*0.3,W/2,H/2,H*0.85);g.addColorStop(0,"rgba(0,0,0,0)");g.addColorStop(1,"rgba(0,0,0,.6)");c.fillStyle=g;c.fillRect(0,0,W,H);
}
function initMedia(){
  document.querySelectorAll("canvas[data-clip]").forEach(cv=>{
    const clip=CLIPS[cv.dataset.clip],c=cv.getContext("2d"),btn=cv.parentNode.querySelector("button");
    const off=document.createElement("canvas");off.width=128;off.height=96;const oc=off.getContext("2d");
    let t=clip.poster||0,playing=false,started=false,last=0;
    function draw(){const td=Math.floor(t*8)/8;OSDQ=null;
      oc.setTransform(.4,0,0,.4,0,0);oc.clearRect(0,0,W,H);oc.save();clip.draw(oc,td);oc.restore();
      c.setTransform(2,0,0,2,0,0);c.imageSmoothingEnabled=true;c.imageSmoothingQuality="high";c.drawImage(off,0,0,W,H);
      post(c,clip,t);if(clip.over)clip.over(c);
      if(OSDQ){c.font="bold 12px 'Courier New',monospace";c.fillStyle=OSDQ.col||"#f0f0f0";c.shadowColor="#000";c.shadowBlur=3;c.fillText(OSDQ.label,10,18);
        const w=c.measureText(OSDQ.time).width;c.fillText(OSDQ.time,OSDQ.right?W-w-10:10,H-10);c.shadowBlur=0}}
    function loop(now){if(!cv.isConnected)return;const dt=Math.min(.1,(now-last)/1000);last=now;
      if(playing){t+=dt;if(t>=clip.dur){t=clip.dur-.01;playing=false;started=false;btn.textContent="Replay footage"}}
      draw();if(playing)requestAnimationFrame(loop)}
    if(btn)btn.onclick=()=>{
      if(playing){playing=false;btn.textContent="Resume footage";return}
      if(!started){t=0;started=true}
      playing=true;btn.textContent="Pause footage";last=performance.now();requestAnimationFrame(loop)};
    draw();
  });
}

