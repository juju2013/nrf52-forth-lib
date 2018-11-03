\ nrf ws2812b driver with i2s
\    Copyright (C) 2018  juju2013@github
\    BSD licensed, See LICENSE
\   original idea from https://electronut.in/nrf52-i2s-ws2812/
\   Need 4 words before inclusion of this driver :
\     PIN.ws PIN.lrck PIN.sck and nbleds
\   will use 3 pins but only PIN.ws is usefull to drive ws2812,
\   other pins are unused by required by I2S module
\   will also consume 12 bytes per pixel...

nbleds 3 * 2 + constant nbwords \ word 4 bytes

\ the frame buffer
nbwords cells buffer: ws.fb

\ internal variables
0 variable ws.r
0 variable ws.g
0 variable ws.b

\  1 byte rgb color to 32 bits ws2812 style color code, %0 -> %1000,  %1=%1110
\  4 LSB + 4 MSB
: ws.32bcolor ( c -- cellC )
  0 0 3 do
    4 lshift
    over i bit and if %1110 else %1000 then or 
  -1 +loop
  4 7 do
    4 lshift
    over i bit and if %1110 else %1000 then or 
  -1 +loop
  swap drop
;
\ write GRB color code to frame-buffer @ addr...addr+11bytes 
: ws.rgb ( r g b addr -- addr+12 )
  swap ws.32bcolor over 8 + ! ( r g addr )
  swap ws.32bcolor over ! ( r addr )
  swap ws.32bcolor over 4 + ! ( add+4 )
  12 +
;
\ convert 32bit ws encoded to 8bit value
: ws.rgb@ ( val -- c )
  dup 16 lshift swap 16 rshift or \ invert hi/lo 16 bits
  0 swap ( c val )
  8 0 do
    dup %0110 and if i bit else 0 then ( c val f )
    rot or swap 4 rshift ( c val>>4 )
  loop 
  drop ( c )
;
\ get rgb value at pixel
: ws.pixel@ ( pixel -- r g b )
  3 * cells ws.fb + ( addr )
  dup @ ws.rgb@ swap 4 + ( g addr+4 )
  dup @ ws.rgb@ swap 4 + ( g r addr+8 )
      @ ws.rgb@  ( g r b )
  rot swap (  r g b )
;

\ fill the frame buffer with a color
: ws.fill ( r g b -- )
  ws.b ! ws.g ! ws.r !
  ws.fb nbleds 0 do >r ws.r @ ws.g @ ws.b @ r> ws.rgb loop drop
;

\ release all resources
: ws.deinit
  i2s.stop
  i2s.irqden
;

\ Module initialization, acquire all resources
: ws.init
  0 0 1 ws.fill

  \ disable i2s then  configure
  i2s en 0!
  i2s mcken 0!
 
  i2s.master
  3200 i2s.mckfreq
  1    i2s swidth ! \ 16 bit width
  0    i2s ratio ! \ MCK/32
  \ The following 3 pins MUST be setted to different PINs
  \ otherwise I2S will NOT work at all
  \ only pin.ws is useful to drive ws2812
  pin.ws   i2s psel.sdout i2s.setpin
  pin.lrck i2s psel.lrck  i2s.setpin
  pin.sck  i2s psel.sck   i2s.setpin
  \ optinal pins
  0        i2s psel.sdin  i2s.setpin
  0        i2s psel.mck   i2s.setpin
  
  ws.fb   i2s txdptr !
  nbwords i2s maxcnt !
  
  \ enable i2s
  i2s.irqen
  i2s mcken 1!
  i2s txen 1!
  i2s en 1!
  ." ws2812/i2s initialized" cr
;

\ set pixel color
: ws.pixel ( r g b led# -- )
  3 * cells ws.fb + ws.rgb drop
;

\ start send frame content
: ws.display
  i2s.start
  begin i2s.state @ 1 > until
;

\ dump fb content
: ws.dump
  ws.fb nbwords 2 lshift dump
;

\ convert x,y to pixel number
\       x 0-->
\  y  1 16 17 32 33 48 49
\  0  2 15 18 31 34 47 50
\  |  3 14 19 30 35 46 51 
\  |  4 13 20 29 36 45 52
\  V  5 12 21 28 37 44 53
\     6 11 22 27 38 43 54 
\     7 10 23 26 39 42 55
\     8 9  24 25 40 41 56
: ws.xy ( x y -- led# )
  \ formula :
  \ ===M1=======   ====m2================   ====m3=====
  \ Y*MOD(X+1;2) + (15-MOD(Y;8))*MOD(X;2) + INT(X/2)*16
  over 1 + 2 mod over *                       \ ." m1:" .v ( x y m1 )
  -rot dup 8 mod 15 swap - 2 pick 2 mod *     \ ." m2:" .v ( m1 x y m2 ) 
  -rot drop 2 / 16 *                          \ ." m3:" .v ( m1 m2 m3 )
  + +
;
