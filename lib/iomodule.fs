\ nrf52 io modules

\ general syntax :
\  $baseaddr module spim ( instance# will be calculated )
\     $offset register reg1
\  spim instance# reg2 @ if 1 reg1 ! then

\ base36 to encode up to 12 alphanum in 1 cells
: alphanum 36 base ! ;

\ current module in use
0 variable _mid \ current module id

\ all module instance add the same offset
: instance0         1-foldable immediate ;
: instance1 $1000 + 1-foldable immediate ;
: instance2 $2000 + 1-foldable immediate ;
: instance3 $3000 + 1-foldable immediate ;
: instance4 $4000 + 1-foldable immediate ;

\ reset module
: demodule 0 _mid ! ;

\ how to define new module:
: module 
  >in @ >r                   \ save current input position
  token alphanum number decimal drop   \ try convert module name to base36 number
  \ 1 = if
  \   else ." cannot convert" quit 
  \ then
  r> >in !            \ restore current input position
  dup _mid !          \ store it
  dup ." _mid=$" hex. \ debug print it
  <builds               \ begin new definition
    ,  ,               \ store mid, mbase
    $40 setflags        \ set constant flag
  does>               \ begin execution 
   dup @ _mid !       \ store mid to _mid
   4 + @ immediate    \ push mbase
;

\ register is create under current _mid
: register
  _mid @ 0= if ." Must use module before register" quit then
  >in @ >r            \ save current input position
  token find drop     \ try find the lastest same word's xt
  r> >in !            \ restore current input position
  <builds             \ what to do when defining a register
    _mid @            \ ( $offset xt _mid  )
    , , ,             \ store them to _mid, xt, $offset
    $40 setflags      \ as constant
  does>
    dup @  ( & _mid )
    swap 4 + dup @  ( _mid & xt )
    swap 4 + @    ( _mid xt $offset )
    rot _mid @ <> if  \ not match current _mid, call previous one ( xt $offset )
      drop dup 0= if ." undefined register" quit then
      execute
    else              \ _mid match ( xt $offset )
      swap drop +               \ add $offset
    then
;


\ generic actions with registers
\ start a task
: 1! ( &register -- ) 1 swap ! ;

\ clear a event
: 0! ( &register -- ) 0 swap ! ;

