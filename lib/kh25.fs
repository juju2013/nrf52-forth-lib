\ NRF52 kh25 SPI flash words
\    Copyright (C) 2018  juju2013@github
\    BSD licensed, See LICENSE

: wait
  500 0 do nop loop
;
: wait2
  28 0 do nop loop
;

\ KH25L32 flash memory connections
20 constant kh25.cs
19 constant kh25.mosi
18 constant kh25.miso
17 constant kh25.clk
16 constant KH25.wp
15 constant KH25.hold

16  buffer: kh25.txbuf
$ff buffer: kh25.rxbuf

: kh.select kh25.cs ioc ;
: kh.deselect kh25.cs ios ;
: kh.ro kh25.wp ioc ;
: kh.wr kh25.wp ios ;

: test
  \ configure pins
  gpio.outpd kh25.cs &pin_cnf !
  gpio.outpd kh25.wp &pin_cnf !
  gpio.outpd kh25.hold &pin_cnf !
  
  kh25.mosi spim mosi !
  kh25.miso spim miso !
  kh25.clk  spim sck  !
  
  kh25.rxbuf spim rxdptr !
  16         spim rxdmaxcnt !
  0          spim rxdlist ! \ don't use arraylist

  kh25.txbuf spim txdptr !
  16         spim txdmaxcnt !
  0          spim txdlist ! \ don't use arraylist

  spim.init
  1000 spim.khz
  spim khz @ hex. ." khz" cr
  kh25.wp ios
  kh25.hold ios
;

: end spim.deinit ;

\ dump rxd buffer
: kh25.rxdump ( -- )
  \ spim rxdamount @ 0 do
  8 0 do
    kh25.rxbuf i + c@ h.2 ." :"
  loop
  cr
;

: kh25.rxclr ( -- ) $ff 2 rshift 0 do kh25.rxbuf i cells + 0! loop ;

\ write cmd to kh25
: >kh25 ( c -- c )
  kh25.txbuf c!
  spim txdmaxcnt 1!
  spim rxdmaxcnt 1!
  kh25.rxclr
  kh25.cs ioc
  spim.ready  0!
  spim tstart 1!
  wait2
  kh25.cs ios
  kh25.rxdump
;

: kh25.res ( -- )
  kh25.txbuf 0!
  $AB kh25.txbuf c!
  4 spim txdmaxcnt !
  5 spim rxdmaxcnt !
  kh25.rxclr
  kh25.cs ioc
  spim.ready  0!
  spim tstart 1!
  spim.wait
  kh25.cs ios
  kh25.rxdump
;

: kh25.rdid ( -- )
  kh25.txbuf 0!
  $9f kh25.txbuf c!
  1 spim txdmaxcnt !
  4 spim rxdmaxcnt !
  kh25.rxclr
  kh25.cs ioc
  spim.ready  0!
  spim tstart 1!
  spim.wait
  kh25.cs ios
  kh25.rxdump
;

: kh25.rems ( -- )
  kh25.txbuf 0!
  $90 kh25.txbuf c!
  4 spim txdmaxcnt !
  6 spim rxdmaxcnt !
  kh25.rxclr
  kh25.cs ioc
  spim.ready  0!
  spim tstart 1!
  spim.wait
  kh25.cs ios
  kh25.rxdump
;

: kh25.rdsfdp ( -- )
  $5a kh25.txbuf !
  0 kh25.txbuf 4 + !
  5   spim txdmaxcnt !
  $ff spim rxdmaxcnt !
  kh25.rxclr
  kh25.cs ioc
  spim.ready  0!
  spim tstart 1!
  spim.wait
  kh25.cs ios
  kh25.rxdump
;

: kh25.wpdis
  $01 kh25.txbuf c!
  $00 kh25.txbuf 1+ c!
  2 spim rxdmaxcnt !
  2 spim txdmaxcnt !
  kh25.rxclr
  kh25.wp ios
  kh25.cs ioc
  spim.ready  0!
  spim tstart 1!
  spim.wait
  kh25.cs ios
  kh25.wp ioc
  kh25.rxdump
;

: kh25.wren
  $06 kh25.txbuf c!
  1 spim rxdmaxcnt !
  1 spim txdmaxcnt !
  kh25.rxclr
  kh25.cs ioc
  spim.ready  0!
  spim tstart 1!
  wait2
  spim tstop 1!
  kh25.cs ios
  kh25.rxdump
;

: kh25.wrdi
  $04 kh25.txbuf c!
  1 spim rxdmaxcnt !
  1 spim txdmaxcnt !
  kh25.rxclr
  kh25.cs ioc
  spim.ready  0!
  spim tstart 1!
  wait2
  spim tstop 1!
  kh25.cs ios
  kh25.rxdump
;

: kh25.rdsr
  $05 kh25.txbuf c!
  3 spim rxdmaxcnt !
  1 spim txdmaxcnt !
  kh25.rxclr
  kh25.cs ioc
  spim.ready  0!
  spim tstart 1!
  spim.wait
  spim tstop 1!
  kh25.cs ios
  kh25.rxdump
;

: kh25.wrsr ( # -- )
  $01 kh25.txbuf c!
  kh25.txbuf 1+ c!
  2 spim rxdmaxcnt !
  2 spim txdmaxcnt !
  kh25.rxclr
  kh25.cs ioc
  spim.ready  0!
  spim tstart 1!
  spim.wait
  spim tstop 1!
  kh25.cs ios
  kh25.rxdump
;

: kh25.read16 ( addr -- )
  8 lshift $03 + kh25.txbuf !
  16 spim rxdmaxcnt !
  4  spim txdmaxcnt !
  kh25.rxclr
  kh25.cs ioc
  spim.ready  0!
  spim tstart 1!
  spim.wait
  spim tstop 1!
  kh25.cs ios
  kh25.rxdump
;

: kh25.se ( addr -- )
  8 lshift $20 + kh25.txbuf !
  4 spim rxdmaxcnt !
  4 spim txdmaxcnt !
  kh25.rxclr
  kh25.cs ioc
  spim.ready  0!
  spim tstart 1!
  spim.wait
  spim tstop 1!
  kh25.cs ios
  kh25.rxdump
;

test test
