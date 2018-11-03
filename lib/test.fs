\ test io words
\    Copyright (C) 2018  juju2013@github
\    BSD licensed, See LICENSE

\ eraseflash
\ compiletoflash
\ include ../embello/hexdump.fs
\ include ../common/disassembler-m3.txt
\ : ----embello---- here , ;
\ 

compiletoram

26 constant twim0.clk
27 constant twim0.dat
8 32 * constant nbleds
9  constant pin.ws
8  constant pin.lrck
7  constant pin.sck



include iomodule.fs
include gpio.fs
include rng.fs
\ include io.fs
include nvic.fs
\ include timer.fs
include i2s.fs
\ include spim.fs
\ include twim.fs
\ include mpu6050.fs
include ws2812.fs

\ : isr-reset reset ;
\ ['] isr-reset irq-NVIC3 !
: test ws.init ws.display ;

: test2 
  ws.init
  begin
    0 0 0 ws.fill
    rng@ 16 mod 3 +  wait 0 do 
      3 0 do rng@ 8 mod wait wait wait loop rng@ ws.pixel 
    loop
    ws.display
    pause
    2000 0 do wait loop
  key? until 
;

: t ws.display ;
