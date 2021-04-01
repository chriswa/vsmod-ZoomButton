using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

[assembly: ModInfo("ZoomButton")]

namespace ZoomButton {
  public class ZoomButton : ModSystem {
    private static string HOTKEY_CODE = "zoombutton";
    private static string FIELD_OF_VIEW_SETTING_NAME = "fieldOfView";
    private static string MOUSE_SENSITIVITY_SETTING_NAME = "mouseSensivity"; // n.b. typo in Vintage Story's code!
    private static string MOUSE_SMOOTHING_SETTING_NAME = "mouseSmoothing";
    private static int MAX_FRAMERATE_MS = 1000 / 90;

    private ICoreClientAPI capi;
    private int originalFieldOfView;
    private int originalMouseSensivity;
    private int originalMouseSmoothing;
    private bool isZooming = false;
    private float zoomState = 0;
    private ModConfig config;
    private SquintOverlayRenderer renderer;

    public override void StartClientSide(ICoreClientAPI api) {
      capi = api;

      capi.Logger.Event("Hello from ZoomButton!");

      // load config file or write it with defaults
      config = api.LoadModConfig<ModConfig>("zoombutton.json");
      if (config == null) {
        config = new ModConfig();
        api.StoreModConfig(config, "zoombutton.json");
      }

      api.Input.RegisterHotKey(HOTKEY_CODE, "Zoom in", GlKeys.Z, HotkeyType.CharacterControls);
      api.Event.RegisterGameTickListener(OnGameTick, MAX_FRAMERATE_MS);

      renderer = new SquintOverlayRenderer(api);
    }

    private void OnGameTick(float dt) {
      bool isHotKeyPressed = capi.Input.KeyboardKeyState[capi.Input.GetHotKeyByCode(HOTKEY_CODE).CurrentMapping.KeyCode];

      // is the player currently zooming in?
      if (isHotKeyPressed && zoomState < 1) {
        // is this the start of a zoom?
        if (!isZooming) {
          originalFieldOfView = capi.Settings.Int[FIELD_OF_VIEW_SETTING_NAME];
          originalMouseSensivity = capi.Settings.Int[MOUSE_SENSITIVITY_SETTING_NAME];
          originalMouseSmoothing = capi.Settings.Int[MOUSE_SMOOTHING_SETTING_NAME];
          isZooming = true;
        }

        // advance zoomState
        zoomState += dt / config.zoomInTimeSec;
        if (zoomState > 1) {
          zoomState = 1; // clamp to 0..1
        }
        UpdateSettings();
      }
      // is the player currently zooming out?
      else if (!isHotKeyPressed && zoomState > 0) {
        // advance zoomState
        zoomState -= dt / config.zoomOutTimeSec;
        if (zoomState < 0) {
          zoomState = 0; // clamp to 0..1
          isZooming = false; // go back to initial state, which allows us to capture any player changes to settings
        }
        UpdateSettings();
      }
      renderer.PercentZoomed = zoomState;
      // otherwise we are already zoomed all the way in or out: nothing to do
    }

    private void UpdateSettings() {
      // update fov and mouse sensitivity via linear interpolation based on zoomState 0..1
      capi.Settings.Int[FIELD_OF_VIEW_SETTING_NAME] = lerp(originalFieldOfView, config.fieldOfView, zoomState);
      capi.Settings.Int[MOUSE_SENSITIVITY_SETTING_NAME] = lerp(originalMouseSensivity, originalMouseSensivity * config.mouseSensitivityFactor, zoomState);
      if (config.changeMouseSmoothing) {
        capi.Settings.Int[MOUSE_SMOOTHING_SETTING_NAME] = lerp(originalMouseSmoothing, config.mouseSmoothing, zoomState);
      }
    }

    private int lerp(float a, float b, float t) {
      return (int)System.Math.Round(a + (b - a) * t);
    }
  }
}
