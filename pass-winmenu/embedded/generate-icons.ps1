
# Percentages below are based on the default icon size of notification area
# icons, which is 16x16. Based on the scaling level used by Windows, the
# appropriate icon size is selected. The larger sizes may also be used by
# Windows Explorer.
$resolutions = 
	"16", # 100%
	"20", # 125%
	"24", # 150%
	"28", # 175%
	"32", # 200%
	"36", # 225%
	"40", # 250%
	"48", # 300%
	"64" # 400%

$iconstyles = @{
	plain = "key";
	ahead = "key;orb-green;arrow-up";
	behind = "key;orb-blue;arrow-down";
	diverged = "key;orb-yellow";
}

inkscape -w 28 -h 28 -C --export-id=key --export-id-only --export-filename=28.png .\icon.svg


magick .\pass-winmenu-16x16.png .\pass-winmenu-32x32.png .\pass-winmenu-48x48.png pass-winmenu.ico
magick .\pass-winmenu-ahead-16x16.png .\pass-winmenu-ahead-32x32.png .\pass-winmenu-ahead-48x48.png pass-winmenu-ahead.ico
magick .\pass-winmenu-behind-16x16.png .\pass-winmenu-behind-32x32.png .\pass-winmenu-behind-48x48.png pass-winmenu-behind.ico
magick .\pass-winmenu-diverged-16x16.png .\pass-winmenu-diverged-32x32.png .\pass-winmenu-diverged-48x48.png pass-winmenu-diverged.ico
