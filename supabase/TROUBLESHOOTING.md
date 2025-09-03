# Supabase Authentication Troubleshooting

## 🚨 Common Issues and Solutions

### "Email address is invalid" Error

This error typically comes from Supabase server-side configuration, not your code. Here's how to fix it:

#### 1. Check Email Confirmation Settings
1. Go to **Supabase Dashboard** → **Authentication** → **Settings**
2. Scroll to **Email Confirmation**
3. **Disable "Enable email confirmations"** (for testing)
4. Save settings and try again

#### 2. Configure Email Templates (If email confirmation is enabled)
1. Go to **Authentication** → **Email Templates**
2. Click **"Confirm signup"**
3. Make sure the template is valid and has proper redirect URL

#### 3. Check Domain Restrictions
1. Go to **Authentication** → **Settings**
2. Scroll to **Site URL**
3. Make sure your site URL is correct: `http://localhost:5173` (for development)
4. Check **Additional redirect URLs** includes your domain

#### 4. SMTP Configuration (If using custom SMTP)
1. Go to **Project Settings** → **Auth**
2. Check if custom SMTP is configured correctly
3. For testing, you can use the default Supabase SMTP

### Quick Fix for Development

**Recommended for testing:**
1. **Disable email confirmations** temporarily
2. **Set Site URL** to `http://localhost:5173`
3. **Clear redirect URLs** or set to `http://localhost:5173/**`

## 🔧 Code-Level Debugging

The updated authentication code now includes:

### Enhanced Error Handling
- ✅ Better error messages for common issues
- ✅ Console logging for debugging
- ✅ Client-side email validation
- ✅ Automatic email trimming and lowercasing

### Debug Steps
1. **Open browser console** when testing authentication
2. **Look for console logs** starting with "Attempting to sign up/in"
3. **Check for Supabase errors** in the console
4. **Verify network requests** in Network tab

## 🚀 Testing the Fix

Try signing up with these test scenarios:

### Valid Emails (should work)
- `test@example.com`
- `user123@gmail.com`
- `potato.lover@domain.co`

### Invalid Emails (should show client-side error)
- `invalid-email` (missing @ and domain)
- `test@` (missing domain)
- `@domain.com` (missing username)

## 📋 Checklist for Working Authentication

- [ ] Email confirmations disabled (for testing)
- [ ] Site URL set to your development URL
- [ ] No domain restrictions in place
- [ ] Migration `002_basic_auth_only.sql` applied successfully
- [ ] Browser console shows no Supabase errors
- [ ] Network requests to Supabase are successful (200 status)

## 🆘 Still Not Working?

If you're still getting the "invalid email" error:

1. **Check Supabase project logs** in Dashboard → Logs
2. **Try a different email** (some emails might be blacklisted)
3. **Create a new test project** in Supabase to isolate the issue
4. **Check your Supabase plan** (some features require paid plans)

## 📞 Last Resort

If nothing works, the issue might be:
- **Supabase project configuration** that requires manual fixing
- **Regional restrictions** on your Supabase project
- **Account-level issues** with your Supabase account

Consider creating a new Supabase project with default settings for testing.