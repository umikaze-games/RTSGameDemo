using UnityEngine;
using UnityEngine.Rendering.Universal;

public class SupplyHut : MonoBehaviour, ISelectable
{
	[SerializeField] DecalProjector decalProjector;
	[field: SerializeField] public int Health { get; private set; }
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
