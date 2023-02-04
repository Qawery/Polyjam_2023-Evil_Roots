using UnityEngine;

namespace Polyjam2023
{
    [CreateAssetMenu(menuName = "ScriptableObjects/TestCardLogic_3")]
    public class TestCardTemplateDescription3 : CardTemplate
    {
        public override string EffectDescription => "Test card 3 effect description.";

        public override bool TryPlayCard(GameState gameState)
        {
            gameState.PlayerHand.AddCards(gameState.PlayerDeck.TakeCards(3));
            return true;
        }
    }
}