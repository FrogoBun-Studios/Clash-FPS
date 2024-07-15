public static class CardTypes
{
    public const string Wizard = "Wizard";
    public const string Valkyrie = "Valkyrie";

    public static Card StringToCard(string cardName){
        switch(cardName){
            case Wizard:
                return new WizardCard();
            case Valkyrie:
                return new ValkyrieCard();
            default:
                return null;
        }
    }
}