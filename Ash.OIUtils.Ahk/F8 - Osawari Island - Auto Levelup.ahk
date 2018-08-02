OFFSET_X = 0
OFFSET_Y = 0


USE_X = 1000
USE_Y = 310
USE_DELAY = 500

CONFIRM_X = 620
CONFIRM_Y = 600
CONFIRM_DELAY = 4000

DISMISS_DELAY = 500


USE_X += %OFFSET_X%
USE_Y += %OFFSET_Y%
CONFIRM_X += %OFFSET_X%
CONFIRM_Y += %OFFSET_Y%


Pause
	Loop {
		Click %USE_X%, %USE_Y%, 0
		Sleep 500
		Click %USE_X%, %USE_Y%, 9
		Sleep %USE_DELAY%

		Click %CONFIRM_X%, %CONFIRM_Y%, 0
		Sleep 500
		Click %CONFIRM_X%, %CONFIRM_Y%, 9
		Sleep %CONFIRM_DELAY%
		
		Click %USE_X%, %USE_Y%, 0
		Sleep 500
		Click %USE_X%, %USE_Y%, 9
		Sleep %DISMISS_DELAY%
	}
F8::Pause
