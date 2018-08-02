;; remember to "clear" the eromon list (i.e., by filtering eromons - like N rarity) before activating the script,
;; otherwise it might end up accidentally clicking on another eromon.


OFFSET_X = 0
OFFSET_Y = 0


USE_X = 980
USE_Y = 460
USE_DELAY = 100

CONFIRM_X = 600
CONFIRM_Y = 500
CONFIRM_DELAY = 500

DISMISS_DELAY = 100


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
F7::Pause
