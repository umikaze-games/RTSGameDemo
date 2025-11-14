using UnityEngine;

[CreateAssetMenu(fileName = "Move Action", menuName = "AI/Actions/Move", order = 100)]
public class MoveCommand :ActionBase
{
	public override bool CanHandle(AbstractCommandable commandable, RaycastHit hit)
	{
		return commandable is IMoveable;
	}

	public override void Handle(AbstractCommandable commandable, RaycastHit hit)
	{
		IMoveable moveable = commandable as IMoveable;
		moveable.MoveTo(hit.point);
	}
}
