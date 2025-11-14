using UnityEngine;
using UnityEngine.Rendering.Universal;

public class AbstractCommandable : MonoBehaviour, ISelectable
{

	[field: SerializeField] public int CurrentHealth { get; private set; }
	[field: SerializeField] public int MaxHealth { get; private set; }
	[field: SerializeField] public ActionBase[] avaliableCommands { get; private set; }
	[SerializeField] DecalProjector decalProjector;
	[SerializeField] UnitSO unitSO;

	protected virtual void Start()
	{
		MaxHealth = unitSO.Health;
		CurrentHealth = unitSO.Health;
	}
	public void Deselect()
	{
		if (decalProjector != null)
		{
			decalProjector.gameObject.SetActive(false);
		}

		Bus<UnitDeselectedEvent>.Raise(new UnitDeselectedEvent(this));
	}

	public void Select()
	{
		if (decalProjector != null)
		{
			decalProjector.gameObject.SetActive(true);
		}
		Bus<UnitSelectedEvent>.Raise(new UnitSelectedEvent(this));
	}
}
