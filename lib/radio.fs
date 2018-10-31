\ NRF52 radio words
\    Copyright (C) 2018  juju2013@github
\    BSD licensed, See LICENSE


$40001000 module    radio
  $000    register  ttxen
  $004    register  trxen
  $008    register  tstart
  $00c    register  tstop
  $010    register  tdisable
  $014    register  trssistart
  $018    register  trssistop
  $01c    register  tbcstart
  $020    register  tbcstop
  
  $100    register  eready
  $104    register  eaddress
  $108    register  epayload
  $10c    register  eend
  $110    register  edisabled
  $114    register  edevmatch
  $118    register  edevmiss
  $11c    register  erssiend
  $128    register  ebcmatch
  $130    register  ecrcok
  $134    register  ecrcerror

  $200    register  shorts
  
  $304    register  intenset
  $308    register  intenclr
  
  $400    register  crcstatus
  $408    register  rxmatch
  $40C    register  rxcrc
  $410    register  dai

  $504    register  packetprt
  $508    register  frequency
  $50C    register  txpower
  $510    register  mode
  $514    register  pcnf0
  $518    register  pcnf1
  $51C    register  base0
  $520    register  base1
  $524    register  prefix0
  $528    register  prefix1
  $52c    register  txaddress
  $530    register  rxaddresses
  $534    register  crccnf
  $538    register  crcpoly
  $53c    register  crcinit
  
  $544    register  tifs
  $548    register  rssisample
  $550    register  radiostate \ don't confuse corevariable
  $554    register  datawhiteiv
  $560    register  bcc

  $600    register  dab0
  $620    register  dap0

: dab ( # -- addr )  2 lshift radio dab0 + ;
: dap ( # -- addr )  2 lshift radio dap0 + ;

  $640    register  dacnf
  $650    register  modecnf0
  $ffc    register  power
