OFFSET_X = 0
OFFSET_Y = 0


EROMON_B_T_X = 430
EROMON_B_T_Y = 290

EROMON_F_T_X = 620
EROMON_F_T_Y = 290

EROMON_B_M_X = 370
EROMON_B_M_Y = 415

EROMON_F_M_X = 560
EROMON_F_M_Y = 415

; Y = 490 -> don't skip dialogues
; Y = 530 -> skip dialogues

EROMON_B_B_X = 310
EROMON_B_B_Y = 490

EROMON_F_B_X = 500
EROMON_F_B_Y = 490

DELAY_T = 1000
DELAY_M = 1200
DELAY_B = 1000


EROMON_F_B_X += %OFFSET_X%
EROMON_F_B_Y += %OFFSET_Y%

EROMON_F_M_X += %OFFSET_X%
EROMON_F_M_Y += %OFFSET_Y%

EROMON_F_T_X += %OFFSET_X%
EROMON_F_T_Y += %OFFSET_Y%


Pause
	Loop {
		Click %EROMON_F_B_X%, %EROMON_F_B_Y%, 0
		Sleep 500
		Click %EROMON_F_B_X%, %EROMON_F_B_Y%, 1
		Sleep %DELAY_B%
		
		Click %EROMON_F_M_X%, %EROMON_F_M_Y%, 0
		Sleep 500
		Click %EROMON_F_M_X%, %EROMON_F_M_Y%, 1
		Sleep %DELAY_M%
		
		Click %EROMON_F_T_X%, %EROMON_F_T_Y%, 0
		Sleep 500
		Click %EROMON_F_T_X%, %EROMON_F_T_Y%, 1
		Sleep %DELAY_T%
}
F9::Pause
