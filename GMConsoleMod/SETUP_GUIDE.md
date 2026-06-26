# MelonLoader Setup Guide

## Vấn đề hiện tại
MelonLoader folder trống - cần cài đặt đúng cách.

## Cách cài đặt đúng

### Tùy chọn 1: Chạy lại installer
1. Tải MelonLoader từ: https://github.com/LavaGang/MelonLoader/releases
2. Tìm file `MelonLoader.Installer.exe` hoặc `MelonLoader.x86.x64.exe`
3. **CHẠY AS ADMINISTRATOR**
4. Installer sẽ tự động detect game và cài vào thư mục game
5. Sau khi xong, thư mục game sẽ có:
   ```
   MelonLoader/
   ├── MelonLoader.dll
   ├── 0Harmony.dll
   ├── MelonLoader.runtimeconfig.json
   └── ...
   Mods/
   ```

### Tùy chọn 2: Copy thủ công từ nơi khác
Nếu MelonLoader đã được cài ở game khác, copy thư mục `MelonLoader/` sang.

### Kiểm tra sau cài đặt
```powershell
Get-ChildItem "C:\Users\Admin\Documents\GitHub\thien_menh_lac_hong\MelonLoader"
```

Sau khi có DLLs, chạy:
```powershell
cd GMConsoleMod
dotnet build
```
