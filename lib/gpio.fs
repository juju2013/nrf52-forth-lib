\ NRF52 basic io words
\    Copyright (C) 2018  juju2013@github
\    BSD licensed, See LICENSE

\ turn a bit position into a single-bit mask
: bit ( u -- u )  
  1 swap lshift  1-foldable 
;
$50000000   module gpio
  $504      register out
  $508      register outset
  $50c      register outclr
  $510      register in
  $514      register dir
  $518      register dirset
  $51c      register dirclr
  $520      register latch
  $524      register detectmode
  
  $700      register pin_cnf

\ some common io pin modes
\  10fedcba9876543210
  %000000000000000111 constant gpio.outpd
  %000000000000001111 constant gpio.outpu
  %000000000000000000 constant gpio.input

\ pin_cnf multiplex
: &pin_cnf ( pin# -- addr )
  2 lshift gpio pin_cnf +
;
\ pin out
: ios ( pin# -- )
  bit gpio outset !
;

: ioc ( pin# -- )
  bit gpio outclr !
;
