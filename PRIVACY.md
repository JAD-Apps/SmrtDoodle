# SmrtDoodle Privacy Policy

**Effective date:** 5 November 2026

**Scope:** This policy describes the SmrtDoodle desktop application for Windows. It does not describe Windows, Microsoft Store, model-hosting services, SmrtPad, or the JAD Apps website; those services have their own privacy practices.

## Artwork and on-device processing

SmrtDoodle opens and saves drawings on your device. AI image operations use the packaged ONNX models and CPU runtime on your device. The app does not send drawing pixels, selections, or model inputs to JAD Apps for cloud inference. It does not create a JAD Apps cloud copy or cloud backup of your drawings. You choose where drawing files are saved, and you can open and export saved work even when Store entitlement is unavailable or expired.

Copy and paste use the Windows clipboard. Other applications that can access the clipboard may be able to read content you copy. If you deliberately use the optional SmrtPad integration, its bridge can pass PNG bytes to SmrtPad on the same PC over a local named pipe. The compatibility handoff writes a temporary PNG to the specified path or to the Windows temporary `SmrtDoodle` folder and can notify SmrtPad of that path over a local named pipe. These transfers are local; they are not uploads to JAD Apps.

## Network connections

The app checks Microsoft Store for its app licence and uses Store purchase or restore flows when you choose those actions in About. The app reads licence information such as the Store product identifier, active or trial status, and expiry. It does not ask for or store your Microsoft account password or payment-card details. Microsoft Store may use cached licence information while the device is offline. Microsoft processes Store account, purchase, licence, and related service data under its own [Privacy Statement](https://www.microsoft.com/en-us/privacy/privacystatement). Windows and Microsoft Store may also make their own connections or collect diagnostics according to your Windows and Store settings. SmrtDoodle does not control those services.

Model files are used locally. The model-acquisition component is offline by default; a download is possible only when explicitly requested with download permission and when a cleared catalogue entry has a download URL. It downloads the listed model file, verifies its size and SHA-256, and writes it to the local destination supplied for acquisition. Permitted model hosts are `huggingface.co`, `cdn-lfs.huggingface.co`, `cas-bridge.xethub.hf.co`, `github.com`, `raw.githubusercontent.com`, and `objects.githubusercontent.com`. A host can see your IP address, the requested model URL, request time, and ordinary connection metadata. It does not receive your drawings or image pixels through this download flow. The host's own privacy policy governs its server logs.

There is no app analytics service or routine app telemetry upload in SmrtDoodle's application code. Crash and diagnostic information is written to local log files. The ONNX Runtime telemetry events are disabled. This does not mean that Microsoft Store, Windows, a model host, or another platform component makes no network requests or collects no service diagnostics.

## Information kept on your device

- **Drawings:** Files remain at the locations from which you open them or to which you save/export them. SmrtDoodle does not keep a separate online copy.
- **Recent-file information:** Where the recent-files component is used, it stores up to ten file paths in Windows local application settings, not copies of the drawings.
- **Logs:** Diagnostic logs are written beneath the Windows local application-data folder in `SmrtDoodle/Logs`. They can contain timestamps, calling component names, error messages, and exception details; an error may include a local file path. On startup, SmrtDoodle attempts to delete log files older than 14 days.
- **Models:** Models included with the app are installed locally. If you use the optional acquisition workflow, downloaded model files remain in its local destination. The model cache contains model files, not your drawings.
- **Temporary handoff files:** A compatibility handoff may leave a PNG in the Windows temporary `SmrtDoodle` folder. SmrtDoodle attempts to remove files there that are more than one hour old when it starts.

## Removing local information

Delete drawing files and exported copies from the folders where you saved them. Delete diagnostic files from `SmrtDoodle/Logs` under your Windows local application-data folder. Delete any temporary handoff PNGs from the Windows temporary `SmrtDoodle` folder, and delete a downloaded model directory from the destination you supplied to the acquisition workflow. To clear app-local settings, use Windows' reset or uninstall controls for SmrtDoodle. Clearing app data does not delete drawings saved elsewhere.

## External links and privacy questions

The About window links to the [JAD Apps website](https://jadapps.app/) and [JAD Apps Support](https://jadapps.app/support). Opening a link sends the usual browser request information to that site; information you submit in its support form is handled under that service's practices. The [JAD Apps website privacy notice](https://jadapps.app/privacy) describes website processing and does not replace this desktop-app policy.

For SmrtDoodle support, use the [JAD Apps Support contact form](https://jadapps.app/support). For privacy questions, you can also contact `privacy@jadapps.app` as listed in the JAD Apps website privacy notice. Microsoft Store and other service providers handle information they collect under their own policies.
