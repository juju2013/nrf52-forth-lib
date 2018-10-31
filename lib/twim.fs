\ nrf52 twim ( i2c Master with EasyDMA ) words
0 variable twim.debug

\ ------------------- register address
$40003000	constant TWIM0
$40003000	constant TWIM1

\ tasks
twim0        constant &twim0.startrx
twim0 $008 + constant &twim0.starttx
twim0 $014 + constant &twim0.stop
twim0 $01c + constant &twim0.suspend
twim0 $020 + constant &twim0.resume

\ events
twim0 $104 + constant &twim0.stopped
twim0 $124 + constant &twim0.error
twim0 $148 + constant &twim0.suspended
twim0 $14c + constant &twim0.rxstarted
twim0 $150 + constant &twim0.txstarted
twim0 $15c + constant &twim0.lastrx
twim0 $160 + constant &twim0.lasttx

\ pointers & counters
twim0 $534 + constant &twim0.rxd.ptr
twim0 $538 + constant &twim0.rxd.maxcnt
twim0 $53C + constant &twim0.rxd.amount
twim0 $540 + constant &twim0.rxd.list

twim0 $544 + constant &twim0.txd.ptr
twim0 $548 + constant &twim0.txd.maxcnt
twim0 $54C + constant &twim0.txd.amount
twim0 $540 + constant &twim0.txd.list

twim0 $588 + constant &twim0.addr

\ shorts
twim0 $200 + constant &twim0.short
 7 bit       constant twim.tx->rx
 8 bit       constant twim.tx->suspend
 9 bit       constant twim.tx->stop
10 bit       constant twim.rx->tx
12 bit       constant twim.rx->stop

\ interrupts
twim0 $304 + constant &twim0.intset
twim0 $304 + constant &twim0.intclr
 1 bit       constant twim.int.stopped
 9 bit       constant twim.int.error
18 bit       constant twim.int.suspended
19 bit       constant twim.int.rxstart
20 bit       constant twim.int.txstart
23 bit       constant twim.int.lastrx
24 bit       constant twim.int.lasttx

\ error source
twim0 $4c4 + constant &twim0.errsrc
 0 bit       constant twim.err.overrun
 1 bit       constant twim.err.anack
 2 bit       constant twim.err.dnack

\ enable, disable
: twim0.en ( -- )
 6 twim0 $500 + !
;
: twim0.den ( -- )
 0 twim0 $500 + !
;

\ pin select 
: twim0.scl ( pin# -- )
  twim0 $508 + !
;
: twim0.sda ( pin# -- )
  twim0 $50c + !
;

\ frequenice, in kbps
: twim0.freq ( freq -- )
  dup 100 = if $01980000 else
  dup 250 = if $04000000 else
               $06400000 \ default
  then then
  twim0 $524 + !
  drop
;

\ quick modes with shorts
: twim0.RO ( -- )  twim.rx->stop &twim0.short ! 1 &twim0.startrx ! ;
: twim0.WO ( -- )  twim.tx->stop &twim0.short ! 1 &twim0.starttx ! ;
: twim0.W&R ( -- ) twim.tx->rx twim.rx->stop + &twim0.short ! 1 &twim0.starttx ! ;
: twim0.R&W ( -- ) twim.rx->tx twim.tx->stop + &twim0.short ! 1 &twim0.startrx ! ;


: twim0.debug 
  ." TWIM0 registers:" cr
  ." &startrx    =$"     &twim0.startrx     @ hex. cr
  ." &starttx    =$"     &twim0.starttx     @ hex. cr
  ." &stop       =$"     &twim0.stop        @ hex. cr
  ." &suspend    =$"     &twim0.suspend     @ hex. cr
  ." &resume     =$"     &twim0.resume      @ hex. cr
  ." &stopped    =$"     &twim0.stopped     @ hex. cr
  ." &error      =$"     &twim0.error       @ hex. cr
  ." &suspended  =$"     &twim0.suspended   @ hex. cr
  ." &rxstarted  =$"     &twim0.rxstarted   @ hex. cr
  ." &txstarted  =$"     &twim0.txstarted   @ hex. cr
  ." &lastrx     =$"     &twim0.lastrx      @ hex. cr
  ." &lasttx     =$"     &twim0.lasttx      @ hex. cr
  ." &rxd.ptr    =$"     &twim0.rxd.ptr     @ hex. cr
  ." &rxd.maxcnt =$"     &twim0.rxd.maxcnt  @ hex. cr
  ." &rxd.amount =$"     &twim0.rxd.amount  @ hex. cr
  ." &rxd.list   =$"     &twim0.rxd.list    @ hex. cr
  ." &txd.ptr    =$"     &twim0.txd.ptr     @ hex. cr
  ." &txd.maxcnt =$"     &twim0.txd.maxcnt  @ hex. cr
  ." &txd.amount =$"     &twim0.txd.amount  @ hex. cr
  ." &txd.list   =$"     &twim0.txd.list    @ hex. cr
  ." &addr       =$"     &twim0.addr        @ hex. cr
;

16 buffer: twim0.txbuf
16 buffer: twim0.rxbuf

0 variable twim0.error
0 variable twim0.stopped
\ test end of i2c txn
: twim0.end  ( -- flag ) \ != 0 for end of transaction, stopped or error
  twim0.error @ dup 0= if drop twim0.stopped @ then
;
\ wait end of transaction
: twim0.wait ( -- )
  begin pause twim0.end until
;

\ clear all events
: twim0.evtclr
  twim.debug @ if twim0.debug then
  &twim0.stopped @ twim0.stopped ! 0 &twim0.stopped !
  &twim0.error   @ twim0.error   ! 0 &twim0.error !
  0 &twim0.suspended !
  0 &twim0.rxstarted !
  0 &twim0.txstarted !
  0 &twim0.lastrx !
  0 &twim0.lasttx !
;

: isr.twim0
  &twim0.stopped @ twim0.end !
  twim0.evtclr
;

\ i2c test
: twim0.init
  twim0.den
  twim0.clk twim0.scl
  twim0.dat twim0.sda
  100 twim0.freq

  twim0.txbuf &twim0.txd.ptr ! 1  &twim0.txd.maxcnt !
  twim0.rxbuf &twim0.rxd.ptr ! 16 &twim0.rxd.maxcnt !

  
  ['] isr.twim0 irq-NVIC3 !
   twim.int.stopped
   twim.int.error +
   twim.int.suspended +
   twim.int.rxstart +
   twim.int.txstart +
   twim.int.lastrx +
   twim.int.lasttx +
  &twim0.intset !
  
  3 irq.en
  twim0.en
;
