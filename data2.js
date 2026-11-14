/* ============================== CASE 4: THE FRIDAY SERIES ============================== */
CASES.push({
  id:"series", ref:"MS/99/0044", title:"The Friday Series", level:"Chief Inspector", mins:"60 to 75 min",
  place:"Marlow district", date:"March 1998 to January 1999",
  blurb:"Four people, four towns, four Friday nights. The press called him the Friday Man. The answer is in the map and the dates. Darker theme, best for older players.",
  brief:[
    "Between March 1998 and January 1999, four people were killed in four small towns around the market town of Marlow. Each was alone and on foot late on a Friday night. Each was found beside a quiet path the next morning. The pathologist was certain it was the same offender every time.",
    "The inquiry, Operation Lantern, interviewed more than 900 people and named no one. The County Herald called him <b>the Friday Man</b>. After January 1999 the killings stopped.",
    "The four scenes are already pinned on your map. A 2021 review has retested the exhibits and reopened the paperwork.",
    "<b>Your job:</b> find the pattern in the map and the dates. Work out where he was based, what decided which Fridays he struck, and name him."
  ],
  budget:10,
  suspects:[
    {id:"dunmore", name:"Ray Dunmore", facts:"46 · Market trader, fabrics · Lives Hollin Bridge", note:"Works a different market each day from a white Transit van. His pitch book puts him at a Friday market within five miles of three of the four scenes."},
    {id:"garrity", name:"Neil Garrity", facts:"39 · Bus driver, Route 47 · Lives Norton Mill", note:"Route 47 runs a late Friday service through all four towns. Two of the victims were carrying tickets for it."},
    {id:"spence", name:"Carl Spence", facts:"34 · Window fitter · Lives Kelham", note:"Owns a white van with a ladder rack. A van like it was seen close to the Norton Mill scene."},
    {id:"lamb", name:"Stuart Lamb", facts:"29 · Night security guard · Lives Marlow", note:"Guards the Norton Mill bus depot. Cautioned in 1996 for following a stranger home. Works every Friday night."},
    {id:"pike", name:"Graham Pike", facts:"52 · Chief crime reporter, County Herald · Lives Marlow", note:"First reporter at every scene. Gave the series its name. His coverage won a regional press award in 1999."}
  ],
  leads:[
    {id:"incA", cat:"Scenes", title:"File A: Ashby Cross", sub:"Carol Whitt, 13 March 1998.", type:"Incident file", ref:"MS/98/0311", date:"14 Mar 1998",
     body:`<p><b>Carol Whitt, 31</b>, bar staff at the Barge Inn. Left work at 23:15 on Friday 13 March to walk half a mile home along the canal path.</p>
<p>Found at 06:05 on Saturday beside the footbridge by a man walking his dog.</p>
<p>Handbag and purse untouched. Coat pocket: a Route 47 bus ticket issued at 17:52, her journey to work.</p>
<p>Property list, 1998: "No clothing recorded as missing."</p>
<p>No witnesses. No vehicle seen.</p>`},
    {id:"incB", cat:"Scenes", title:"File B: Norton Mill", sub:"Dennis Okafor, 5 June 1998.", type:"Incident file", ref:"MS/98/0688", date:"6 Jun 1998",
     body:`<p><b>Dennis Okafor, 27</b>, staff nurse. Left the Mill Road clinic at 22:40 on Friday 5 June to walk to his flat.</p>
<p>Found at 06:50 on Saturday behind the bus depot wall by a cleaner.</p>
<p>Post-mortem note: left glove missing. He was seen leaving work wearing both. <b>Detail withheld from the press.</b></p>
<p>Witness, a taxi driver: "About ten to eleven there was a white van with a ladder rack parked on Mill Lane with no lights on. I thought nothing of it."</p>
<p>Bus pass for Route 47 in his wallet.</p>`},
    {id:"incC", cat:"Scenes", title:"File C: Kelham", sub:"Ruth Marsden, 25 September 1998.", type:"Incident file", ref:"MS/98/1140", date:"26 Sep 1998",
     body:`<p><b>Ruth Marsden, 44</b>, teacher. Left the quiz night at the Plough at 23:00 on Friday 25 September. It was raining. Friends saw her put up a green umbrella.</p>
<p>Found at 07:10 on Saturday at the allotment gate.</p>
<p>Umbrella never found. <b>Detail withheld from the press.</b></p>
<p>Tape lifts taken from the back of her coat: a number of short maroon fibres. Sent to the lab.</p>`},
    {id:"incD", cat:"Scenes", title:"File D: Hollin Bridge", sub:"Paul Fry, 22 January 1999.", type:"Incident file", ref:"MS/99/0044", date:"23 Jan 1999",
     body:`<p><b>Paul Fry, 38</b>, shop manager. Left a friend's house at 23:30 on Friday 22 January to walk home through the churchyard.</p>
<p>Found at 07:30 on Saturday on the churchyard path.</p>
<p>Flat cap missing. <b>Detail withheld from the press.</b></p>
<p>Witness, a woman letting her dog out at about midnight: "A dark red saloon car came backwards out of Church Lane, fast, with no lights. The number plate started with a P. There was a 2 in it and a K."</p>`},
    {id:"fibre", cat:"Forensics", title:"Fibre report", sub:"The maroon fibres, with a 2021 retest.", requires:"incC", type:"Laboratory report", ref:"MS/21/0207/L1", date:"1998 and 2021",
     body:`<p>Maroon tufted nylon fibres recovered from the coats of Ruth Marsden (C) and Paul Fry (D).</p>
<p>2021 retest: the same fibres are present on Carol Whitt's cardigan (A). They were missed in 1998.</p>
<p>Source: the fibre matches the boot carpet fitted to Rover 200 and 400 series saloon cars built between 1995 and 1999.</p>
<p>Comment: vans of the period have rubber or bare plywood load floors. <b>These fibres did not come from a van.</b> The victims were in contact with the boot of a saloon car.</p>`},
    {id:"profile", cat:"Forensics", title:"Geographic profile", sub:"What the pattern on the map says about him.", type:"Analyst's report", ref:"MS/21/0207/4", date:"12 Mar 2021",
     body:`<p>Most repeat offenders of this kind travel out from one fixed base along roads they know, then go straight back. They avoid their own doorstep, where they might be recognised.</p>
<p>Method: draw a circle through the two scenes that are furthest apart. In most cases the offender's base is inside that circle, and close to the middle of the pattern.</p>
<p>Here, each scene is in a different town. A base in any one of those four towns would put three scenes far away and one on his doorstep. That is unlikely.</p>
<p>All four scenes are 20 to 25 minutes by main road from one place.</p>
<p>Timing: the gaps are 12, 16 and 17 weeks. He does not strike every Friday, or every month. <b>Something in his working life decides which Fridays he is free, mobile and unobserved.</b></p>`},
    {id:"articles", cat:"Records", title:"County Herald cuttings", sub:"How the series was reported.", type:"Press cuttings", ref:"MS/98/PR/1-3", date:"Mar to Sep 1998",
     body:`<p>Three cuttings from the Operation Lantern press file. All three stories carry the same byline.</p>`},
    {id:"presslog", cat:"Records", title:"Police press office log", sub:"What was released, and when police knew it.", requires:"articles", type:"Policy log", ref:"MS/98/PO/7", date:"Jun to Jul 1998",
     body:`<p><b>9 June 1998.</b> "Monday's Herald reports that items of clothing were taken. No detail of missing clothing has been released by this office and none will be. Leak inquiry opened."</p>
<p><b>22 June 1998.</b> "Leak inquiry: 41 officers and staff asked. None admits speaking to Mr Pike. Mr Pike declines to name his source."</p>
<p><b>14 July 1998.</b> "Carol Whitt's sister attended to collect property. She states that Carol's blue scarf, which she always wore to work, is not among it and has never been found. <b>Until today the incident room did not know anything had been taken in March.</b>"</p>
<p class="hand">Handwritten beside the entry: "Then how did the Herald print it five weeks ago? Action?"</p>
<p>No action is recorded.</p>`},
    {id:"rota", cat:"Records", title:"Route 47 driver rota", sub:"Who drove the late Friday bus.", type:"Staff record", ref:"MS/99/0044/31", date:"1998 to 1999",
     body:`<div class="tw"><table class="rec"><tr><th>Friday</th><th>Late service driver</th></tr>
<tr><td>13 Mar 1998</td><td>N. Garrity</td></tr>
<tr><td>5 Jun 1998</td><td>N. Garrity</td></tr>
<tr><td>25 Sep 1998</td><td>Relief driver. N. Garrity on annual leave 19 Sep to 3 Oct.</td></tr>
<tr><td>22 Jan 1999</td><td>N. Garrity</td></tr></table></div>
<p>Garrity's passport: Spanish entry stamp 19 September 1998, exit stamp 3 October. Hotel bill, Málaga, fourteen nights.</p>
<p>On the other three nights the tachograph shows the bus moving on schedule until 00:20, then parked at the depot.</p>`},
    {id:"clock", cat:"Records", title:"Depot patrol clock", sub:"Stuart Lamb's night shifts.", type:"Security record", ref:"MS/99/0044/36", date:"1998 to 1999",
     body:`<p>The Norton Mill depot guard carries a watchman's clock and must turn a key in it at six fixed points around the site every 30 minutes. The paper tape cannot be altered afterwards.</p>
<p>Tapes for 13 March, 5 June, 25 September 1998 and 22 January 1999: every station punched, every half hour, 22:00 to 06:00. Guard on duty each night: S. Lamb.</p>
<p>Kelham and Hollin Bridge are each about 15 miles from the depot. Neither can be reached and returned from inside 30 minutes.</p>`},
    {id:"spence", cat:"Interviews", title:"Carl Spence interview", sub:"The white van with the ladder rack.", type:"Interview record", ref:"MS/98/0688/12", date:"1998 and 1999",
     body:`<p>"Yes, that was my van on Mill Lane on the fifth of June. I was fitting two windows at the Miller's Arms after closing, cash in hand, which is why I did not come forward. The landlord was with me from half ten till one."</p>
<p>Landlord confirms, and produces the windows.</p>
<p>Marlow General Hospital: C. Spence admitted 18 January 1999 with a broken leg after a fall from a ladder. Discharged 27 January. He was on Ward 3, in plaster, on the night of 22 January.</p>`},
    {id:"dunmore", cat:"Interviews", title:"Ray Dunmore file", sub:"The market trader and his van.", type:"Interview record and search", ref:"MS/99/0044/40", date:"Feb 1999",
     body:`<p>Pitch book confirms Friday markets near three scenes on the relevant dates.</p>
<p>24 to 27 September 1998: Dunmore had a stand at a trade fair at the NEC, Birmingham, 90 miles away. Stand pass and hotel bill produced. The hotel's car park log shows his van did not leave.</p>
<p>Van examined February 1999: bare plywood load floor, no carpet. Stock is cotton and polyester dress fabric. No maroon nylon of any kind.</p>
<p>"I am on the road six days a week. Of course I was near. So were ten thousand other people."</p>`},
    {id:"plate", cat:"Records", title:"Vehicle inquiry", sub:"A dark red saloon, P registration, a 2 and a K.", requires:"incD", type:"Vehicle records", ref:"MS/21/0207/9", date:"2021",
     body:`<p>Search: dark red or maroon saloon cars registered in Marlow district with a P prefix and both a 2 and a K in the plate.</p>
<div class="tw"><table class="rec"><tr><th>Plate</th><th>Car</th><th>Keeper</th><th>Notes</th></tr>
<tr><td>P382 KTV</td><td>Rover 214, maroon</td><td>Mrs E. Hallett, 81</td><td>Keeper in a care home from 1997. Car on bricks in a garage, no battery.</td></tr>
<tr><td>P526 KAU</td><td>Ford Mondeo, dark red</td><td>Private</td><td>Written off in a crash, November 1998. Not a Rover.</td></tr>
<tr><td>P427 KNN</td><td>Rover 416, Nightfire Red</td><td>County Herald Newspapers Ltd</td><td>Newsroom pool car. Sold at auction March 1999. Never examined.</td></tr></table></div>`},
    {id:"roster", cat:"Records", title:"Herald late-call roster", sub:"Which reporter was on duty each Friday night.", type:"Staff roster", ref:"MS/21/0207/11", date:"1998 to 1999",
     body:`<p>County Herald newsroom. The late-call reporter covers police, fire and ambulance calls from 18:00 to 01:00, alone, and has the use of the pool car.</p>
<p>Junior reporters share most Fridays. The chief crime reporter takes one late call each quarter.</p>
<div class="tw"><table class="rec"><tr><th>Quarter</th><th>G. Pike's late call</th></tr>
<tr><td>Jan to Mar 1998</td><td>Friday 13 March</td></tr>
<tr><td>Apr to Jun 1998</td><td>Friday 5 June</td></tr>
<tr><td>Jul to Sep 1998</td><td>Friday 25 September</td></tr>
<tr><td>Oct to Dec 1998</td><td>Swapped. Taken in January.</td></tr>
<tr><td>Jan 1999</td><td>Friday 22 January</td></tr></table></div>
<p>Pike left the Herald in April 1999 and moved abroad.</p>`},
    {id:"porter", cat:"Records", title:"Night porter's book", sub:"Herald House front desk, signed in and out.", type:"Exhibit", ref:"MS/21/0207/E3", date:"1998 to 1999",
     body:`<div class="tw"><table class="rec"><tr><th>Date</th><th>Entry</th></tr>
<tr><td>13 Mar 1998</td><td>G. Pike took pool car keys 22:05. Keys back 00:55.</td></tr>
<tr><td>5 Jun 1998</td><td>G. Pike, pool car, out 22:10. Back 00:40.</td></tr>
<tr><td>25 Sep 1998</td><td>G. Pike, pool car, out 22:20. Back 01:05. Car wet and muddy.</td></tr>
<tr><td>22 Jan 1999</td><td>G. Pike, pool car, out 22:45. Back 01:10.</td></tr></table></div>
<p class="hand">Porter's note, March 1998: "Mr Pike says he likes to do his police calls in person."</p>
<p>On six other Fridays sampled, the junior reporter on late call did not take the car at all.</p>`},
    {id:"pike_int", cat:"Interviews", title:"Graham Pike, 1999 interview", sub:"Seen as a witness, about his source.", type:"Interview record", ref:"MS/99/0044/52", date:"3 Mar 1999",
     body:`<p class="q">Q. Who told you about the missing clothing?</p>
<p>"A police source. I will go to prison before I name him."</p>
<p class="q">Q. What do you do on a late call?</p>
<p>"I sit by the newsroom phone and ring round the stations every hour. I do not go out unless something breaks, and on those nights nothing broke until the morning."</p>
<p class="q">Q. What do you drive?</p>
<p>"My own car. A Volvo. Blue."</p>
<p class="q">Q. You were first at every scene.</p>
<p>"That is the job. I gave him his name. Somebody had to."</p>`}
  ],
  killer:"pike",
  whyQ:"What decided which Fridays he struck?",
  motives:[
    {id:"a", t:"They were his late-call nights, alone with the pool car", ok:true},
    {id:"b", t:"They were Friday market days in each town"},
    {id:"c", t:"They were the nights the late bus ran"},
    {id:"d", t:"They were the nights the depot was unguarded"}
  ],
  howQ:"Which detail could only have come from the killer?",
  hows:[
    {id:"a", t:"He printed the missing scarf five weeks before the police knew it was gone", ok:true},
    {id:"b", t:"A white van was seen near one of the scenes"},
    {id:"c", t:"Two victims carried Route 47 tickets"},
    {id:"d", t:"He lived in one of the four towns"}
  ],
  solution:[
    "<b>Graham Pike was the Friday Man.</b> He was the Herald's chief crime reporter, and four times in ten months he reported on killings he had carried out himself.",
    "The map points at Marlow. Each scene is in a different town, about 20 minutes from the centre by a main road. The other suspects live on the edge of the pattern. Pike worked in the middle of it.",
    "The dates are his. The chief reporter took one late call a quarter: 13 March, 5 June, 25 September and 22 January. Those are the four Fridays. On each one the night porter logged him out with the pool car for about three hours, although he told police he sat by the phone.",
    "The pool car was a maroon Rover 416, plate P427 KNN: the dark red saloon with a P, a 2 and a K, and the source of the boot-carpet fibres on three victims. It was sold two months after the last killing.",
    "And he gave himself away in print. On 8 June 1998 he wrote that the killer had taken Carol Whitt's blue scarf. The police did not learn the scarf was missing until her sister told them on 14 July.",
    "Garrity was in Spain for Kelham. Lamb's patrol clock was punched every half hour. Spence was fitting windows, and later in hospital. Dunmore was in Birmingham and his van has no carpet."
  ]
});

Object.assign(PEOPLE,{
 v_series0:{n:"Carol Whitt",s:"#e6c0a4",h:"#6a3a22",st:"long",top:"#3a4a6a",e:"98"},
 v_series1:{n:"Dennis Okafor",s:"#8a5a3c",h:"#141210",st:"short",top:"#4a7a8a",e:"98"},
 v_series2:{n:"Ruth Marsden",s:"#e2bea4",h:"#5a4a40",st:"bob",top:"#7a3a4a",gl:1,e:"98"},
 v_series3:{n:"Paul Fry",s:"#dcb090",h:"#6a5a48",st:"balding",top:"#55604a",mo:1,e:"98"},
 dunmore:{s:"#d8a888",h:"#3a3028",st:"short",top:"#6a4a2a",mo:1,e:"98"},
 garrity:{s:"#e0b898",h:"#7a5a3a",st:"short",top:"#2a3a5a",tie:1,e:"98"},
 spence:{s:"#dcae8c",h:"#b89858",st:"short",top:"#8a8a80",e:"98"},
 lamb:{s:"#c8946c",h:"#1a1816",st:"short",top:"#1c1c20",e:"98"},
 pike:{s:"#e2bca0",h:"#9a9a94",st:"balding",top:"#4a4038",gl:1,tie:1,old:1,e:"98"}
});
EXTRA.series={
  articles:{news:[
    {paper:"County Herald",date:"Monday 16 March 1998",head:"Barmaid found dead on canal path",by:"Graham Pike, Chief Crime Reporter",body:"Police have begun a murder inquiry after the body of Carol Whitt, 31, was found beside the footbridge at Ashby Cross early on Saturday. Detectives say there is no obvious motive."},
    {paper:"County Herald",date:"Monday 8 June 1998",head:"‘Friday Man’ strikes again",by:"Graham Pike, Chief Crime Reporter",body:"A nurse found dead at Norton Mill on Saturday was the second victim of the same killer, the Herald can reveal. The Herald understands the killer took his victim's left glove, just as he took Carol Whitt's blue scarf in March."},
    {paper:"County Herald",date:"Monday 28 September 1998",head:"Third Friday, third town",by:"Graham Pike, Chief Crime Reporter",body:"Teacher Ruth Marsden left a pub quiz in Kelham in the rain on Friday night and never reached home. Once again, sources say, something of hers is missing."}
  ]},
  dunmore:{rc:[{head:"ARDEN COURT HOTEL  NEC",lines:["BIRMINGHAM B40","GUEST  MR R DUNMORE","ARR 24/09/98  DEP 27/09/98","--------------------",["3 NIGHTS  ROOM 214","147.00"],["CAR PARK  VAN","9.00"],["TOTAL","£156.00"],"PAID  SWITCH","--------------------","THANK YOU FOR STAYING"]}]}
};

/* ============================== VICTIMS, QUESTIONS, LINKS, MAPS ============================== */
const ADD={
 lock:{
  victims:[{k:"v_lock",name:"Alan Pryce",sub:"52 · Lock-keeper"}],
  links:[
   {a:["log"],b:["claim"],t:"The boat that 'sank' was afloat at 23:40"},
   {a:["hallam_int"],b:["sheila_st"],t:"He left at 22:30, not at closing time"},
   {a:["hallam_int","s:hallam"],b:["fuel"],t:"Not in bed: buying diesel at 00:12"},
   {a:["hallam_int"],b:["wife"],t:"His alibi witness takes it back"},
   {a:["paint"],b:["log","claim"],t:"Fresh blue paint, and only the Kestrel was afloat"},
   {a:["watch"],b:["log"],t:"Twelve minutes from the last entry to the water"},
   {a:["colin_st"],b:["sheila_st"],t:"Two witnesses: a boat went through after dark"},
   {a:["casino"],b:["tony_int","s:tony"],t:"Tony was 26 miles away",clears:"tony"},
   {a:["rota"],b:["maureen_int","s:maureen"],t:"Maureen never left Ward 6",clears:"maureen"}
  ],
  map:{name:"Harrowby and the canal",seed:11,scale:"1 mile",
   water:[{d:"M-10 40 C60 55 110 110 150 170 S215 240 235 275 S300 370 345 410 S390 470 410 490",w:5,label:"Trent and Marlow Canal",lx:78,ly:112,rot:44}],
   woods:["M30 210 q34 -32 76 -10 q22 32 -10 62 q-42 22 -72 -10z","M296 196 q42 -22 74 10 q10 42 -32 52 q-44 2 -42 -62z","M60 420 q40 -18 66 8 q6 34 -30 40 q-40 -4 -36 -48z"],
   roads:[{d:"M180 -10 C200 80 230 160 255 230 S300 340 318 388 S350 460 360 510",c:"a"},{d:"M40 92 C100 120 170 150 250 218",c:"b"},{d:"M262 250 C290 190 310 140 330 100 S360 40 380 -10",c:"b"},{d:"M160 187 L196 218 L250 228",c:"m"},{d:"M228 265 L258 240",c:"m"},{d:"M205 238 L246 224",c:"m"}],
   rail:["M-10 330 C80 320 160 400 240 470 S300 500 320 510"],
   built:[[246,214,28],[262,244,12],[52,82,9],[334,98,7],[322,382,5]],
   labels:[{t:"HARROWBY",x:292,y:222,c:"town"},{t:"Stoke Reach",x:70,y:104,c:"vil"},{t:"A614",x:302,y:318,c:"rd"},{t:"to Nottingham 26 miles",x:214,y:16,c:"rd"}],
   pins:[
    {id:"stoke",x:45,y:60,name:"Stoke Reach mooring",note:"Where the Kestrel was reported sunk on 29 September."},
    {id:"boatyard",x:160,y:185,name:"Hallam's Boatyard",note:"Half a mile upstream of the lock. Sheds, a slipway and a private wet dock."},
    {id:"anchor",x:204,y:240,name:"The Anchor",note:"Darts night. The yard backs on to the towpath."},
    {id:"lock",x:230,y:268,name:"Harrowby Lock",note:"Lock, cottage and office. Alan was found in the lower basin."},
    {id:"garage",x:320,y:388,name:"Marlow Road garage",note:"All-night garage, a mile and a half below the lock. The canal runs behind it."},
    {id:"hospital",x:332,y:98,name:"County General",note:"Ward 6, where Maureen Pryce worked nights."}
   ],
   route:{q:"String the killer's movements on the night of 14 October, in order.",order:["anchor","boatyard","lock","garage"],
    notes:["22:30. Leaves the darts early.","Takes the Kestrel out of the yard, no lights.","23:40 to 23:52. Alan comes out with his torch.","00:12. On foot, wet to the knee, buying diesel for the boat."]}}
 },
 bowls:{
  victims:[{k:"v_bowls",name:"Margaret Ellwood",sub:"67 · Club treasurer"}],
  links:[
   {a:["cert"],b:["tox"],t:"Signed off as her heart. The lab says overdose."},
   {a:["tox"],b:["register"],t:"Forty tablets missing from the only dispensary"},
   {a:["will"],b:["diary"],t:"She was about to cancel the £40,000"},
   {a:["will"],b:["fund"],t:"The Patient Fund was his own purse"},
   {a:["farrow_int"],b:["hobbs"],t:"He says he never fetches drinks. She watched him."},
   {a:["farrow_int"],b:["seating","kemp_int"],t:"Not chance: he asked to sit beside her"},
   {a:["tox"],b:["doyle_int","s:doyle"],t:"Tablets, not foxgloves",clears:"doyle"},
   {a:["accounts"],b:["kemp_int","s:kemp"],t:"The debt was repaid and the matter closed",clears:"kemp"},
   {a:["paula_int"],b:["evening","s:paula"],t:"She arrived after the dose was given",clears:"paula"},
   {a:["jean_st"],b:["evening","s:jean"],t:"Shared soup cannot be aimed at one guest",clears:"jean"}
  ],
  map:{name:"Little Stanton",seed:23,scale:"¼ mile",
   water:[{d:"M410 330 C330 320 260 360 200 380 S80 400 -10 380",w:3,label:"Stanton Brook",lx:250,ly:372,rot:-14}],
   woods:["M300 320 q40 -20 70 6 q8 36 -30 44 q-42 0 -40 -50z","M20 60 q40 -26 72 0 q10 40 -28 50 q-44 2 -44 -50z"],
   roads:[{d:"M-10 250 C80 240 150 255 210 265 S330 250 410 230",c:"b"},{d:"M300 248 C330 300 350 370 365 430 S380 490 385 510",c:"b"},{d:"M200 262 C190 200 150 190 118 200 S90 150 80 -10",c:"m"},{d:"M230 262 C250 220 270 180 285 160 S330 120 410 90",c:"m"},{d:"M160 258 C155 290 150 320 150 345 S110 420 70 440",c:"m"}],
   rail:["M-10 462 C100 442 250 472 410 452"],
   built:[[205,258,26],[122,202,9],[288,162,7],[318,136,5],[152,330,5],[366,428,10],[76,438,5]],
   labels:[{t:"LITTLE STANTON",x:230,y:296,c:"town"},{t:"to Stanton Road",x:340,y:486,c:"rd"},{t:"Church Lane",x:150,y:180,c:"rd"}],
   pins:[
    {id:"hall",x:212,y:272,name:"Village hall",note:"The annual dinner, Friday 20 November."},
    {id:"surgery",x:118,y:200,name:"The surgery",note:"Dr Farrow's practice and its dispensary."},
    {id:"rose",x:284,y:162,name:"Rose Cottage",note:"Margaret's home. The new will was signed here on 28 October."},
    {id:"doyle",x:322,y:134,name:"Doyle's garden",note:"Next door. Foxgloves along the disputed hedge."},
    {id:"bowls",x:150,y:346,name:"Bowls club",note:"The green and the pavilion."},
    {id:"sol",x:368,y:432,name:"Pargeter and Lowe",note:"Solicitors. Margaret's appointment was for Monday 23 November."},
    {id:"station",x:72,y:442,name:"Stanton Road station",note:"Paula's train from Leeds arrived at 19:42."}
   ],
   route:{q:"String the three places where the plan was carried out, in order.",order:["rose","surgery","hall"],
    notes:["28 October. A home visit, and a new will.","Forty digoxin tablets leave the dispensary.","20 November, 19:00. One glass of sherry."]}}
 },
 platform:{
  victims:[{k:"v_platform",name:"Daniel Reeve",sub:"24 · Trainee reporter"}],
  links:[
   {a:["path"],b:["receipt"],t:"Ate at 17:22, dead by about half past seven"},
   {a:["height"],b:["s:mark","mark_int"],t:"173 cm is Mark's height, not Daniel's"},
   {a:["mark_int"],b:["server"],t:"Not online from six: missing 17:48 to 23:31"},
   {a:["bank"],b:["pobox"],t:"The loans lead to Mark's phone number"},
   {a:["phone"],b:["leah_int"],t:"A text Daniel would never have written"},
   {a:["pete_int"],b:["jacket"],t:"The jacket came back on the 22:31, worn by Mark"},
   {a:["scene"],b:["mark_int","cctv"],t:"Dry shoes: he never walked out into that rain"},
   {a:["susan_int"],b:["bank"],t:"'A problem closer to home'"},
   {a:["bannister_f"],b:["path","s:bannister"],t:"His alibi covers the real time of death",clears:"bannister"},
   {a:["leah_int"],b:["s:leah"],t:"On the till from four until half past midnight",clears:"leah"}
  ],
  map:{name:"Wexborough",seed:37,scale:"½ mile",
   water:[{d:"M-10 420 C120 400 260 440 410 410",w:7,label:"River Wex",lx:200,ly:440,rot:4}],
   woods:["M20 40 q44 -24 76 4 q8 40 -32 48 q-44 0 -44 -52z","M320 440 q40 -10 60 14 q0 30 -34 32 q-34 -6 -26 -46z"],
   roads:[{d:"M-10 260 C40 330 80 380 150 400 S320 380 410 300",c:"a"},{d:"M300 118 C290 170 270 220 250 250 S180 300 120 335",c:"b"},{d:"M120 200 L250 200 L340 230",c:"m"},{d:"M200 150 L200 262 L210 340",c:"m"},{d:"M250 250 L295 292 L360 330",c:"m"},{d:"M90 300 L125 335 L150 372",c:"m"},{d:"M150 150 L150 290",c:"m"},{d:"M250 140 L250 200",c:"m"}],
   rail:["M-10 182 C100 152 220 132 300 118 S380 60 410 20"],
   built:[[205,232,70],[282,262,30],[132,322,26],[298,152,16],[372,58,10],[150,180,18]],
   labels:[{t:"WEXBOROUGH",x:92,y:246,c:"town"},{t:"Linfield",x:346,y:74,c:"vil"},{t:"High Street",x:262,y:186,c:"rd"}],
   pins:[
    {id:"tanner",x:124,y:338,name:"Tanner's Row",note:"The flat at number 14, the lock-up garages behind, a cash machine and the Golden Bowl."},
    {id:"station",x:300,y:120,name:"Wexborough station",note:"Ticket hall, Platform 2 for Linfield, Platform 1 for arrivals."},
    {id:"linfield",x:372,y:50,name:"Linfield station",note:"Twelve minutes up the line."},
    {id:"townhall",x:252,y:200,name:"Town hall",note:"Planning committee, 19:00 to 22:15."},
    {id:"vaults",x:198,y:264,name:"The Vaults",note:"The bar where Leah worked that night."},
    {id:"gazette",x:296,y:294,name:"Gazette offices",note:"Daniel's desk, with his rail pass in the drawer."}
   ],
   route:{q:"String the journey of the red jacket after nine o'clock on 6 March. A place can be used twice.",order:["tanner","station","linfield","station","tanner"],
    notes:["21:20. Leaves the flat in Daniel's jacket, carrying his phone.","21:47. Buys a ticket in cash and is filmed on Platform 2.","22:10. Gets off, buys a single back at 22:24.","22:52. Arrives on Platform 1 and bins the jacket.","23:05. Home. £200 from the cash machine at 23:20."]}}
 },
 series:{
  victims:[{k:"v_series0",name:"Carol Whitt",sub:"A · Ashby Cross"},{k:"v_series1",name:"Dennis Okafor",sub:"B · Norton Mill"},{k:"v_series2",name:"Ruth Marsden",sub:"C · Kelham"},{k:"v_series3",name:"Paul Fry",sub:"D · Hollin Bridge"}],
  links:[
   {a:["articles"],b:["presslog"],t:"He printed the scarf five weeks before police knew"},
   {a:["fibre"],b:["plate"],t:"Maroon boot carpet, maroon Rover"},
   {a:["plate"],b:["porter","roster"],t:"On late call, the pool car was his"},
   {a:["roster"],b:["porter","profile"],t:"His four late calls are the four Fridays"},
   {a:["pike_int"],b:["porter"],t:"'I sit by the phone.' The porter logged him out."},
   {a:["rota"],b:["s:garrity","incC"],t:"Garrity was in Spain for Kelham",clears:"garrity"},
   {a:["clock"],b:["s:lamb"],t:"Punched in every half hour, 15 miles away",clears:"lamb"},
   {a:["spence"],b:["s:spence","incB"],t:"The van was there to fit windows",clears:"spence"},
   {a:["dunmore"],b:["s:dunmore","fibre"],t:"In Birmingham, and no carpet in the van",clears:"dunmore"}
  ],
  map:{name:"Marlow district",seed:51,scale:"5 miles",
   water:[{d:"M-10 130 C50 125 80 115 100 100 S220 30 410 70",w:4,label:"Marlow Canal",lx:210,ly:44,rot:-6},{d:"M-10 300 C100 284 150 268 200 270 S330 304 410 284",w:6,label:"River Marl",lx:300,ly:308,rot:6}],
   woods:["M180 100 q40 -22 70 6 q6 36 -32 42 q-40 -2 -38 -48z","M20 220 q36 -20 62 6 q4 34 -28 40 q-38 -4 -34 -46z","M190 400 q44 -16 70 12 q0 34 -36 36 q-40 -6 -34 -48z","M350 220 q30 -16 50 8 q0 30 -28 32 q-30 -6 -22 -40z"],
   roads:[{d:"M200 250 C170 200 130 150 95 110 S50 40 30 -10",c:"a"},{d:"M200 250 C240 210 280 160 322 118 S370 60 410 30",c:"a"},{d:"M200 250 C240 290 280 340 318 388 S360 450 380 510",c:"a"},{d:"M200 250 C160 290 120 340 82 382 S40 440 20 510",c:"a"},{d:"M95 110 C170 82 250 82 322 118 C352 200 352 300 318 388 C250 422 150 422 82 382 C54 300 54 200 95 110",c:"b"}],
   rail:["M-10 432 C80 402 150 302 200 256 S330 182 410 172"],
   built:[[200,250,70],[95,110,18],[322,118,18],[318,388,18],[82,382,18]],
   labels:[{t:"MARLOW",x:236,y:262,c:"town"},{t:"Ashby Cross",x:118,y:132,c:"vil"},{t:"Norton Mill",x:300,y:142,c:"vil"},{t:"Kelham",x:300,y:372,c:"vil"},{t:"Hollin Bridge",x:112,y:366,c:"vil"}],
   pins:[
    {id:"A",x:86,y:100,name:"A · Ashby Cross",note:"Carol Whitt. Friday 13 March 1998. Canal footbridge."},
    {id:"B",x:332,y:110,name:"B · Norton Mill",note:"Dennis Okafor. Friday 5 June 1998. Behind the bus depot."},
    {id:"C",x:328,y:398,name:"C · Kelham",note:"Ruth Marsden. Friday 25 September 1998. Allotment gate."},
    {id:"D",x:72,y:392,name:"D · Hollin Bridge",note:"Paul Fry. Friday 22 January 1999. Churchyard path."},
    {id:"marlow",x:200,y:250,name:"Marlow",note:"Market town. Police headquarters, the hospital and the County Herald are all here."}
   ],
   route:{q:"String the four scenes together in the order they happened.",order:["A","B","C","D"],half:true,
    notes:["13 March 1998.","5 June 1998, twelve weeks later.","25 September 1998, sixteen weeks later.","22 January 1999, seventeen weeks later."]},
   anchor:{q:"Now drop a pin where you think he was based. Tap anywhere on the map.",x:200,y:250,r:42,name:"Herald House, Marlow"}}
 }
};
CASES.forEach(c=>Object.assign(c,ADD[c.id]));

const PHOTOS=new Set(["v_lock", "hallam", "maureen", "tony", "sheila", "colin", "v_bowls", "kemp", "paula", "farrow", "jean", "doyle", "v_platform", "bannister", "mark", "leah", "pete", "susan", "v_series0", "v_series1", "v_series2", "v_series3","dunmore","garrity","spence","lamb","pike"]);

EXTRA.lock.scene={photo:{src:"scene_lock.jpg",cap:"Lower basin, 15 Oct 94. Torch as found."}};
EXTRA.bowls.evening={photo:{src:"scene_bowls.jpg",cap:"Village hall, 20 Nov 87, after the ambulance left."}};
EXTRA.platform.scene={photo:{src:"scene_platform.jpg",cap:"Lock-ups behind Tanner's Row, 9 Mar 03."}};
EXTRA.series.incD={photo:{src:"scene_series.jpg",cap:"Churchyard path, Hollin Bridge, 23 Jan 99."}};
