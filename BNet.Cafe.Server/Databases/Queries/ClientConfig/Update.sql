UPDATE client_config 
SET 
    DBTimeZone = '{DBTimeZone}',
    DBAccountCreationAllowed = '{DBAccountCreationAllowed}',
    DBAutoShutDownInterval = '{DBAutoShutDownInterval}',
    DBDesktopSlideShow = '{DBDesktopSlideShow}',
    DBResetShutdownCountdown = '{DBResetShutdownCountdown}'
WHERE DBId = '{DBId}';