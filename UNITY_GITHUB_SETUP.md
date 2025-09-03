# 🎮 Unity GitHub Connection Guide

Follow these steps to connect your Unity project directly to this GitHub repository so all changes I make will automatically sync to your Unity project.

## 📋 Prerequisites

1. **Unity Hub** installed on your computer
2. **Git** installed on your computer
3. **GitHub Desktop** (recommended) or Git command line knowledge
4. Your GitHub account logged in

## 🔗 Step 1: Clone the Repository

### Option A: Using GitHub Desktop (Recommended)
1. Open **GitHub Desktop**
2. Click **"Clone a repository from the Internet"**
3. Go to the **"URL"** tab
4. Enter your repository URL: `https://github.com/YOUR_USERNAME/YOUR_REPO_NAME.git`
5. Choose where to save it on your computer (e.g., `C:\Unity Projects\PotatoCardGame`)
6. Click **"Clone"**
7. When prompted about branches, select **"unity-mobile-game"** branch

### Option B: Using Git Command Line
```bash
git clone https://github.com/YOUR_USERNAME/YOUR_REPO_NAME.git
cd YOUR_REPO_NAME
git checkout unity-mobile-game
```

## 🎯 Step 2: Open Unity Project

1. Open **Unity Hub**
2. Click **"Open"** or **"Add project from disk"**
3. Navigate to the cloned repository folder
4. Select the **"Unity-PotatoCardGame"** folder inside your cloned repo
5. Click **"Open"**

Unity will automatically:
- Detect it's a Unity project
- Import all the scripts I've created
- Set up the project structure
- Create the necessary Unity files

## ⚙️ Step 3: Unity Project Setup

When Unity opens the project for the first time:

1. **Unity Version**: If prompted, use Unity **6000.2.2f1** (Unity 6) or newer
2. **Import Settings**: Accept all import settings
3. **Package Manager**: Unity will automatically install required packages

## 🔄 Step 4: Enable Auto-Sync

### In GitHub Desktop:
1. Go to **Repository → Repository Settings**
2. Enable **"Automatically fetch origin"** 
3. Set fetch interval to **"Every minute"** for real-time updates

### In Unity:
1. Go to **Edit → Preferences → External Tools**
2. Set **External Script Editor** to your preferred code editor
3. Enable **"Auto Refresh"** in **Edit → Preferences → General**

## 📱 Step 5: Configure for Mobile Development

1. **Switch Platform**:
   - Go to **File → Build Settings**
   - Select **Android** or **iOS**
   - Click **"Switch Platform"**

2. **Install Mobile Modules** (if not already installed):
   - Open **Unity Hub**
   - Go to **Installs**
   - Click the gear icon next to your Unity version
   - Select **"Add Modules"**
   - Install **Android Build Support** and/or **iOS Build Support**

## 🔧 Step 6: Required Packages

Unity should auto-install these, but if not, install via **Window → Package Manager**:

1. **Newtonsoft JSON** (for API communication)
2. **DOTween** (for animations) - Import from Asset Store
3. **TextMeshPro** (should be included)
4. **Unity UI** (should be included)

## 🎮 Step 7: Test the Setup

1. **Open the Main Scene**:
   - In Project window, navigate to `Assets/Scenes/`
   - Double-click on the main scene (I'll create this)

2. **Test Scripts**:
   - Check that all scripts compile without errors
   - Look for any missing references in the Inspector

3. **Test Git Sync**:
   - Make a small change to any file
   - Commit and push in GitHub Desktop
   - Watch for my updates to appear automatically

## 🚀 Step 8: Workflow

Once set up, here's how it works:

1. **I make changes** to the Unity project files in the repository
2. **GitHub Desktop automatically fetches** the changes
3. **Unity auto-refreshes** and applies the changes
4. **You see the updates** immediately in your Unity Editor!

## 🔍 Monitoring Changes

**In GitHub Desktop:**
- Watch the **"Changes"** tab for incoming updates
- You'll see exactly what I've modified
- All commits will show my progress

**In Unity:**
- Console will show compilation status
- Inspector will update with new components
- Scene view will reflect visual changes

## 🛠️ Troubleshooting

### If Unity doesn't recognize the project:
1. Make sure you selected the **"Unity-PotatoCardGame"** folder
2. Let Unity create the necessary project files
3. If issues persist, close Unity and reopen the project

### If scripts don't compile:
1. Check **Console** window for specific errors
2. Make sure all required packages are installed
3. Try **Assets → Reimport All**

### If Git sync isn't working:
1. Check your internet connection
2. Make sure you're on the **"unity-mobile-game"** branch
3. Try manually fetching in GitHub Desktop

### If you see merge conflicts:
1. **Don't panic!** This is normal
2. In GitHub Desktop, choose **"Open in [Editor]"**
3. Or ask me to help resolve conflicts

## 📞 Getting Help

If you run into any issues:
1. Check the **Unity Console** for error messages
2. Look at **GitHub Desktop** for sync status
3. Send me the error messages and I'll help fix them!

## 🎉 What's Next?

Once connected, you'll be able to:
- ✅ See all my Unity development in real-time
- ✅ Test the game as I build it
- ✅ Run it on your mobile device
- ✅ Provide feedback on features
- ✅ Watch the game come to life!

The magic happens automatically - every script, prefab, scene, and asset I create will appear in your Unity project instantly! 🚀