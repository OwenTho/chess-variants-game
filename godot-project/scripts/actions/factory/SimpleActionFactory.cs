
public partial class SimpleActionFactory<T> : ActionFactory where T : ActionBase, new()
{
    public override ActionBase CreateAction()
    {
        return new T();
    }

    public override bool ActionTypeMatches(ActionBase action)
    {
        return action.GetType() == typeof(T);
    }
}