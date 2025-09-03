# 🔧 Fix Unity "Read Only" Error

This error happens when Git changes file permissions during the sync. Here's how to fix it:

## 🚀 **Quick Fix Methods**

### **Method 1: File Properties (Easiest)**
1. **Right-click** on your project folder: `D:\UnityProjects\potatolegends`
2. Select **"Properties"**
3. **Uncheck** "Read-only" if it's checked
4. Click **"Apply"**
5. Choose **"Apply to all subfolders and files"**
6. Click **"OK"**
7. **Try opening in Unity again**

### **Method 2: Command Prompt (If Method 1 doesn't work)**
1. **Open Command Prompt as Administrator**
2. Run these commands:
```cmd
cd "D:\UnityProjects\potatolegends"
attrib -R /S /D *
icacls . /grant Everyone:F /T
```
3. **Try opening in Unity again**

### **Method 3: PowerShell (Alternative)**
1. **Open PowerShell as Administrator**
2. Run:
```powershell
cd "D:\UnityProjects\potatolegends"
Get-ChildItem -Recurse | ForEach-Object { $_.IsReadOnly = $false }
```
3. **Try opening in Unity again**

## 🔄 **If Still Not Working**

### **Method 4: Fresh Copy**
1. **Close Unity Hub completely**
2. **Rename** your current folder to `potatolegends-backup`
3. **Clone fresh** from GitHub:
   ```cmd
   cd "D:\UnityProjects"
   git clone https://github.com/Krissz554/potatolegends.git
   cd potatolegends
   git checkout unity-mobile-game
   ```
4. **Copy your personal files** from the backup if needed
5. **Open in Unity Hub**

### **Method 5: Unity Hub Reset**
1. **Open Unity Hub**
2. **Remove** the project from the list (don't delete files)
3. **Add** the project again:
   - Click "Open" or "Add project from disk"
   - Select `D:\UnityProjects\potatolegends`
4. **Unity should recognize it** as a valid project now

## 🛡️ **Prevent Future Issues**

### **Git Configuration**
Add this to your Git config to prevent permission issues:
```cmd
cd "D:\UnityProjects\potatolegends"
git config core.filemode false
git config core.autocrlf true
```

### **Windows Defender/Antivirus**
1. **Add exception** for your Unity projects folder
2. **Exclude** `D:\UnityProjects\` from real-time scanning
3. This prevents file locking during Git operations

## 🎯 **Root Cause**

This happens because:
- **Git on Windows** can change file permissions during sync
- **Unity requires** full read/write access to project files
- **Windows file system** sometimes locks files during updates
- **Antivirus software** might interfere with file access

## ⚡ **Quick Test**

After trying any method:
1. **Navigate** to `D:\UnityProjects\potatolegends`
2. **Try creating** a new text file in the folder
3. **If you can create files** → Unity should work
4. **If you can't** → Try the next method

## 🆘 **Emergency Solution**

If nothing else works:
1. **Create a new folder**: `D:\UnityProjects\potatolegends-new`
2. **Copy all files** manually from the GitHub folder
3. **Open the new folder** in Unity Hub
4. **This bypasses** any permission issues

---

**Most likely fix**: Method 1 (Properties → Uncheck Read-only) should solve it immediately! 🎯