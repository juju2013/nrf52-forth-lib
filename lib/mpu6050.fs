\ nRF52 MPU6050 driver
\  need twim0 module

$1A constant mpu.CONFIG
$1B constant mpu.GYRO_CONFIG
$1C constant mpu.ACCEL_CONFIG     
$1D constant mpu.FF_THR           
$1E constant mpu.FF_DUR           
$1F constant mpu.MOT_THR          
$20 constant mpu.MOT_DUR          
$21 constant mpu.ZRMOT_THR        
$22 constant mpu.ZRMOT_DUR        

$37 constant mpu.INT_PIN_CFG      
$38 constant mpu.INT_ENABLE       
$39 constant mpu.DMP_INT_STATUS   
$3A constant mpu.INT_STATUS       
$3B constant mpu.ACCEL_XOUT
$3D constant mpu.ACCEL_YOUT
$3F constant mpu.ACCEL_ZOUT
$41 constant mpu.TEMP_OUT
$43 constant mpu.GYRO_XOUT
$45 constant mpu.GYRO_YOUT
$47 constant mpu.GYRO_ZOUT

$61 constant mpu.MOT_DETECT_STATUS
$67 constant mpu.I2C_MST_DELAY_CTRL
$68 constant mpu.SIGNAL_PATH_RESET
$69 constant mpu.MOT_DETECT_CTRL  
$6A constant mpu.USER_CTRL        
$6B constant mpu.PWR_MGMT_1       
$6C constant mpu.PWR_MGMT_2       

0 variable mpu.detected

: mpu.detected? ( -- flag )
  mpu.detected @
  0 mpu.detected !
;

\ write mpu register
: >mpu ( reg val -- )
  swap twim0.txbuf c! twim0.txbuf 1+ c!
  2 &twim0.txd.maxcnt !
  twim0.WO
  twim0.wait
;

\ read mpu register 1byte
: mpu> ( reg -- val )
  twim0.txbuf c! 0 twim0.rxbuf !
  1 &twim0.txd.maxcnt ! 1 &twim0.rxd.maxcnt !
  twim0.W&R
  twim0.wait
  twim0.rxbuf c@
;
\ read 2 mpu register for 16 bits value
: 2mpu> ( reg -- val )
  dup mpu> 8 lshift swap 1+ mpu> +
;

\ 100th °C
: mpu.temp ( -- )
  $41 2mpu> 16 lshift 100 340 16 lshift */ 3653 +
;

: mpu.int ( -- )
  0 0 &io.evt ! \ clear event
  1 mpu.detected !
;

: mpu.init ( -- )
  twim0.init
  $68 &twim0.addr !
  mpu.PWR_MGMT_1        1 >mpu

  \ setup interrupt
  0 21 io.gpiote.event io.gpiote.TG io.gpiote.L io.gpiote.conf
  ['] mpu.int irq-NVIC6 !
  6 irq.en
    
  \ -- enable Motion interrupt
  mpu.SIGNAL_PATH_RESET 7   >mpu
  mpu.INT_PIN_CFG       30  >mpu
  mpu.ACCEL_CONFIG      1   >mpu
  mpu.MOT_THR           2   >mpu
  mpu.MOT_DUR           4   >mpu
  mpu.MOT_DETECT_CTRL   $15 >mpu
  mpu.INT_ENABLE        $40 >mpu
  
  0 bit io.gpiote $304 + !
  
;

: mpu.reset ( -- )
  7 bit $6B >mpu
;

: mpu.deinit ( -- )
  0 bit io.gpiote $308 + !
  mpu.PWR_MGMT_1        0 >mpu
  6 irq.den
  ['] unhandled irq-NVIC6 !
;


: mpu. ( -- ) \ dump mpu regs
  cr ." 00: -0 -1 -2 -3 -4 -5 -6 -7 -8 -9 -A -B -C -D -E -F"
  $FF $0 do 
    i 16 /mod drop 0= if cr i 16 / h.2 ." :" then ."  " 
    i mpu> h.2
  loop
  cr
;

: mpu.selftest ( -- )
  \ Gyro selftest
  $1B
  dup %11100 3 lshift >mpu
  dup %11101 3 lshift >mpu
  dup %11110 3 lshift >mpu
      %11111 3 lshift >mpu
  \ Accel selftest
  $1C
  dup %11100 3 lshift >mpu
  dup %11101 3 lshift >mpu
  dup %11110 3 lshift >mpu
      %11111 3 lshift >mpu
  wait
;

: mpu.monitor
  begin
    pause mpu.detected? if
      ." Mouvement detected!" cr
      ." MPU.INT:" mpu.INT_STATUS mpu> h.2 cr
      ." MPU ACCL:" mpu.ACCEL_XOUT 2mpu> h.4 mpu.ACCEL_YOUT 2mpu> h.4 mpu.ACCEL_ZOUT 2mpu> h.4  cr
      ." MPU GYRO:" mpu.GYRO_XOUT 2mpu> h.4 mpu.GYRO_YOUT 2mpu> h.4 mpu.GYRO_ZOUT 2mpu> h.4  cr
    then
  key? until
;
