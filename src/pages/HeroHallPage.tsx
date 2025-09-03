import React from 'react';
import { FantasyLayout } from '@/components/FantasyLayout';
import HeroHall from '@/components/HeroHall';

const HeroHallPage: React.FC = () => {
  return (
    <FantasyLayout showBattleButton={false}>
      <div className="container mx-auto px-4 py-8">
        <div className="mb-8">
          <h1 className="text-4xl font-bold text-white mb-2" style={{ fontFamily: "'Cinzel', serif" }}>
            Hero Hall
          </h1>
          <p className="text-gray-300 text-lg">
            Choose your champion for battle
          </p>
        </div>
        
        <div className="bg-white/5 backdrop-blur-sm rounded-2xl border border-white/10 p-6">
          <HeroHall standalone={true} />
        </div>
      </div>
    </FantasyLayout>
  );
};

export default HeroHallPage;