
// A value returned by an existing card if the card ids match.
public enum CardReturn
{
    // If the new card is ignored by the existing card and it moves to the next.
    Ignored,
    // The existing card was disabled (and then moves on).
    DisabledThis,
    // The new card is disabled (and then moves on).
    DisabledNew,
    // The new card has been combined with the existing card, and does not continue on to other cards.
    Combined
}