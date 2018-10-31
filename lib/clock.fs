\ NRF52 clock words
\    Copyright (C) 2018  juju2013@github
\    BSD licensed, See LICENSE

$40000000   module clock
  $000      register thfclkstart
  $004      register thfclkstop
  $008      register tlfclkstart
  $00c      register tlfclkstop
  $010      register tcal
  $014      register tctstart
  $018      register tctstop

  $100      register ehfclk
  $104      register elfclk
  $10c      register edone
  $110      register ectto
  
  $304      register  intenset
  $308      register  intenclr

  $408      register  hfclkrun
  $40c      register  hfclkstat
  $414      register  lfclkrun
  $418      register  lfclkstat
  $41c      register  lfclksrccopy
  $518      register  lfclksrc

  $538      register  ctiv
  $55c      register  traceconfig
