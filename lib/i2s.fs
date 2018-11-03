\ nrf52 i2S words
\    Copyright (C) 2018  juju2013@github
\    BSD licensed, See LICENSE

0 variable i2s.state
\ 11 constant debug1
\ 12 constant debug2
\ gpio.outpu debug1 &pin_cnf !
\ gpio.outpu debug2 &pin_cnf !

\ -------------------- register address
$40025000 module I2S
      $0  register tstart
      $4  register tstop
      
    $104  register erxptrupd
    $108  register estopped
    $114  register etxptrupd
    
    $300  register inten
    $304  register intenset
    $308  register intenclr
    $500  register en
    $504  register mode
    $508  register rxen
    $50c  register txen
    $510  register mcken
    $514  register MCKFREQ
    $518  register ratio
    $51C  register swidth
    $520  register calign \ don't foo forth word
    $524  register format
    $528  register channels
    $538  register rxdptr
    $540  register txdptr
    $550  register maxcnt
    $560  register psel.mck
    $564  register psel.sck
    $568  register psel.lrck
    $56c  register psel.sdin
    $570  register psel.sdout

\ clear all events, eg, clear all pending interrupts
: i2s.evtclr
  i2s etxptrupd 0!
  i2s erxptrupd 0! 
  i2s estopped 0!
;

: i2s.start ( -- ) i2s.evtclr   i2s.state 0! i2s tstart 1!  ;
: i2s.stop  ( -- ) i2s.evtclr 2 i2s.state !  i2s tstop 1!  ;

: i2s.master ( ) i2s mode 0! ;
: i2s.slave ( )  i2s mode 1! ;

\ set master clock frequence, in khz
: i2s.mckfreq ( freq -- )   \ freq in khz
  case
    16000 of $80000000 endof
    10666 of $50000000 endof
     8000 of $40000000 endof
     6400 of $30000000 endof
     5333 of $28000000 endof
     4000 of $20000000 endof
     3200 of $18000000 endof
     2909 of $16000000 endof
     2133 of $11000000 endof
     2000 of $10000000 endof
     1523 of $0c000000 endof
     1391 of $0b000000 endof
     1066 of $08800000 endof
     1032 of $08400000 endof
     1000 of $08000000 endof
      761 of $06000000 endof
      507 of $04100000 endof
             $020C0000 \ 256 Khz
  endcase
  i2s MCKFREQ !
;

\ set pins
: i2s.setpin ( pin register -- ) \ 0 to disconnect
  swap dup 0= if drop $ffffffff then
  swap !
;

\ dump internal i2s states
: i2s.debug.evt
 ." i2s.rxptrupd=$" i2s erxptrupd @ hex. cr
 ." i2s.stopped= $" i2s estopped @ hex. cr
 ." i2s.txptrupd=$" i2s etxptrupd @ hex. cr
;

: i2s.debug
 ." I2S internal:" cr
 i2s.debug.evt
 ." i2s.start=   $" i2s tstart @ hex. cr
 ." i2s.stop=    $" i2s tstop @ hex. cr
 ." i2s.inten=   $" i2s inten @ hex. cr
 ." i2s.en=      $" i2s en @ hex. cr
 ." i2s.rxen=    $" i2s rxen @ hex. cr
 ." i2s.txen=    $" i2s txen @ hex. cr
 ." i2s.mcken=   $" i2s mcken @ hex. cr
 ." i2s.mckfreq= $" i2s mckfreq @ hex. cr
 ." i2s.ratio=   $" i2s ratio @ hex. cr
 ." i2s.swidth=  $" i2s swidth @ hex. cr
 ." i2s.rxd=     $" i2s rxdptr @ hex. cr
 ." i2s.txd=     $" i2s txdptr @ hex. cr
 ." i2s.maxcnt=  $" i2s maxcnt @ hex. cr
 cr
 ." i2s.psel.mck =$"  i2s   psel.mck @ hex. cr
 ." i2s.psel.sck =$"  i2s   psel.sck @ hex. cr
 ." i2s.psel.lrck =$" i2s   psel.lrck  @ hex. cr
 ." i2s.psel.sdin =$" i2s   psel.sdin  @ hex. cr
 ." i2s.psel.sdout=$" i2s   psel.sdout @ hex. cr
cr
 ." ws.state=    " i2s.state @ . cr
;


\ ISR, internal state machine to send only 1 full frame
: isr-i2s
  i2s.evtclr
  i2s.state @ dup 2 = if i2s.stop then
  1+ i2s.state !
;

\ enable i2s interrupt
: i2s.irqen ( -- )
  ['] isr-i2s irq-NVIC37 !
\  76543210
  %00100000 i2s intenset !  \ only enable txptrupd event
  37 irq.en   \ XXX: FIXME: less hardcoded way to do this ?
;
\ disable i2s interrupt
: i2s.irqden ( -- )
  i2s.stop
  37 irq.den   \ XXX: FIXME: less hardcoded way to do this ?
  ['] unhandled irq-NVIC37 !
  0 i2s intenclr !
;
