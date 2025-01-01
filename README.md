This project was inspired by [nevbriv's VTOL VR telemetry mod](https://github.com/nevbriv/vtolvr-telemetry), but has been rewritten to support modern .NET features and Yaw2 chair integration.

# VTOL VR Telemetry Mod

### Description
This mod is designed for **VTOL VR** to gather telemetry data and send it to a **Yaw2 motion chair** or other external devices. It enables real-time data collection, allowing enhanced immersion and motion tracking integration.

---

### Features
- Collects telemetry data from VTOL VR, such as speed, position, orientation, and more.
- Supports exporting telemetry data in **JSON** or **CSV** format.
- Sends data over **UDP** to external devices.
- Configurable options for:
  - Data export formats (JSON, CSV)
  - Receiver IP and port for UDP communication
- Works seamlessly with Yaw2 motion chairs.

---

### Requirements
- **VTOL VR** (latest version)
- **Yaw2 motion chair** (optional for integration)
- .NET Framework 4.x (for building the mod)

---

### Installation
1. Clone the repository or download the ZIP file.
   ```bash
   git clone https://github.com/yourusername/vtolvrTelemetry-Mod.git
