VTOL VR Telemetry Mod for Yaw Devices

Description

This mod is designed for VTOL VR to gather telemetry data and send it to a Yaw2 motion chair or other external devices. It enables real-time data collection, allowing enhanced immersion and motion tracking integration.

###Features

 * Collects telemetry data from VTOL VR, including:

  * Pitch, Roll, and (optionally) Yaw

  * Airspeed and vertical speed

  * Angle of Attack (AoA)

* Sends telemetry data over UDP to external devices.

* Configurable receiver IP and port for UDP communication.

* Optimized for Yaw2 motion chairs.

###Requirements

* VTOL VR Mod Loader (latest version)

* Yaw2 motion chair (optional for integration)

* .NET Framework 4.x (for building the mod)

###Installation

1. Clone the repository or download the ZIP file.

    git clone https://github.com/yourusername/vtolvrTelemetry-Mod.git

2. Build the mod using the required .NET tools.

3. Install the mod into VTOL VR's mod loader.

4. Configure UDP settings as needed.

5. Launch VTOL VR and verify telemetry data is being sent to the external receiver.

###Configuration

* Yaw Handling: Yaw is currently disabled due to unpredictable behavior in VTOL VR.

* Motion Sensitivity: Roll and pitch can be fine-tuned for more natural motion feedback.

###Troubleshooting

* No telemetry data detected? Ensure the mod is properly installed and running.

* Chair movements feel reversed? Adjust inversion settings for roll/pitch as needed.

* Yaw movement issues? Consider enabling yaw if needed, but be aware of potential misalignment with VTOL VR's re-centering mechanics.

###Future Plans

* Improve yaw integration for better synchronization.

* Add user-configurable motion sensitivity settings.

* Expand compatibility for additional motion systems.

