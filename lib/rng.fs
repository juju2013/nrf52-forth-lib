\ nrf62 random number generator words
\    Copyright (C) 2018  juju2013@github
\    BSD licensed, See LICENSE

\ ----------------- register addresses
$4000D000  module rng
  $0    register tstart
  $004  register tstop
  $100  register evalrdy
  $200  register shorts
  $304  register intenset
  $308  register intenclr
  $504  register config
  $508  register value


: rng.init
  rng shorts 1! \ generate only 1 value then stop
  rng tstart 1!
;

\ get a previously generated random number
\   make sur some delay between calling this !
: rng@ ( -- c )
  rng value @ \ return previous value
  rng tstart 1! \ start generate another one
;

rng.init
