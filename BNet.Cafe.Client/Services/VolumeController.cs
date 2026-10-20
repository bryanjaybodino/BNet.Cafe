using System;
using System.Runtime.InteropServices;

namespace BNet.Cafe.Client.Services
{
    public static class VolumeController
    {
        [ComImport]
        [Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")]
        private class MMDeviceEnumerator { }

        [Guid("A95664D2-9614-4F35-A746-DE8DB63617E6"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IMMDeviceEnumerator
        {
            int EnumAudioEndpoints(int dataFlow, int dwState, out IntPtr ppDevices);
            int GetDefaultAudioEndpoint(int dataFlow, int role, out IMMDevice ppEndpoint);
            int GetDevice(string pwstrId, out IMMDevice ppDevice);
            int RegisterEndpointNotificationCallback(IntPtr pClient);
            int UnregisterEndpointNotificationCallback(IntPtr pClient);
        }

        [Guid("D666063F-1587-4E43-81F1-B948E807363F"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IMMDevice
        {
            int Activate(ref Guid iid, int dwClsCtx, IntPtr pActivationParams, out IntPtr ppInterface);
            int OpenPropertyStore(int stgmAccess, out IntPtr ppProperties);
            int GetId(out IntPtr ppstrId);
            int GetState(out int pdwState);
        }

        [Guid("5CDF2C82-841e-4546-9722-0CF74078229A"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IAudioEndpointVolume
        {
            int RegisterControlChangeNotify(IntPtr pNotify);
            int UnregisterControlChangeNotify(IntPtr pNotify);
            int GetChannelCount(out int pnChannelCount);
            int SetMasterVolumeLevel(float fLevelDB, ref Guid pguidEventContext);
            int SetMasterVolumeLevelScalar(float fLevel, ref Guid pguidEventContext);
            int GetMasterVolumeLevel(out float pfLevelDB);
            int GetMasterVolumeLevelScalar(out float pfLevel);
            int SetChannelVolumeLevel(uint nChannel, float fLevelDB, ref Guid pguidEventContext);
            int SetChannelVolumeLevelScalar(uint nChannel, float fLevel, ref Guid pguidEventContext);
            int GetChannelVolumeLevel(uint nChannel, out float pfLevelDB);
            int GetChannelVolumeLevelScalar(uint nChannel, out float pfLevel);
            int SetMute(bool bMute, ref Guid pguidEventContext);
            int GetMute(out bool pbMute);
            int GetVolumeChannelCount(out int pdwChannelCount);
            int GetVolumeRange(out float pflVolumeMin, out float pflVolumeMax, out float pflVolumeIncrement);
        }

        private static float _originalVolume = 0.5f;
        private static bool _originalMute = false;
        private static bool _isVolumeSaved = false;

        public static void Mute()
        {
            try
            {
                var device = GetDefaultAudioDevice();
                if (device == null) return;

                Guid iid = typeof(IAudioEndpointVolume).GUID;
                device.Activate(ref iid, 1, IntPtr.Zero, out IntPtr ptr);
                if (ptr != IntPtr.Zero)
                {
                    var volume = (IAudioEndpointVolume)Marshal.GetTypedObjectForIUnknown(ptr, typeof(IAudioEndpointVolume));
                    Marshal.Release(ptr);

                    Guid guid = Guid.Empty;

                    // Safely store user's current settings only if not already saved
                    if (!_isVolumeSaved)
                    {
                        volume.GetMasterVolumeLevelScalar(out _originalVolume);
                        volume.GetMute(out _originalMute);
                        _isVolumeSaved = true;
                    }

                    // Force volume to 0 and explicitly engage Windows mute
                    volume.SetMasterVolumeLevelScalar(0.0f, ref guid);
                    volume.SetMute(true, ref guid);
                }
            }
            catch { }
        }

        public static void Restore()
        {
            try
            {
                if (!_isVolumeSaved) return;

                var device = GetDefaultAudioDevice();
                if (device == null) return;

                Guid iid = typeof(IAudioEndpointVolume).GUID;
                device.Activate(ref iid, 1, IntPtr.Zero, out IntPtr ptr);
                if (ptr != IntPtr.Zero)
                {
                    var volume = (IAudioEndpointVolume)Marshal.GetTypedObjectForIUnknown(ptr, typeof(IAudioEndpointVolume));
                    Marshal.Release(ptr);

                    Guid guid = Guid.Empty;

                    // Restore previous volume and user's original mute state
                    volume.SetMasterVolumeLevelScalar(_originalVolume, ref guid);
                    volume.SetMute(_originalMute, ref guid);
                    _isVolumeSaved = false;
                }
            }
            catch { }
        }

        /// <summary>
        /// Sets the master volume level to 100% (1.0f) and unmutes the device.
        /// </summary>
        public static void SetMaxVolume()
        {
            try
            {
                var device = GetDefaultAudioDevice();
                if (device == null) return;

                Guid iid = typeof(IAudioEndpointVolume).GUID;
                device.Activate(ref iid, 1, IntPtr.Zero, out IntPtr ptr);
                if (ptr != IntPtr.Zero)
                {
                    var volume = (IAudioEndpointVolume)Marshal.GetTypedObjectForIUnknown(ptr, typeof(IAudioEndpointVolume));
                    Marshal.Release(ptr);

                    Guid guid = Guid.Empty;

                    // Set scalar to 1.0 (100%) and unmute
                    volume.SetMasterVolumeLevelScalar(1.0f, ref guid);
                    volume.SetMute(false, ref guid);
                }
            }
            catch { }
        }

        private static IMMDevice GetDefaultAudioDevice()
        {
            try
            {
                var enumerator = (IMMDeviceEnumerator)Activator.CreateInstance(typeof(MMDeviceEnumerator));
                enumerator.GetDefaultAudioEndpoint(0, 1, out IMMDevice device);
                return device;
            }
            catch
            {
                return null;
            }
        }
    }
}