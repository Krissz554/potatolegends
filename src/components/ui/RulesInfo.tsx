import React from 'react'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'
import { Sparkles, Crown, Star } from 'lucide-react'

export const CollectionRulesInfo: React.FC = () => {
  return (
    <Card className="mb-4">
      <CardHeader>
        <CardTitle className="text-lg flex items-center gap-2">
          <Star className="h-5 w-5" />
          Collection & Deck Building Rules
        </CardTitle>
      </CardHeader>
      <CardContent className="space-y-4">
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          {/* Collection Rules */}
          <div>
            <h4 className="font-semibold mb-2 text-blue-600">Collection Limits</h4>
            <div className="space-y-2 text-sm">
              <div className="flex items-center justify-between">
                <div className="flex items-center gap-2">
                  <Sparkles className="h-4 w-4 text-purple-500" />
                  <span>Exotic cards</span>
                </div>
                <Badge variant="outline">1 copy max</Badge>
              </div>
              <div className="flex items-center justify-between">
                <div className="flex items-center gap-2">
                  <Crown className="h-4 w-4 text-yellow-500" />
                  <span>Legendary cards</span>
                </div>
                <Badge variant="outline">2 copies max</Badge>
              </div>
              <div className="flex items-center justify-between">
                <div className="flex items-center gap-2">
                  <Star className="h-4 w-4 text-gray-500" />
                  <span>Regular cards</span>
                </div>
                <Badge variant="outline">4 copies max</Badge>
              </div>
            </div>
          </div>

          {/* Deck Rules */}
          <div>
            <h4 className="font-semibold mb-2 text-green-600">Deck Building Limits</h4>
            <div className="space-y-2 text-sm">
              <div className="flex items-center justify-between">
                <span>Deck size</span>
                <Badge>Exactly 30 cards</Badge>
              </div>
              <div className="flex items-center justify-between">
                <div className="flex items-center gap-2">
                  <Sparkles className="h-4 w-4 text-purple-500" />
                  <span>Exotic cards</span>
                </div>
                <Badge variant="outline">1 copy max</Badge>
              </div>
              <div className="flex items-center justify-between">
                <div className="flex items-center gap-2">
                  <Crown className="h-4 w-4 text-yellow-500" />
                  <span>Legendary cards</span>
                </div>
                <Badge variant="outline">2 copies max</Badge>
              </div>
              <div className="flex items-center justify-between">
                <div className="flex items-center gap-2">
                  <Star className="h-4 w-4 text-gray-500" />
                  <span>Regular cards</span>
                </div>
                <Badge variant="outline">2 copies max</Badge>
              </div>
            </div>
          </div>
        </div>

        <div className="bg-blue-50 border border-blue-200 rounded-lg p-3">
          <p className="text-sm text-blue-800">
            <strong>Why these rules?</strong> You can collect more cards than you can use in decks, 
            encouraging collecting, trading, and experimentation while keeping competitive play balanced.
          </p>
        </div>
      </CardContent>
    </Card>
  )
}