# WildbowUpdateBot
Update notifier that monitors for new entries in the Wildbow web serials.

Written by Blackhole (Julia)
Find me at https://twitter.com/BlackholeTI and https://github.com/BlackholeGH
The Wildbow serials are by Wildbow (John C. McCrae).

Features

- The program runs in the background, minimized to the notification tray.
- The program will autorun on startup (this can be disabled in settings if you wish).
- The program will poll the website regularly, to check if a new update has been released. The poll interval can be set in settings to anywhere from 1 minute to 24 hours. This means that the program will notify you of new updates almost immediately (and faster than the RSS feed for the website, which I believe only updates hourly).
- Notifications will only be displayed when a new chapter/blog entry is posted.
- Notifications will appear on the bottom right of the screen, and will display the new chapter title, the subtitle, and the top banner image, as in the screenshot above. Notifications will disappear after 15 seconds if not interacted with.
- Clicking the notification will close it and open the web serial URL.
- Notifications are also accompanied by a chime to get your attention. You can adjust the volume of this (or turn it off completely) in settings.
- The program is designed for Pale updates, but is general enough that the current version will most likely work for future web serials published in the same way, or indeed any arbitrary Wordpress blog. You can specify the URL to monitor in settings, so that even if in the future the software is not updated for a future Wildbow serial, the current version should theoretically still work, although using it for other blogs may result in some unintended formatting behaviour.

I intend to keep updating this, so let me know if you encounter any bugs or issues, although you may need to bear with me in order to get fixes out. So far this is only designed to run on Windows, but I could potentially release versions for other operating systems in the future if there were demand (however such versions would be difficult to test from my end).

A .rar archive of the compiled program can be found in the releases section. Download the archive, extract it, and run. Do not remove the executable from the surrounding asset files.

Additional information can be found in the distribution README.txt.
