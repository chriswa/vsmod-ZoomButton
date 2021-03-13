# vsmod-ZoomButton

A mod for Vintage Story which allows the player to "zoom".

Have you ever changed your Field of View graphics setting in your client to zoom in? Yup, that's pretty much all this mod does: it gives you a HotKey to temporarily drop your Field of View to the legal minimum of 20 and halves your mouse sensitivity.

## Usage

Hold down the HotKey (default Z) to zoom. Release to zoom back out.

## Customization

After the mod has been loaded once by the game, it will create a config file at `%appdata%\VintagestoryData\ModConfig\zoombutton.json`, which can be used to customize its features. The defaults settings are listed below. To restore defaults, you can simply delete the file and relaunch the game.

```
{
  "zoomInTimeSec": 0.5,
  "zoomOutTimeSec": 0.1,
  "fieldOfView": 20,
  "mouseSensitivityFactor": 0.5,
  "changeMouseSmoothing": false,
  "mouseSmoothing": 0.0
}
```

For example, if you want smoother (aka cinematic or drunken) mouse controls while zooming, set "changeMouseSmoothing" to `false`.
