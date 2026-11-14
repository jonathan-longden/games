
/* ============================== CASE FILES ============================== */
const CASES = [
{
  id:"lock", ref:"HM/94/0417", title:"The Lock-Keeper's Last Night", level:"Rookie", mins:"35 to 45 min",
  place:"Harrowby Lock, East Midlands", date:"Saturday 15 October 1994",
  blurb:"A lock-keeper is found in his own canal basin. In 1994 it was written off as a slip in the dark. Thirty years later his missing logbook turns up in a house clearance.",
  brief:[
    "At 06:40 on Saturday 15 October 1994, an angler found the body of <b>Alan Pryce, 52</b>, in the lower basin of Harrowby Lock. Alan had kept the lock for nineteen years and lived alone in the lock cottage.",
    "The 1994 inquest recorded an open verdict. The working theory was that he slipped on wet stone during a late check of the gates. Nobody was charged.",
    "In March 2024 a house clearance firm handed in a canal logbook with Alan's name inside the cover. The file has been reopened and passed to your team.",
    "<b>Your job:</b> decide whether this was an accident. If it was not, name the person responsible, say why they did it, and explain what happened at the lock that night."
  ],
  budget:9,
  suspects:[
    {id:"hallam", name:"Derek Hallam", facts:"47 · Owner, Hallam's Boatyard · 5 ft 10", note:"Runs the boatyard half a mile upstream. Darts captain at The Anchor. Lost a boat to vandals three weeks before the death."},
    {id:"maureen", name:"Maureen Pryce", facts:"49 · Staff nurse · Alan's estranged wife", note:"Separated from Alan in 1992. Never divorced. Once held a life policy on him."},
    {id:"tony", name:"Tony Voss", facts:"23 · Unemployed · Alan's nephew and lodger", note:"Lodged at the lock cottage. Known to owe money to a bookmaker. Argued with Alan two days before."},
    {id:"sheila", name:"Sheila Grant", facts:"58 · Landlady, The Anchor", note:"Her pub backs on to the towpath. Alan had reported her to the licensing bench for serving after hours."},
    {id:"colin", name:"Colin Marsh", facts:"61 · Retired fitter · Found the body", note:"Fishes the basin most Saturdays. Cautioned in 1989 for taking fish without a permit."}
  ],
  leads:[
    {id:"scene", cat:"Scene", title:"1994 scene report", sub:"What the first officers found at the lock.", type:"Scene report", ref:"HM/94/0417/1", date:"15 Oct 1994",
     body:`<p>Deceased recovered from lower basin 07:15. Dressed in work trousers, jumper, donkey jacket, boots laced.</p>
<p>Lock-side railings intact. Stone wet from overnight drizzle. A torch was found on the towpath by the lower gate, switched on, battery nearly flat.</p>
<p>Lock cottage door unlocked. Fire banked. A mug of tea on the office desk, full and cold.</p>
<p>Officer's note: the lock logbook, which the deceased is said to have filled in daily, was not on the office desk or anywhere in the cottage. Not pursued.</p>
<p>Left sleeve of the donkey jacket carries a smear of dark blue paint, 4 inches long, at elbow height. Jacket retained.</p>`},
    {id:"watch", cat:"Scene", title:"The wristwatch", sub:"Property bag item 3.", type:"Property record", ref:"HM/94/0417/3", date:"15 Oct 1994",
     body:`<p>Gents' wind-up wristwatch, steel case, leather strap. Glass cracked across the face. Not waterproof.</p>
<p>Hands stopped at <b>11:52</b>. Date window shows 14.</p>
<p>Watchmaker's opinion (2024): the movement flooded and stopped within a minute or two of going under. The crack is from a hard knock, not from water.</p>`},
    {id:"path", cat:"Forensics", title:"Pathologist's summary", sub:"Cause of death and one unexplained bruise.", type:"Pathology summary", ref:"HM/94/0417/5", date:"17 Oct 1994",
     body:`<p>Cause of death: drowning.</p>
<p>One bruise to the back of the head. Consistent with a fall backwards on to stone. Equally consistent with a single blow from behind or a hard shove against something solid before he entered the water.</p>
<p>No alcohol. No drugs. No signs of illness that would cause a collapse.</p>
<p>Comment: a man who slips forward into a lock does not usually bruise the back of his head.</p>`},
    {id:"paint", cat:"Forensics", title:"Paint analysis", sub:"The blue smear on the jacket, tested in 2024.", requires:"scene", type:"Laboratory report", ref:"HM/24/0092/L2", date:"11 Apr 2024",
     body:`<p>Sample: blue smear from left sleeve of donkey jacket.</p>
<p>Result: marine enamel, shade sold as Oxford Blue. The paint was still soft when it transferred, so the surface had been painted within the previous few weeks.</p>
<p>Supplier ledgers show one local buyer of that shade in 1994: Hallam's Boatyard. The yard used it on three boats that year: <b>Heron</b>, <b>Marlow Maid</b> and <b>Kestrel</b>.</p>
<p>Yard records place Heron and Marlow Maid on the slipway out of the water from 3 to 21 October 1994.</p>`},
    {id:"sheila_st", cat:"Witnesses", title:"Sheila Grant's statement", sub:"The landlady on who was in the pub, and what she heard.", type:"Witness statement", ref:"HM/94/0417/8", date:"16 Oct 1994",
     body:`<p>"Friday was darts night. I called time at eleven and had the doors bolted by ten past.</p>
<p>Derek Hallam captains the team. He threw his last leg about ten and left straight after, about half past ten. I remember because he never leaves before the raffle.</p>
<p>I was putting the bottles out the back at about quarter to midnight and I heard an engine on the water. A slow diesel, going down towards the lock. No lights that I could see. Nobody moves a boat at that hour.</p>
<p>Yes, Alan reported me to the magistrates in the summer. I was fined forty pounds. I was cross about it. I would not hurt a man over forty pounds."</p>`},
    {id:"colin_st", cat:"Witnesses", title:"Colin Marsh's statement", sub:"The angler who found Alan.", type:"Witness statement", ref:"HM/94/0417/2", date:"15 Oct 1994",
     body:`<p>"I got to the basin about half six. I saw his jacket first, then him. I ran to the phone box.</p>
<p>One thing was wrong. The paddles on the bottom gates were up and the chamber was empty. Alan always left the lock full at night, he said it kept the gates tight. Somebody had worked a boat down through that lock after he did his evening round.</p>
<p>I did have a caution about the fishing permit. I have paid for one ever since."</p>`},
    {id:"log", cat:"Records", title:"The recovered logbook", sub:"Handed in by a house clearance firm in 2024.", type:"Exhibit", ref:"HM/24/0092/E1", date:"Recovered 4 Mar 2024",
     body:`<p>Hardback ledger, "A. PRYCE, HARROWBY LOCK" inside the cover. Found in a tea chest in the loft of a house in Stoke Reach. The house belonged to the late Derek Hallam's brother.</p>
<p>Final page of entries, 14 October 1994:</p>
<div class="tw"><table class="rec"><tr><th>Time</th><th>Boat</th><th>Dir.</th><th>Remarks</th></tr>
<tr><td>09:15</td><td>Willow</td><td>Up</td><td>Hire boat. Showed them the paddles.</td></tr>
<tr><td>14:30</td><td>Tamar</td><td>Down</td><td>Coal.</td></tr>
<tr><td>17:55</td><td>Willow</td><td>Down</td><td>Same family. Left lock full, 18:20.</td></tr>
<tr><td>23:40</td><td>KESTREL</td><td>Down</td><td class="hand">No lights. She is supposed to be on the bottom at Stoke Reach?? Going out to see who has her.</td></tr></table></div>
<p>No entries after this.</p>`},
    {id:"claim", cat:"Records", title:"Insurance claim: Kestrel", sub:"Why Alan thought that boat was 'on the bottom'.", requires:"log", type:"Insurer's file", ref:"MMI/C/94/2281", date:"Sept to Nov 1994",
     body:`<p>Claimant: D. Hallam, Hallam's Boatyard. Vessel: 42 ft cruiser <b>Kestrel</b>, repainted and refitted August 1994.</p>
<p>Claim: vessel holed by vandals and sunk at her mooring at Stoke Reach on the night of 29 September 1994. Total loss.</p>
<p>Loss adjuster's note: "Water at the mooring is deep and black. I was shown an oil slick and a mooring line. Wreck not sighted. Salvage judged uneconomic."</p>
<p>Paid in full, <b>£38,000</b>, 22 November 1994.</p>
<p>Margin note, 10 October: "Call from a Mr Pryce at Harrowby Lock asking who handles the Kestrel claim. Said he would ring back on Monday with something we ought to know."</p>`},
    {id:"fuel", cat:"Records", title:"Marlow Road garage receipts", sub:"Collected in the 1994 door-to-door and never read.", type:"Till records", ref:"HM/94/0417/14", date:"14 to 15 Oct 1994",
     body:`<p>All-night garage, Marlow Road, 1.5 miles below Harrowby Lock by road. The canal passes behind it.</p>
<div class="tw"><table class="rec"><tr><th>Time</th><th>Sale</th><th>Paid</th></tr>
<tr><td>22:48</td><td>Petrol £15.00</td><td>Cash</td></tr>
<tr><td>23:31</td><td>Cigarettes</td><td>Cash</td></tr>
<tr><td>00:12</td><td>Diesel, two cans, £10.00</td><td>Cheque, D. HALLAM</td></tr>
<tr><td>01:05</td><td>Petrol £8.00</td><td>Cash</td></tr></table></div>
<p>Cashier (1994): "The man with the cans was on foot and his trousers were wet to the knee."</p>`},
    {id:"casino", cat:"Records", title:"Regal Casino door book", sub:"Where Tony Voss says he was.", type:"Membership log", ref:"HM/94/0417/11", date:"14 Oct 1994",
     body:`<p>Regal Casino, Nottingham. 26 miles from Harrowby by road.</p>
<p>Member 4471, A. VOSS. Signed in 22:05. Signed out 01:40.</p>
<p>Pit supervisor: "He was at the roulette table all night. He lost what he came with and borrowed a tenner for the bus."</p>`},
    {id:"rota", cat:"Records", title:"Hospital night rota", sub:"Maureen Pryce's shift.", type:"Staff record", ref:"HM/94/0417/12", date:"14 Oct 1994",
     body:`<p>County General, Ward 6. Night shift 21:00 to 07:00.</p>
<p>Staff Nurse M. Pryce signed the drug round at 22:00, 00:00 and 02:00. Ward sister confirms she did not leave the ward.</p>`},
    {id:"hallam_int", cat:"Interviews", title:"Derek Hallam, 1994 interview", sub:"His account of that night.", type:"Interview record", ref:"HM/94/0417/9", date:"18 Oct 1994",
     body:`<p class="q">Q. Where were you on Friday night?</p>
<p>"Darts at The Anchor. I was there till Sheila called time at eleven. Then I walked home and went to bed. My wife will tell you. I was in by half past."</p>
<p class="q">Q. How well did you know Alan Pryce?</p>
<p>"To nod to. He kept himself to himself. Wrote everything down in that book of his, every boat, every minute. Daft old stick."</p>
<p class="q">Q. You lost a boat recently.</p>
<p>"Kestrel. Kids put a hole in her. She is in twenty foot of water at Stoke Reach and that is where she will stay. A sad loss."</p>`},
    {id:"wife", cat:"Interviews", title:"The former Mrs Hallam, 2024", sub:"Derek Hallam's alibi witness, thirty years on.", requires:"hallam_int", type:"Interview record", ref:"HM/24/0092/7", date:"2 May 2024",
     body:`<p>Derek Hallam died in 2016. His former wife, divorced from him in 2001, agreed to speak.</p>
<p>"I took a tablet and I was asleep by ten that night. I never heard him come in. In the morning he said, if anyone asks, I was home by half eleven. So that is what I said.</p>
<p>His trousers and boots were in the outhouse, soaked. He said he had slipped off the yard jetty.</p>
<p>That winter we had money. A new van, the roof done. He told me it was the insurance and to stop asking."</p>`},
    {id:"tony_int", cat:"Interviews", title:"Tony Voss, 1994 interview", sub:"The nephew with the debts.", type:"Interview record", ref:"HM/94/0417/10", date:"16 Oct 1994",
     body:`<p>"I owe Bailey's the bookmakers about two thousand three hundred. I asked Uncle Alan for a loan on Wednesday and he said no and we had words. That is all it was.</p>
<p>Friday I was in town. I got the last bus back and the police were already at the cottage.</p>
<p>He was wound up about something all that week. He said somebody round here was telling lies about a boat and he was going to write it all down and ring somebody on Monday."</p>`},
    {id:"maureen_int", cat:"Interviews", title:"Maureen Pryce, 1994 interview", sub:"The estranged wife and the life policy.", type:"Interview record", ref:"HM/94/0417/13", date:"17 Oct 1994",
     body:`<p>"We separated two years ago. No hard feelings, we just ran out of things to say.</p>
<p>There was a policy on him, fifteen thousand. I stopped paying into it last year and it lapsed. I get nothing. You can ring them.</p>
<p>Alan would never go out to that lock at night without his logbook being up to date. If it is missing, somebody took it."</p>`}
  ],
  killer:"hallam",
  motives:[
    {id:"a", t:"To stop Alan exposing an insurance fraud", ok:true},
    {id:"b", t:"To inherit money and clear a gambling debt"},
    {id:"c", t:"To collect on a life insurance policy"},
    {id:"d", t:"Revenge for being reported to the licensing magistrates"}
  ],
  howQ:"What was the Kestrel doing at Harrowby Lock at 23:40?",
  hows:[
    {id:"a", t:"Being moved downstream in secret after being falsely reported sunk", ok:true},
    {id:"b", t:"Being stolen from the boatyard by someone who needed cash"},
    {id:"c", t:"Delivering after-hours stock to The Anchor"},
    {id:"d", t:"Alan was moving it himself to check the lock gates"}
  ],
  solution:[
    "<b>Derek Hallam killed Alan Pryce.</b> The Kestrel never sank. Hallam hid her, claimed £38,000, and waited for a dark night to move her down the canal to be sold or broken up.",
    "The only way downstream was through Harrowby Lock. Alan heard the engine, wrote the 23:40 entry and walked out with his torch. He had already phoned the insurer and promised to ring back on Monday.",
    "There was a struggle beside the freshly painted hull. That is the blue paint on Alan's sleeve and the bruise on the back of his head. His watch stopped at 23:52. Hallam took the logbook, finished working the boat through, and left the lock empty.",
    "His alibi fails three ways. The landlady saw him leave at half past ten, not eleven. He paid for diesel by cheque at 00:12 with wet trousers, a mile and a half below the lock. His former wife now says she was asleep and was told what to say.",
    "Tony was at the casino until 01:40. Maureen never left Ward 6 and her policy had lapsed. Sheila and Colin are the two witnesses who noticed a boat had gone through."
  ]
},
{
  id:"bowls", ref:"EL/87/1120", title:"The Bowls Club Dinner", level:"Detective", mins:"40 to 50 min",
  place:"Little Stanton village hall", date:"Friday 20 November 1987",
  blurb:"A retired teacher collapses at her club's annual dinner. Her own doctor signs it off as her heart. A blood sample kept in a hospital freezer says otherwise.",
  brief:[
    "At 21:15 on Friday 20 November 1987, <b>Margaret Ellwood, 67</b>, treasurer of Little Stanton Bowls Club, collapsed at the club's annual dinner. She died in the ambulance. Sixty-one people were in the hall.",
    "Her GP, who was sitting beside her, certified a heart attack. There was no post-mortem and no inquest.",
    "In 2019 the county hospital audited its old freezer stock. A blood sample taken from Margaret in the ambulance was tested as part of a training exercise. It contained <b>digoxin</b>, a heart drug, at many times the safe level. She had never been prescribed it.",
    "<b>Your job:</b> work out who gave Margaret the drug, why they wanted her dead, and how it reached her in a hall full of people."
  ],
  budget:9,
  suspects:[
    {id:"kemp", name:"Ronald Kemp", facts:"61 · Club chairman · Retired bank clerk", note:"Ran the evening as master of ceremonies. He and Margaret were the only two signatories on the club account."},
    {id:"paula", name:"Paula Ellwood", facts:"34 · Margaret's niece and only relative", note:"Lives in Leeds. Behind on her mortgage. Always understood she would inherit everything."},
    {id:"farrow", name:"Dr Ian Farrow", facts:"52 · Village GP · Club member", note:"Margaret's doctor for eleven years. Runs the surgery and its dispensary alone. Signed the death certificate."},
    {id:"jean", name:"Jean Aldous", facts:"45 · Caterer for the dinner", note:"Lost her job in the school kitchen in 1979 after a complaint from Margaret, then deputy head."},
    {id:"doyle", name:"Frank Doyle", facts:"70 · Margaret's next-door neighbour", note:"Two years of solicitors' letters over a boundary hedge. A keen gardener known for his foxgloves."}
  ],
  leads:[
    {id:"evening", cat:"Scene", title:"Timeline of the dinner", sub:"Pieced together from the 2019 interviews.", type:"Case summary", ref:"EL/19/0340/1", date:"2019",
     body:`<div class="tw"><table class="rec"><tr><th>Time</th><th>Event</th></tr>
<tr><td>19:00</td><td>Doors open. Sherry reception. Guests help themselves from a side table.</td></tr>
<tr><td>19:30</td><td>Everyone seated. Grace.</td></tr>
<tr><td>19:40</td><td>Soup, ladled from one tureen.</td></tr>
<tr><td>20:10</td><td>Main course, plated in the kitchen, carried out by two schoolgirls.</td></tr>
<tr><td>20:45</td><td>Raffle, called by the chairman.</td></tr>
<tr><td>21:05</td><td>Margaret says she feels sick and that the lights have "yellow rings round them".</td></tr>
<tr><td>21:15</td><td>Margaret collapses. Dr Farrow attends. Ambulance called 21:18.</td></tr></table></div>
<p>Nobody else in the hall was taken ill.</p>`},
    {id:"seating", cat:"Scene", title:"Seating plan", sub:"Found pinned inside the club minute book.", type:"Exhibit", ref:"EL/19/0340/E2", date:"20 Nov 1987",
     body:`<p>Typed plan, six tables. Relevant entries:</p>
<div class="tw"><table class="rec"><tr><th>Table</th><th>Seats</th></tr>
<tr><td>Top</td><td>R. Kemp (centre), vicar, club president</td></tr>
<tr><td>2</td><td>Mrs E. Hobbs · <b>Miss M. Ellwood</b> · Dr I. Farrow · Miss P. Ellwood</td></tr>
<tr><td>4</td><td>F. Doyle (aisle seat, "crutches")</td></tr></table></div>
<p>On the typed plan Dr Farrow is at Table 5. His name is crossed out there and written in by hand beside Margaret. Pencil note in the chairman's writing: "Dr F. asked to swap. Wants to keep an eye on Margaret."</p>`},
    {id:"tox", cat:"Forensics", title:"Toxicology report", sub:"What the 2019 test found, and what it rules out.", type:"Laboratory report", ref:"EL/19/0340/L1", date:"8 Jul 2019",
     body:`<p>Sample: blood, labelled ELLWOOD M., taken 21:31 on 20 Nov 1987.</p>
<p>Result: digoxin at roughly eight times the upper treatment level.</p>
<p>The sample contains digoxin only. Poisoning from the foxglove plant leaves a mixture of related compounds. None are present. <b>This was tablets, not a plant.</b></p>
<p>Timing: the symptoms described, sickness and yellow halos around lights, usually begin one to two hours after a large dose is swallowed. That puts the dose at about <b>19:00 to 20:00</b>.</p>
<p>Digoxin tablets are small, crush to a fine powder and taste bitter.</p>`},
    {id:"cert", cat:"Records", title:"Death certificate and surgery notes", sub:"What the doctor wrote, against what her file says.", type:"Medical records", ref:"EL/19/0340/4", date:"1987",
     body:`<p>Death certificate, signed I. Farrow MB ChB, 20 Nov 1987: "Myocardial infarction. Known cardiac history. Seen by me within 14 days." No referral to the coroner.</p>
<p>Margaret's surgery file, same practice:</p>
<p>"14 Sept 1987. Annual check. BP normal. Heart sounds normal. Fit and well. No medication."</p>
<p>There is no entry anywhere in eleven years of notes recording a heart complaint.</p>
<p>Last entry: "26 Oct 1987. Home visit." No reason given.</p>`},
    {id:"hobbs", cat:"Witnesses", title:"Edith Hobbs's statement", sub:"She sat on Margaret's other side.", type:"Witness statement", ref:"EL/19/0340/6", date:"19 Jul 2019",
     body:`<p>Mrs Hobbs is 91. Her memory of the evening is clear.</p>
<p>"We all fetched our own sherry. Except Margaret. Dr Farrow brought hers over to her, ever so attentive. She pulled a face and said it was bitter, and that the club had bought the cheap stuff this year. She drank it anyway. She was brought up not to waste.</p>
<p>Over the soup she whispered to me that she had made a silly mistake with her will and she was putting it right on Monday. She said she had been talked into something."</p>`},
    {id:"jean_st", cat:"Witnesses", title:"Jean Aldous's statement", sub:"The caterer on how the food went out.", type:"Witness statement", ref:"EL/19/0340/7", date:"22 Jul 2019",
     body:`<p>"One tureen of soup for the whole hall. I ladled, the girls carried. The mains were all the same, plated in a row, and the girls took whichever was nearest. I could not have picked out one plate for one person if I tried.</p>
<p>The sherry was the club's own. Set out on the side table before I arrived. I never touched it.</p>
<p>Yes, Miss Ellwood got me sacked from the school. That was eight years before. I built a business out of it. I would hardly wait eight years and then do it in front of sixty people."</p>`},
    {id:"accounts", cat:"Records", title:"Club accounts, 1986 to 1987", sub:"Margaret's own ledger.", type:"Exhibit", ref:"EL/19/0340/E3", date:"1987",
     body:`<p>Treasurer's ledger in Margaret's handwriting.</p>
<p>Between March 1986 and August 1987, eleven cheques made out to cash, signed R. Kemp alone, total <b>£1,840</b>. No receipts.</p>
<p>16 Nov 1987: single cash deposit, £1,840.</p>
<p class="hand">Margaret's note beside it: "R.K. has made good in full. He was very ashamed. Matter closed. The committee need not be told."</p>`},
    {id:"kemp_int", cat:"Interviews", title:"Ronald Kemp, 2019 interview", sub:"The chairman and the missing money.", type:"Interview record", ref:"EL/19/0340/9", date:"25 Jul 2019",
     body:`<p>Mr Kemp is 93 and lives in a care home.</p>
<p>"I borrowed from the club when my wife was ill. Margaret found it in the summer. She gave me until the dinner to pay it back and I did, the Monday before. She was decent about it. More than I deserved.</p>
<p>That night I was on the door from seven shaking hands, then at the top table, then calling the raffle. I never went near Table 2.</p>
<p>Dr Farrow asked me that afternoon to move him next to her. He said her heart wanted watching. I had never heard she had anything wrong with her heart."</p>`},
    {id:"will", cat:"Records", title:"Margaret's wills", sub:"Two wills, three weeks apart from the death.", type:"Probate file", ref:"EL/19/0340/10", date:"1984 and 1987",
     body:`<p><b>Will of 3 May 1984.</b> Everything to "my niece Paula Ellwood".</p>
<p><b>Will of 28 October 1987.</b> The house to Paula. The whole of her savings, <b>£40,000</b>, to "the Little Stanton Surgery Patient Fund, sole trustee Dr I. Farrow, to use as he sees fit".</p>
<p>The 1987 will was signed at Margaret's home. Witnesses: the surgery receptionist, and Mrs D. Farrow.</p>
<p>Probate was granted on the 1987 will in February 1988.</p>`},
    {id:"diary", cat:"Records", title:"Solicitor's appointment diary", sub:"Who Margaret planned to see on the Monday.", requires:"will", type:"Exhibit", ref:"EL/19/0340/E5", date:"Nov 1987",
     body:`<p>Desk diary, Pargeter & Lowe, solicitors.</p>
<p>Entry for <b>Monday 23 November 1987, 10:00</b>:</p>
<p class="hand">"Miss Ellwood. To revoke will of 28 Oct and go back to the 1984 one. Says she was talked into it during a home visit and has not slept since. Rang Thurs 19th. Says she has told the person concerned."</p>
<p>The appointment is struck through with "deceased".</p>`},
    {id:"register", cat:"Records", title:"Surgery dispensary register", sub:"Tablet stock at the village practice.", requires:"tox", type:"Stock register", ref:"EL/19/0340/12", date:"Nov 1987",
     body:`<p>Little Stanton was a dispensing practice: the surgery kept and handed out its own medicines. The register was kept by Dr Farrow personally. The receptionist had no key.</p>
<div class="tw"><table class="rec"><tr><th>Date</th><th>Digoxin 250 microgram tablets</th></tr>
<tr><td>2 Nov</td><td>Stock count: 500</td></tr>
<tr><td>2 to 27 Nov</td><td>Dispensed to named patients: 140</td></tr>
<tr><td>30 Nov</td><td>Health authority spot check: <b>320 in stock</b></td></tr></table></div>
<p>Inspector's note: "40 tablets unaccounted for. Dr Farrow says a bottle was dropped and swept up. Accepted."</p>`},
    {id:"doyle_int", cat:"Interviews", title:"Frank Doyle, 1987 note", sub:"The neighbour with the foxgloves.", type:"File note", ref:"EL/19/0340/13", date:"1987 and 2019",
     body:`<p>Mr Doyle died in 1996. A parish constable spoke to him in December 1987 after village gossip about the hedge.</p>
<p>"We fell out over six foot of privet, yes. I would have won, too. I had my hip done in October. I was on two crutches at that dinner and I did not leave my chair from sitting down to the ambulance coming. Ask anyone on Table 4."</p>
<p>2019: three surviving guests from Table 4 confirm he did not get up.</p>`},
    {id:"paula_int", cat:"Interviews", title:"Paula Ellwood, 2019 interview", sub:"The niece who expected to inherit.", type:"Interview record", ref:"EL/19/0340/14", date:"30 Jul 2019",
     body:`<p>"I came down from Leeds on the 17:10. It was late. I still have the diary: 'train got in 19:42, taxi, missed the sherry'. I walked into the hall as they were clearing the soup bowls, about eight o'clock.</p>
<p>I was in arrears on the flat, I will not pretend otherwise. I thought I was getting everything. I found out about the new will at the solicitor's in December. Forty thousand pounds to a fund nobody in the village had heard of.</p>
<p>Aunt Margaret did not have a bad heart. She walked four miles a day."</p>`},
    {id:"fund", cat:"Records", title:"Patient Fund bank statements", sub:"Where bequests to the surgery went.", requires:"will", type:"Bank records", ref:"EL/19/0340/15", date:"1985 to 1989",
     body:`<p>Account: "Little Stanton Surgery Patient Fund". Opened March 1985. Sole signatory: Dr I. Farrow.</p>
<div class="tw"><table class="rec"><tr><th>Date</th><th>In</th><th>Out</th></tr>
<tr><td>Jun 1985</td><td>Estate of Mr A. Tilley, £12,000</td><td></td></tr>
<tr><td>1985 to 86</td><td></td><td>School fees, £9,400</td></tr>
<tr><td>Sep 1986</td><td>Estate of Mrs V. Crane, £18,500</td><td></td></tr>
<tr><td>1986 to 87</td><td></td><td>Car finance, £11,200 · Building work at Dr Farrow's home address, £8,900</td></tr>
<tr><td>Oct 1987</td><td colspan="2">Balance: £212. Overdue notice on the car finance.</td></tr>
<tr><td>Mar 1988</td><td>Estate of Miss M. Ellwood, £40,000</td><td></td></tr></table></div>
<p>No payment from the account was ever made to the surgery or to a patient.</p>`},
    {id:"farrow_int", cat:"Interviews", title:"Dr Ian Farrow, 2019 interview", sub:"Now 84 and retired.", type:"Interview record", ref:"EL/19/0340/16", date:"6 Aug 2019",
     body:`<p class="q">Q. Why did you certify a heart attack?</p>
<p>"She had a weak heart. I had been treating it for years."</p>
<p class="q">Q. Her notes say otherwise.</p>
<p>"Not everything goes in the notes."</p>
<p class="q">Q. How did you come to be sitting beside her?</p>
<p>"Chance. Ronald did the seating."</p>
<p class="q">Q. Did you bring her a drink?</p>
<p>"I never fetch drinks. People fetched their own."</p>
<p class="q">Q. When did you last speak to her before the dinner?</p>
<p>"She telephoned the surgery on the Thursday. Something about paperwork. I told her we would talk at the dinner."</p>`}
  ],
  killer:"farrow",
  motives:[
    {id:"a", t:"To stop her revoking a will that paid £40,000 into a fund he controlled", ok:true},
    {id:"b", t:"To hide money taken from the club account"},
    {id:"c", t:"To inherit before the will could be changed again"},
    {id:"d", t:"A long-held grudge over a lost job or a boundary dispute"}
  ],
  howQ:"How did the digoxin reach Margaret?",
  hows:[
    {id:"a", t:"Crushed tablets in the sherry that was carried to her at the reception", ok:true},
    {id:"b", t:"Foxglove leaves stirred into her soup"},
    {id:"c", t:"Added to her plate of the main course in the kitchen"},
    {id:"d", t:"The wrong tablets, prescribed to her weeks earlier"}
  ],
  solution:[
    "<b>Dr Ian Farrow killed Margaret Ellwood.</b> His 'Patient Fund' was his private purse. Two earlier bequests had paid for school fees, a car and building work, and by October 1987 the account held £212.",
    "On a home visit he talked Margaret into a new will leaving the fund £40,000. She regretted it, booked the solicitor for Monday, and on the Thursday told him so. He had one weekend.",
    "He took 40 digoxin tablets from the dispensary only he controlled, asked the chairman to move him beside her, and carried her a sherry at the reception. She said it was bitter. The lab puts the dose between 19:00 and 20:00, and the reception was the only time anyone handed her something meant for her alone.",
    "Then he did the one thing only he could do: he signed the death certificate, invented a 'known cardiac history' her own notes disprove, and kept the coroner away.",
    "The drug was tablets, so Doyle's foxgloves are irrelevant, and he never left his chair. Kemp had repaid the club and never went near Table 2. Paula arrived at eight, after the dose, and did not know the will had changed. Jean could not have aimed a ladle of shared soup at one guest."
  ]
},
{
  id:"platform", ref:"WB/03/0932", title:"Platform Two", level:"Inspector", mins:"45 to 60 min",
  place:"Wexborough", date:"Thursday 6 March 2003",
  blurb:"A young reporter is filmed boarding the 21:58 train. Three days later he is found in a locked garage behind his own flat. Everyone has an alibi for ten o'clock. That may be the wrong time to ask about.",
  brief:[
    "<b>Daniel Reeve, 24</b>, was a trainee reporter on the Wexborough Gazette. On Thursday 6 March 2003, station CCTV recorded a figure in his red jacket buying a ticket and boarding the 21:58 to Linfield.",
    "On Sunday 9 March he was found dead in a lock-up garage on Tanner's Row, directly behind the flat he shared. He had a single head injury.",
    "The 2003 team believed Daniel came back from Linfield that night and was attacked near home, probably over a story he was writing about a council land sale. A councillor was questioned twice. Nobody was charged.",
    "<b>Your job:</b> test that theory. Name the person responsible, give the motive, and say who is on that CCTV."
  ],
  budget:9,
  suspects:[
    {id:"bannister", name:"Cllr Hugh Bannister", facts:"58 · Chair of planning · 6 ft 1", note:"Subject of Daniel's unpublished story about the sale of council land to a developer. The 2003 prime suspect."},
    {id:"mark", name:"Mark Ostler", facts:"25 · IT temp · Daniel's flatmate · 5 ft 8", note:"Shared the Tanner's Row flat with Daniel for two years. Reported him missing on the Friday evening."},
    {id:"leah", name:"Leah Simmons", facts:"23 · Bar staff · Daniel's girlfriend · 5 ft 4", note:"Together eight months. They argued on the Tuesday. Due to meet him that Thursday night."},
    {id:"pete", name:"Pete Lawson", facts:"44 · Station night staff · 5 ft 11", note:"On duty at Wexborough station that night. Daniel's red jacket was later found in his locker. Convicted in 1998 of theft from lost property."},
    {id:"susan", name:"Susan Clegg", facts:"51 · Editor, Wexborough Gazette · 5 ft 5", note:"Pulled Daniel's council story the day before he vanished. They rowed about it in front of the newsroom."}
  ],
  leads:[
    {id:"scene", cat:"Scene", title:"Scene report: the lock-up", sub:"Garage 7, Tanner's Row.", type:"Scene report", ref:"WB/03/0932/1", date:"9 Mar 2003",
     body:`<p>Garage 7 belongs to the landlord of 14 Tanner's Row and goes with the upstairs flat. One padlock. The key hangs on a hook in the flat's hallway. Padlock found <b>locked and undamaged</b>.</p>
<p>Deceased lying behind stacked boxes, covered with a dust sheet. Shirt, tie, work trousers, shoes. No jacket. No phone. No wallet.</p>
<p>It rained hard in Wexborough from 20:00 on Thursday until the early hours. The deceased's shoes and trouser hems are <b>clean and dry-marked, with no mud or water staining</b>.</p>
<p>No blood in the garage. He was not injured here.</p>`},
    {id:"path", cat:"Forensics", title:"Pathologist's summary", sub:"Injury, and a clue about timing.", type:"Pathology summary", ref:"WB/03/0932/4", date:"11 Mar 2003",
     body:`<p>Cause of death: a single head injury, consistent with falling or being pushed hard against a solid edge such as a worktop or hearth.</p>
<p>After three days, temperature cannot give a time of death.</p>
<p>However: the stomach held a meal of noodles and vegetables that had barely begun to digest. In my opinion he died <b>within about two hours of eating it</b>.</p>
<p>2003 investigators' note: "Assume he ate in Linfield late evening."</p>`},
    {id:"receipt", cat:"Records", title:"Golden Bowl takeaway", sub:"A card payment on the Thursday.", type:"Till record", ref:"WB/03/0932/15", date:"6 Mar 2003",
     body:`<p>Golden Bowl, corner of Tanner's Row.</p>
<p><b>17:22</b>, one chicken chow mein, paid on debit card ending 4410, D. REEVE.</p>
<p>Counter staff (2003): "He is a regular. He said he was eating at home because he had something to sort out with someone."</p>
<p>No food outlet in Linfield recalls him. No card payment was made there.</p>`},
    {id:"cctv", cat:"Scene", title:"Station CCTV", sub:"The last sighting.", type:"CCTV log", ref:"WB/03/0932/6", date:"6 Mar 2003",
     body:`<div class="tw"><table class="rec"><tr><th>Time</th><th>Camera</th><th>What it shows</th></tr>
<tr><td>21:47</td><td>Ticket hall</td><td>Figure in red jacket, hood up over a dark beanie, buys a single to Linfield from the machine. Pays cash.</td></tr>
<tr><td>21:50</td><td>Platform 2</td><td>Same figure stands beside the timetable poster, back to camera.</td></tr>
<tr><td>21:58</td><td>Platform 2</td><td>Boards the Linfield train.</td></tr></table></div>
<p>The face is never visible. Identified in 2003 as Daniel Reeve "by his distinctive jacket", by his flatmate.</p>
<p>Daniel held a monthly rail pass for the Linfield line. It was found in his desk at the Gazette.</p>`},
    {id:"height", cat:"Forensics", title:"CCTV height analysis", sub:"A 2023 measurement of the figure on Platform 2.", requires:"cctv", type:"Imaging report", ref:"WB/23/0118/L3", date:"14 Sep 2023",
     body:`<p>The timetable poster frame on Platform 2 is still in place and was measured.</p>
<p>Using the frame as a scale, the figure at 21:50 stands <b>173 cm, give or take 3 cm</b>. That is about 5 ft 8.</p>
<p>Daniel Reeve's height, from his passport application and the post-mortem: <b>188 cm</b>, 6 ft 2.</p>
<p>Conclusion: the person on Platform 2 is not Daniel Reeve.</p>`},
    {id:"phone", cat:"Records", title:"Daniel's phone records", sub:"Calls, one text, and where the handset went.", type:"Phone records", ref:"WB/03/0932/8", date:"6 Mar 2003",
     body:`<div class="tw"><table class="rec"><tr><th>Time</th><th>Event</th><th>Mast</th></tr>
<tr><td>12:15</td><td>Call to his bank, 9 min</td><td>Town centre</td></tr>
<tr><td>16:40</td><td>Call to Gazette newsdesk</td><td>Town centre</td></tr>
<tr><td>18:05</td><td>Text to Leah: "cant make tonite, chasing a story. x"</td><td>Tanner's Row</td></tr>
<tr><td>18:05 to 21:20</td><td>No use. Handset stationary.</td><td>Tanner's Row</td></tr>
<tr><td>21:45</td><td>Handset registers</td><td>Wexborough station</td></tr>
<tr><td>22:10</td><td>Handset registers</td><td>Linfield</td></tr>
<tr><td>23:05</td><td>Handset registers</td><td>Tanner's Row</td></tr>
<tr><td>23:10</td><td>Switched off. Never recovered.</td><td></td></tr></table></div>`},
    {id:"bank", cat:"Records", title:"Daniel's bank file", sub:"Why he rang the bank that lunchtime.", type:"Bank records", ref:"WB/03/0932/9", date:"Jan to Mar 2003",
     body:`<p>Between January and February 2003, a credit card and two personal loans were opened in Daniel Reeve's name. Total borrowed: <b>£6,500</b>. All statements were sent to a PO box, not to Tanner's Row.</p>
<p>Fraud line call note, 6 March, 12:15: "Customer says he has never applied for any of these. Found a final demand in the recycling at home. Says: 'I think I know who it is. I am going to have it out with him tonight and then I will ring you back.'"</p>
<p>He did not ring back.</p>
<p>Final transaction: cash machine on Tanner's Row, <b>23:20 on 6 March</b>, £200, correct PIN.</p>`},
    {id:"pobox", cat:"Records", title:"PO box application", sub:"Who collected the post.", requires:"bank", type:"Exhibit", ref:"WB/23/0118/E4", date:"8 Jan 2003",
     body:`<p>PO Box 114, Wexborough sorting office. Opened 8 January 2003 in the name D. Reeve. Proof of identity: Daniel's driving licence, which he reported lost in December 2002.</p>
<p>Contact mobile number on the form: 07700 900 162.</p>
<p>That number is registered to <b>Mark Ostler</b>.</p>
<p>Collection book: post signed for six times, each with the initials "D.R." in a hand the examiner says is not Daniel's.</p>`},
    {id:"mark_int", cat:"Interviews", title:"Mark Ostler, 2003 interview", sub:"The flatmate's account.", type:"Interview record", ref:"WB/03/0932/10", date:"10 Mar 2003",
     body:`<p>"Dan came in from work about half five. He was in a rush, said he had a source to meet in Linfield.</p>
<p>He left about quarter past nine. Red jacket, the one he always wore, hood up because it was chucking it down. That is the last I saw of him.</p>
<p>I was in all night. I was on the computer, online, from about six o'clock right through till two in the morning. I never left the flat."</p>
<p class="q">Q. Who has a key to garage 7?</p>
<p>"It hangs in the hall. Anyone who has been in the flat could have taken it, I suppose."</p>`},
    {id:"server", cat:"Records", title:"Game server logs", sub:"Checking 'online from six till two'.", requires:"mark_int", type:"Server records", ref:"WB/23/0118/7", date:"6 to 7 Mar 2003",
     body:`<p>Account "ostler_m", supplied by the game's publisher from archived logs.</p>
<div class="tw"><table class="rec"><tr><th>Log in</th><th>Log out</th></tr>
<tr><td>15:02</td><td>17:48</td></tr>
<tr><td>23:31</td><td>02:10</td></tr></table></div>
<p>No connection from the flat's internet line between 17:48 and 23:31.</p>`},
    {id:"bannister_f", cat:"Interviews", title:"Cllr Bannister file", sub:"The 2003 prime suspect.", type:"Interview record and minutes", ref:"WB/03/0932/11", date:"Mar 2003",
     body:`<p>Voicemail left on the Gazette newsdesk, 4 March: "Tell your boy to be very careful what he prints. I have solicitors."</p>
<p>Bannister, interviewed: "I was angry and I said so. The story was wrong and his editor agreed with me. I never met the lad."</p>
<p>Council minutes, 6 March: planning committee, Cllr Bannister in the chair, opened 19:00, closed 22:15. Fourteen members and two clerks present. He is recorded speaking on every item.</p>
<p>Taxi receipt, town hall to his home, 22:40.</p>
<p>From 17:00 to 18:45 he was at a site visit with three council officers.</p>`},
    {id:"leah_int", cat:"Interviews", title:"Leah Simmons, 2003 interview", sub:"The girlfriend and the text message.", type:"Interview record", ref:"WB/03/0932/12", date:"10 Mar 2003",
     body:`<p>"We had a row on Tuesday about him working all hours. We had made it up. He was meant to come to the bar at nine.</p>
<p>I worked the Vaults from four in the afternoon till half past midnight. The till has my code on it all night and my manager was there.</p>
<p>The text was odd. Dan never wrote 'tonite' or missed out an apostrophe. He used to correct mine. He wrote texts like he was writing for the paper."</p>`},
    {id:"pete_int", cat:"Interviews", title:"Pete Lawson, 2003 interview", sub:"How the jacket ended up in his locker.", type:"Interview record", ref:"WB/03/0932/13", date:"12 Mar 2003",
     body:`<p>"I was on from nine till five in the morning. I never saw the lad.</p>
<p>About half eleven I was emptying the bin in the gents on Platform 1 and there was a red jacket stuffed in it with a woolly hat in the pocket. Good jacket. Dry on the inside. I took it. I should not have. With my record I knew how it would look, so I said nothing till you opened my locker.</p>
<p>Platform 1 is where the trains from Linfield come in. The 22:31 from Linfield gets in at 22:52."</p>
<p>Staff rota and the signal box log confirm Lawson was in the ticket office or signal box from 21:00 to 23:25.</p>`},
    {id:"susan_int", cat:"Interviews", title:"Susan Clegg, 2003 interview", sub:"The editor who pulled his story.", type:"Interview record", ref:"WB/03/0932/14", date:"11 Mar 2003",
     body:`<p>"I spiked the council story on the lawyers' advice. He did not have a second source. He shouted, I shouted. That is newsrooms.</p>
<p>On Thursday I drove to Leeds for the regional press awards. I was at a table of ten from seven until eleven. There are photographs.</p>
<p>He rang the desk at twenty to five to say he would not be in early on Friday. He told the secretary he had 'a problem closer to home to sort out tonight'. He did not mention Linfield or any source."</p>`},
    {id:"jacket", cat:"Forensics", title:"Jacket and hat: 2023 testing", sub:"Retested with modern methods.", requires:"pete_int", type:"Laboratory report", ref:"WB/23/0118/L5", date:"2 Oct 2023",
     body:`<p>Red waterproof jacket and dark beanie, recovered from P. Lawson's locker in 2003.</p>
<p>Right pocket: a train ticket, single, <b>Linfield to Wexborough, issued 22:24</b>, paid cash.</p>
<p>Inside of the beanie: DNA from two men. One is Daniel Reeve. The other, which is the stronger and more recent deposit, matches <b>Mark Ostler</b>.</p>
<p>No trace of Peter Lawson on the inside of either item.</p>`}
  ],
  killer:"mark",
  motives:[
    {id:"a", t:"Daniel had found loans taken out in his name and was about to report who did it", ok:true},
    {id:"b", t:"To stop the council land story being published"},
    {id:"c", t:"Jealousy and anger after an argument in the relationship"},
    {id:"d", t:"A robbery for his jacket, phone and wallet"}
  ],
  howQ:"Who is the figure on the station CCTV at 21:50?",
  hows:[
    {id:"a", t:"The killer, wearing Daniel's jacket to create a false last sighting", ok:true},
    {id:"b", t:"Daniel, on his way to meet a source in Linfield"},
    {id:"c", t:"A member of station staff wearing a jacket from lost property"},
    {id:"d", t:"An unknown source who had borrowed Daniel's jacket"}
  ],
  solution:[
    "<b>Mark Ostler killed Daniel Reeve.</b> Using Daniel's lost driving licence, Mark opened a PO box, a credit card and two loans in his name. The PO box form carries Mark's own mobile number.",
    "On Thursday Daniel found a final demand at home, rang the bank at 12:15, and told two people he had a problem 'closer to home' to sort out that night. He bought a takeaway at 17:22 and went back to the flat. Mark logged off his game at 17:48.",
    "The pathologist says Daniel died within two hours of that meal, so by about half past seven. His shoes were dry, and it rained from eight. He never left the building alive. The 18:05 text, with spelling Daniel never used, was Mark buying time.",
    "At 21:20 Mark put on the red jacket and hat, took Daniel's phone, and let the station cameras see 'Daniel' leave town. The figure is 173 cm. Daniel was 188. Mark is 5 ft 8. He paid cash for a ticket although Daniel had a rail pass, came back on the 22:31, dumped the jacket on Platform 1, drew £200 with Daniel's card at 23:20 and was back online at 23:31.",
    "The garage key hung in the flat's hallway and the padlock was relocked. Everyone else's alibi covers the real time of death: Bannister was on a site visit and then chairing a minuted meeting, Leah was on the till from four, Clegg was in Leeds, and Lawson's only offence was keeping a good jacket."
  ]
}
];

