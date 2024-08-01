
using Godot;

public abstract partial class CardFactory : Node
{
    public string cardId { get; internal set; }
    internal CardBase CreateNewCard(GameState game)
    {
        CardBase newCard = CreateCard(game);
        newCard.cardId = cardId;
        return newCard;
    }
    
    public abstract CardBase CreateCard(GameState game);
}
