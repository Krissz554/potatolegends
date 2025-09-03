import { useState, useEffect } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { User, Save, X } from "lucide-react";
import { toast } from "sonner";
import { useAuth } from "@/contexts/AuthContext";
import { supabase } from "@/lib/supabase";

interface SettingsModalProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

export const SettingsModal = ({ open, onOpenChange }: SettingsModalProps) => {
  const { user } = useAuth();
  const [username, setUsername] = useState("");
  const [originalUsername, setOriginalUsername] = useState("");
  const [isLoading, setIsLoading] = useState(false);
  const [isSaving, setIsSaving] = useState(false);

  // Load current username when modal opens
  useEffect(() => {
    if (open && user) {
      loadUserProfile();
    }
  }, [open, user]);

  const loadUserProfile = async () => {
    if (!user) return;
    
    setIsLoading(true);
    try {
      const { data: profile, error } = await supabase
        .from('user_profiles')
        .select('username')
        .eq('id', user.id)
        .single();

      if (error && error.code !== 'PGRST116') { // PGRST116 = no rows returned
        console.error('Error loading profile:', error);
        toast.error('Failed to load profile');
        return;
      }

      const currentUsername = profile?.username || '';
      setUsername(currentUsername);
      setOriginalUsername(currentUsername);
    } catch (error) {
      console.error('Unexpected error loading profile:', error);
      toast.error('Failed to load profile');
    } finally {
      setIsLoading(false);
    }
  };

  const saveUsername = async () => {
    if (!user) return;
    
    // Validate username
    const trimmedUsername = username.trim();
    if (trimmedUsername.length < 3) {
      toast.error('Username must be at least 3 characters long');
      return;
    }
    
    if (trimmedUsername.length > 20) {
      toast.error('Username must be 20 characters or less');
      return;
    }

    if (!/^[a-zA-Z0-9_-]+$/.test(trimmedUsername)) {
      toast.error('Username can only contain letters, numbers, hyphens, and underscores');
      return;
    }

    if (trimmedUsername === originalUsername) {
      toast.info('No changes to save');
      return;
    }

    setIsSaving(true);
    try {
      // Check if username is already taken
      const { data: existingUser, error: checkError } = await supabase
        .from('user_profiles')
        .select('id')
        .eq('username', trimmedUsername)
        .neq('id', user.id)
        .limit(1);

      if (checkError) {
        console.error('Error checking username:', checkError);
        toast.error('Failed to validate username');
        return;
      }

      if (existingUser && existingUser.length > 0) {
        toast.error('Username is already taken');
        return;
      }

      // Update or insert username
      const { error: upsertError } = await supabase
        .from('user_profiles')
        .upsert({
          id: user.id,
          username: trimmedUsername,
          updated_at: new Date().toISOString()
        }, {
          onConflict: 'id'
        });

      if (upsertError) {
        console.error('Error saving username:', upsertError);
        toast.error('Failed to save username');
        return;
      }

      setOriginalUsername(trimmedUsername);
      toast.success('Username saved successfully!');
    } catch (error) {
      console.error('Unexpected error saving username:', error);
      toast.error('Failed to save username');
    } finally {
      setIsSaving(false);
    }
  };

  const handleClose = () => {
    // Reset to original value if not saved
    setUsername(originalUsername);
    onOpenChange(false);
  };

  const hasChanges = username.trim() !== originalUsername;

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2">
            <User className="w-5 h-5" />
            Settings
          </DialogTitle>
        </DialogHeader>

        <div className="space-y-6">
          {/* Username Section */}
          <Card>
            <CardHeader className="pb-3">
              <CardTitle className="text-lg flex items-center gap-2">
                <User className="w-4 h-4" />
                Username
              </CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="space-y-2">
                <Label htmlFor="username">Display Name</Label>
                <Input
                  id="username"
                  value={username}
                  onChange={(e) => setUsername(e.target.value)}
                  placeholder="Enter your username"
                  disabled={isLoading || isSaving}
                  maxLength={20}
                />
                <p className="text-xs text-muted-foreground">
                  3-20 characters, letters, numbers, hyphens, and underscores only
                </p>
              </div>

              <div className="flex gap-2">
                <Button
                  onClick={saveUsername}
                  disabled={!hasChanges || isLoading || isSaving}
                  className="flex-1"
                >
                  {isSaving ? (
                    <>
                      <div className="w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin mr-2" />
                      Saving...
                    </>
                  ) : (
                    <>
                      <Save className="w-4 h-4 mr-2" />
                      Save
                    </>
                  )}
                </Button>
                
                {hasChanges && (
                  <Button
                    onClick={() => setUsername(originalUsername)}
                    variant="outline"
                    disabled={isLoading || isSaving}
                  >
                    <X className="w-4 h-4" />
                  </Button>
                )}
              </div>
            </CardContent>
          </Card>

          {/* Future settings sections can be added here */}
        </div>

        <div className="flex justify-end">
          <Button variant="outline" onClick={handleClose}>
            Close
          </Button>
        </div>
      </DialogContent>
    </Dialog>
  );
};