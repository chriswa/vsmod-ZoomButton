using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace ZoomButton
{
    public class ZoomButton : ModSystem
    {
        private static string HOTKEY_CODE = "zoombutton";
        private static string FIELD_OF_VIEW_SETTING_NAME = "fieldOfView";
        private static string MOUSE_SENSITIVITY_SETTING_NAME = "mouseSensivity"; // n.b. typo in Vintage Story's code!
        private static float ZOOM_IN_TIME_S = 0.5F;
        private static float ZOOM_OUT_TIME_S = 0.1F;
        private static int TARGET_FIELD_OF_VIEW = 20;
        private static float TARGET_MOUSE_SENSITIVITY_FACTOR = 0.5F;
        private static int MAX_FRAMERATE_MS = 1000 / 90;

        private ICoreClientAPI capi;
        private int originalFieldOfView;
        private int originalMouseSensivity;
        private bool isZooming = false;
        private float zoomState = 0;

        public override void StartClientSide(ICoreClientAPI api)
        {
            base.StartClientSide(api);
            capi = api;
            api.Input.RegisterHotKey(HOTKEY_CODE, "Zoom in", GlKeys.Z, HotkeyType.CharacterControls);
            api.Event.RegisterGameTickListener(OnGameTick, MAX_FRAMERATE_MS);
        }

        private void OnGameTick(float dt)
        {
            bool isHotKeyPressed = capi.Input.KeyboardKeyStateRaw[capi.Input.GetHotKeyByCode(HOTKEY_CODE).CurrentMapping.KeyCode];
            
            // is the player currently zooming in?
            if (isHotKeyPressed && zoomState < 1)
            {
                // is this the start of a zoom?
                if (!isZooming)
                {
                    originalFieldOfView = capi.Settings.Int[FIELD_OF_VIEW_SETTING_NAME];
                    originalMouseSensivity = capi.Settings.Int[MOUSE_SENSITIVITY_SETTING_NAME];
                    isZooming = true;
                }

                // advance zoomState
                zoomState += dt / ZOOM_IN_TIME_S;
                if (zoomState > 1) {
                    zoomState = 1; // clamp to 0..1
                }
                UpdateSettings();
            }
            // is the player currently zooming out?
            else if (!isHotKeyPressed && zoomState > 0)
            {
                // advance zoomState
                zoomState -= dt / ZOOM_OUT_TIME_S;
                if (zoomState < 0) {
                    zoomState = 0; // clamp to 0..1
                    isZooming = false; // go back to initial state, which allows us to capture any player changes to settings
                }
                UpdateSettings();
            }
            // otherwise we are already zoomed all the way in or out: nothing to do
        }
        
        private void UpdateSettings()
        {
            // update fov and mouse sensitivity via linear interpolation based on zoomState 0..1
            capi.Settings.Int[FIELD_OF_VIEW_SETTING_NAME] = lerp(originalFieldOfView, TARGET_FIELD_OF_VIEW, zoomState);
            capi.Settings.Int[MOUSE_SENSITIVITY_SETTING_NAME] = lerp(originalMouseSensivity, originalMouseSensivity * TARGET_MOUSE_SENSITIVITY_FACTOR, zoomState);
        }

        private int lerp(float a, float b, float t)
        {
            return (int)System.Math.Round(a + (b - a) * t);
        }
    }
}
