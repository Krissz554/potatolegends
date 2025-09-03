import React, { useState } from 'react';
import { FantasyLayout } from '@/components/FantasyLayout';
import { FantasyCollection } from '@/components/FantasyCollection';
import { UnlockAnimation } from '@/components/UnlockAnimation';
import { motion } from 'framer-motion';
import { Library, Sparkles } from 'lucide-react';

const CollectionPage: React.FC = () => {
  const [showUnlockAnimation, setShowUnlockAnimation] = useState(false);
  const [unlockAnimationData, setUnlockAnimationData] = useState<any>(null);

  const handleUnlock = (data: any) => {
    setUnlockAnimationData(data);
    setShowUnlockAnimation(true);
  };

  return (
    <FantasyLayout showBattleButton={false}>
      <div className="h-full flex flex-col">
        <FantasyCollection onUnlock={handleUnlock} />
      </div>

      {/* Unlock Animation */}
      {showUnlockAnimation && (
        <UnlockAnimation 
          isVisible={showUnlockAnimation}
          onComplete={() => {
            setShowUnlockAnimation(false);
            setUnlockAnimationData(null);
          }}
        />
      )}
    </FantasyLayout>
  );
};

export default CollectionPage;