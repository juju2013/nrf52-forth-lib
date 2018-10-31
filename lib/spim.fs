\ nRF62 SPI master with EasyDMA

\ module definition
$40003000	  module spim

\ instantiation

\ registers
  $010	    register tstart
  $014	    register tstop
  $01C	    register tsuspend
  $020	    register tresume
 
  $104	    register estopped
  $110	    register erxend
  $118	    register eend
  $120	    register etxend
  $14C	    register estarted

  $200	    register shorts
  $304	    register intset
  $308	    register intclr
  $500	    register enable

  $508	    register sck
  $50C	    register mosi
  $510	    register miso
  
  $524	    register khz

  $534	    register rxdptr
  $538	    register rxdmaxcnt
  $53C	    register rxdamount
  $540	    register rxdlist
 
  $544	    register txdptr
  $548	    register txdmaxcnt
  $54C	    register txdamount
  $550	    register txdlist
 
  $554      register config
  $5c0      register ORC
  
: spim.khz ( freq -- ) \ set spim frequence in kHZ
  swap case
    250 of $04000000 endof
    500 of $08000000 endof
   1000 of $10000000 endof
   2000 of $20000000 endof
   4000 of $40000000 endof
   8000 of $80000000 endof
    $02000000 \ 125Khz
  endcase
  dup spim khz ! spim khz ! \ xxx: bug ? only 2nd write take effet ???
;

\ isr related words
0 variable spim.ready
0 variable spim.estopped
0 variable spim.erxend
0 variable spim.eend
0 variable spim.etxend
0 variable spim.estarted

: spim.evtclear
  spim estopped 0!
  spim erxend 0!
  spim eend 0!
  spim etxend 0!
  spim estarted 0!
;
: spim.evtsave
  spim estopped @ spim.estopped !   \ clear all event register
  spim erxend @ spim.erxend !
  spim eend @ spim.eend !
  spim etxend @ spim.etxend !
  spim estarted @ spim.estarted !
;
: isr-spim
  spim.ready 1!     \ indic event ready
  spim.evtsave      \ store all event for later
  spim.evtclear     \ clear all event
;

\ test end of i2c txn
: spim.end  ( -- flag ) \ != 0 for end of transaction, stopped or error
  spim.ready @
;
\ wait end of transaction
: spim.wait ( -- )
  begin pause spim.end until
;
: spim.irqen
  \ 76543210
   %01010010 spim intset !
  3 irq.en
;
: spim.irqden
  \ 76543210
   %01010010 spim intclr !
  3 irq.den
;
: spim.init 
  spim.ready 0!
  spim.evtclear
  ['] isr-spim irq-NVIC3 !
  spim.irqen
  7 spim enable !
;
: spim.deinit 
  spim.irqden
  spim.evtclear
  spim.ready 0!
  ['] unhandled irq-NVIC3 !
  spim enable 0!
;
\ dump spim registers
: spim.debug
  ." SPIM debug:" cr
  ."    estopped =$" spim estopped @ hex. cr
  ."    erxend   =$" spim erxend @ hex. cr
  ."    eend     =$" spim eend @ hex. cr
  ."    etxend   =$" spim etxend @ hex. cr
  ."    estarted =$" spim estarted @ hex. cr
  ."    shorts   =$" spim shorts @ hex. cr
  ."    intset   =$" spim intset @ hex. cr
  ."    intclr   =$" spim intclr @ hex. cr
  ."    enable   =$" spim enable @ hex. cr
  ."    sck      =$" spim sck @ hex. cr
  ."    mosi     =$" spim mosi @ hex. cr
  ."    miso     =$" spim miso @ hex. cr
  ."    khz      =$" spim khz @ hex. cr
  ."    rxdptr   =$" spim rxdptr @ hex. cr
  ."    rxdmaxcnt=$" spim rxdmaxcnt @ hex. cr
  ."    rxdamount=$" spim rxdamount @ hex. cr
  ."    rxdlist  =$" spim rxdlist @ hex. cr
  ."    txdptr   =$" spim txdptr @ hex. cr
  ."    txdmaxcnt=$" spim txdmaxcnt @ hex. cr
  ."    txdamount=$" spim txdamount @ hex. cr
  ."    txdlist  =$" spim txdlist @ hex. cr
  ."    config   =$" spim config @ hex. cr
  ."    ORC      =$" spim ORC @ hex. cr
  cr
  ."    spim.ready   =$" spim.ready @ hex. cr
  ."    spim.estopped=$" spim.estopped @ hex. cr
  ."    spim.erxend  =$" spim.erxend @ hex. cr
  ."    spim.eend    =$" spim.eend @ hex. cr
  ."    spim.etxend  =$" spim.etxend @ hex. cr
  ."    spim.estarted=$" spim.estarted @ hex. cr
;
