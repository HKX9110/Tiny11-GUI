# Support

Tiny11 GUI is a community-maintained beta project. There is no guaranteed response time, but complete and sanitized reports are much easier to investigate.

## Where to ask

- Use [GitHub Discussions](https://github.com/brk4lp/Tiny11-GUI/discussions) for setup questions, compatibility results, and general help.
- Use the structured [issue forms](https://github.com/brk4lp/Tiny11-GUI/issues/new/choose) for reproducible bugs and focused feature requests.
- Follow [SECURITY.md](SECURITY.md) for vulnerabilities; do not disclose them in a public issue or discussion.

## Information to include

- Tiny11 GUI version or commit;
- host Windows edition, version, and build;
- whether the host is stock or already trimmed/debloated;
- source ISO Windows build, language, edition, and WIM/ESD format;
- preset and any manually changed options;
- the exact failed stage and a short sanitized log excerpt.

Remove product keys, usernames, machine names, personal paths, and other private information before posting. Generated Windows behavior can vary by image and update level, so a reproducible virtual-machine test is more useful than an assumption based only on the resulting ISO size.

## Build failures reported in 1.2.2

Hyper-V error `-2146498548` (`0x800F080C`) means the requested feature is unknown. The Hyper-V role is [not supported on Windows Home](https://learn.microsoft.com/en-us/windows-server/virtualization/hyper-v/get-started/install-hyper-v). Version 1.2.3 queries the selected image with DISM before attempting removal and skips only that specific missing-feature result. Other query and removal failures still stop the build with native diagnostics. This check uses the image's feature metadata, not a localized edition-name comparison.

If the build reaches **Removing OneDrive** and fails with `Access to the path is denied`, the setup executable in the mounted image may be protected. Version 1.2.3 uses file deletion that removes a leaf symbolic link without deleting its target. On failure, it changes ownership, permissions, and attributes only on that offline setup file, using `icacls /L` and `attrib /L` to avoid following a symbolic link. It uses a numeric SID so it does not depend on Windows language. Directories and parent reparse points are rejected; permissions are never changed recursively. A failed retry remains fatal and reports the target path and native error.

Mounted WIM files can have a reparse tag (`0x80000008`) without being symbolic links. On the tested English Windows 11 25H2 image, OneDrive setup granted administrators only read/execute access and `icacls /setowner` failed with exit code 5. `takeown /F <file> /A`, followed by a file-specific permission grant and deletion, succeeded on a separate writable WIM copy. The fallback permits only regular files and verified WIM/WOF tags; it refuses other reparse types. This verifies OneDrive file removal, not a complete ISO build or Windows installation.

The repeated DISM failures at Hyper-V, capability inventory, driver inventory, and component-store cleanup have a likely common cause in 1.2.2: the script performs those operations while its offline registry hives are still loaded. Windows error 32 means a [sharing violation](https://learn.microsoft.com/en-us/windows/win32/debug/system-error-codes--0-499-). Version 1.2.3 unloads and checks all three hives before deep cleanup. The inventory commands already request English output, so a Chinese ISO does not explain that ordering defect. Confirm the actual locked file in `%WINDIR%\Logs\DISM\dism.log` if the error persists.

CORE error `-2146498555` (`0x800F0805`, `CBS_E_INVALID_PACKAGE`) is separate. The updated script warns with the full package identity and DISM output and continues; that package may remain in the image. This does not repair an invalid package or establish why DISM rejected it. Other errors still stop the build. Include the DISM log and, if available, the mounted image's `Windows\Logs\CBS\CBS.log` when reporting repeat failures.

## Microsoft account setup and answer files

Enable **Bypass Microsoft Account** and **Skip Network Connection** in the GUI. A custom XML file is unnecessary: the generator creates a namespaced answer file for the selected image architecture. For a custom x64 file, use [docs/autounattend.xml](docs/autounattend.xml); for ARM64, change `processorArchitecture="amd64"` to `processorArchitecture="arm64"`. Keep `language="neutral"` and the XML namespace unchanged for a Chinese ISO. This template does not set the installation language, partition disks, create an account, or embed credentials.

The template sets Microsoft's documented [`HideOnlineAccountScreens`](https://learn.microsoft.com/en-us/windows-hardware/customize/desktop/unattend/microsoft-windows-shell-setup-oobe-hideonlineaccountscreens) option during `oobeSystem`. Test the complete OOBE flow in a VM with the exact Windows edition/build and network conditions you intend to use; a successful XML merge is not proof that every Windows build will complete local-account setup.

An empty file or `<unattend />` without the required namespace is not a valid template. The updated generator validates the root and namespace before merging, preserves XML-declared encoding while loading, and clears the read-only attribute on the destination before saving. It does not change permissions on the original custom file. If access is still denied, check the destination folder's write permissions and the exact path in the error log.
