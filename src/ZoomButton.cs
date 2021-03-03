using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace ZoomButton
{
    public class ZoomButton : ModSystem
    {
        private ICoreClientAPI capi;
        private int previousFieldOfView = 80;
        private int previousMouseSensivity = 100;
        private bool isZooming = false;
        public override void StartClientSide(ICoreClientAPI api)
        {
            base.StartClientSide(api);
            capi = api;
            api.Input.RegisterHotKey("zoombutton", "Zoom (zoom)", GlKeys.Z, HotkeyType.CharacterControls);
            api.Input.SetHotKeyHandler("zoombutton", (KeyCombination comb) =>
            {
                api.Logger.Event("Zoom HotKey pressed!");

                if (isZooming) { return true; }
                isZooming = true;

                previousFieldOfView = api.Settings.Int["fieldOfView"];
                previousMouseSensivity = api.Settings.Int["mouseSensivity"];

                api.Event.RegisterCallback(OnTick, 10);

                incrementalZoom();

                return true;
            });

        }

        private void incrementalZoom()
        {
            // TODO: lerp over time
            capi.Settings.Int["fieldOfView"] = max(20, capi.Settings.Int["fieldOfView"] - 3);
            capi.Settings.Int["mouseSensivity"] = max(previousMouseSensivity / 2, capi.Settings.Int["mouseSensivity"] - 3);

        }

        private void OnTick(float dt)
        {
            bool isPressed = capi.Input.KeyboardKeyStateRaw[capi.Input.GetHotKeyByCode("zoombutton").CurrentMapping.KeyCode];
            if (isPressed)
            {
                incrementalZoom();

                capi.Event.RegisterCallback(OnTick, 10);
            }
            else
            {
                capi.Settings.Int["fieldOfView"] = previousFieldOfView;
                capi.Settings.Int["mouseSensivity"] = previousMouseSensivity;
                isZooming = false;
            }

        }

        private int max(int a, int b)
        {
            return a > b ? a : b;
        }
    }
}
