import React from 'react';
import { FantasyLayout } from '@/components/FantasyLayout';
import { FantasyDeckBuilder } from '@/components/FantasyDeckBuilder';

const DeckBuilderPage: React.FC = () => {
  return (
    <FantasyLayout showBattleButton={false}>
      <FantasyDeckBuilder />
    </FantasyLayout>
  );
};

export default DeckBuilderPage;