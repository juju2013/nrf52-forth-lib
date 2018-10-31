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

include iomodule.fs
\ include io.fs
\ include timer.fs
\ include i2s.fs
include gpio.fs
include nvic.fs
include spim.fs

\ : isr-reset reset ;
\ ['] isr-reset irq-NVIC3 !

