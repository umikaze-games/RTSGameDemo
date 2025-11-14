using UnityEngine;

public interface ICommand
{
	public void Handle(AbstractCommandable commandable,RaycastHit hit);
	public bool CanHandle(AbstractCommandable commandable, RaycastHit hit);
}
