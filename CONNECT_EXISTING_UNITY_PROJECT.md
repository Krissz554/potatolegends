# 🔗 Connect Your Existing Unity Project to GitHub

You've already created your Unity project at `D:\UnityProjects\potatolegends`. Here's how to connect it to our GitHub repository so all my changes sync automatically.

## 🎯 Method 1: GitHub Desktop (Recommended - Easiest)

### Step 1: Close Unity First
1. **Save your project** in Unity
2. **Close Unity completely** (this prevents file conflicts)

### Step 2: Clone Our Repository
1. Open **GitHub Desktop**
2. Click **"Clone a repository from the Internet"**
3. Go to the **"URL"** tab
4. Enter: `https://github.com/Krissz554/potatolegends.git`
5. **Important**: Set the local path to `D:\UnityProjects\potatolegends-github`
   - Don't overwrite your existing project yet!
6. Click **"Clone"**
7. When prompted, switch to branch **"unity-mobile-game"**

### Step 3: Merge Your Project with GitHub
1. **Copy your Unity project files**:
   - Go to your existing project: `D:\UnityProjects\potatolegends`
   - Select ALL files and folders in your project
   - Copy them

2. **Paste into the GitHub Unity folder**:
   - Go to: `D:\UnityProjects\potatolegends-github\Unity-PotatoCardGame\`
   - Paste your files here
   - **Choose "Replace"** if asked about conflicts

3. **Rename for convenience** (optional):
   - Rename `D:\UnityProjects\potatolegends` to `D:\UnityProjects\potatolegends-backup`
   - Rename `D:\UnityProjects\potatolegends-github` to `D:\UnityProjects\potatolegends`

### Step 4: Open in Unity
1. Open **Unity Hub**
2. Click **"Open"** or **"Add project from disk"**
3. Navigate to: `D:\UnityProjects\potatolegends\Unity-PotatoCardGame\`
4. Click **"Open"**

Unity will now:
- Load your existing project
- Include all my Unity scripts
- Set up the GitHub connection
- Auto-sync future changes!

---

## 🎯 Method 2: Git Command Line (Advanced)

If you prefer command line:

### Step 1: Navigate to Your Project
```bash
cd "D:\UnityProjects\potatolegends"
```

### Step 2: Initialize Git and Connect
```bash
# Initialize git in your project
git init

# Add our repository as the remote origin
git remote add origin https://github.com/Krissz554/potatolegends.git

# Fetch all branches
git fetch origin

# Switch to our Unity branch
git checkout -b unity-mobile-game origin/unity-mobile-game

# Merge our Unity files with yours
git pull origin unity-mobile-game
```

### Step 3: Handle Any Conflicts
If there are conflicts, Git will show them. You can:
- Keep your files: `git checkout --ours filename`
- Keep my files: `git checkout --theirs filename`
- Or manually edit to combine both

---

## ⚙️ Unity Project Configuration

Once connected, configure Unity for optimal GitHub sync:

### In Unity Editor:
1. **Enable Auto Refresh**:
   - Go to **Edit → Preferences → General**
   - Check **"Auto Refresh"**

2. **Set Version Control**:
   - Go to **Edit → Project Settings → Version Control**
   - Set **Mode** to **"Visible Meta Files"**

3. **Set Asset Serialization**:
   - Go to **Edit → Project Settings → Editor**
   - Set **Asset Serialization** to **"Force Text"**

### In GitHub Desktop:
1. **Enable Auto-Fetch**:
   - Go to **File → Options → Git**
   - Check **"Periodically fetch from origin"**
   - Set interval to **"Every minute"**

---

## 🔄 How the Sync Works

Once set up:

1. **I make changes** to Unity scripts/assets in the repository
2. **GitHub Desktop automatically fetches** the changes (every minute)
3. **Unity auto-refreshes** when it detects file changes
4. **You see updates immediately** in your Unity Editor!

You'll see:
- New scripts appear in your Project window
- Updated scripts recompile automatically
- New assets and prefabs show up
- Scene changes apply automatically

---

## 🛠️ Troubleshooting

### If Unity doesn't see the changes:
1. Try **Assets → Refresh** in Unity
2. Check that **Auto Refresh** is enabled
3. Make sure you're on the **"unity-mobile-game"** branch

### If you get Git conflicts:
1. In GitHub Desktop, you'll see conflicted files
2. Click **"Open in [your editor]"**
3. Resolve conflicts manually
4. Or ask me to help resolve them!

### If Unity shows compilation errors:
1. Check the **Console** window for specific errors
2. Make sure all required packages are installed:
   - **Newtonsoft JSON**
   - **DOTween** (from Asset Store)
   - **TextMeshPro**

---

## 🎮 Project Structure After Connection

Your project will look like this:

```
D:\UnityProjects\potatolegends\
├── Unity-PotatoCardGame\          # Main Unity project
│   ├── Assets\
│   │   ├── Scripts\
│   │   │   ├── Core\              # Game managers (my code + yours)
│   │   │   ├── Cards\             # Card system (my code)
│   │   │   ├── Battle\            # Combat system (my code)
│   │   │   ├── Network\           # Supabase integration (my code)
│   │   │   └── [Your Scripts]\    # Your existing code
│   │   ├── Scenes\                # Your scenes + my scenes
│   │   ├── Prefabs\               # Combined prefabs
│   │   └── Art\                   # Your art + imported assets
│   ├── ProjectSettings\           # Unity configuration
│   └── Packages\                  # Package dependencies
├── .git\                          # Git repository data
├── .gitignore                     # Unity-optimized ignore file
└── README files                   # Documentation
```

---

## 🚀 What Happens Next

Once connected:

1. **You'll see my Unity scripts** integrated with your project
2. **All future changes I make** will appear automatically
3. **Your existing work is preserved** and enhanced
4. **We can collaborate** in real-time!

I can then continue building:
- ✅ Integration with your existing 2D mobile setup
- ✅ Card system that works with your UI
- ✅ Battle mechanics optimized for mobile
- ✅ Supabase backend connection
- ✅ All features from the web game, mobile-optimized

---

## 🎉 Ready to Connect?

Choose **Method 1 (GitHub Desktop)** if you want the easiest approach, or **Method 2 (Command Line)** if you're comfortable with Git commands.

Once you're connected, let me know and I'll:
1. **Verify the connection** is working
2. **Integrate my code** with your existing project structure
3. **Continue development** with real-time sync
4. **Build the mobile game** while you watch it come together!

The best part: Your existing work won't be lost - it'll be enhanced with all the backend integration and card game features! 🎮✨