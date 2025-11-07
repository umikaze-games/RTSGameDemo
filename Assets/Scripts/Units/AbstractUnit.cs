using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(NavMeshAgent))]
public abstract class AbstractUnit : MonoBehaviour, ISelectable, IMoveable
{
	[SerializeField] private Transform target;
	[SerializeField] private DecalProjector decalProjecter;
	private NavMeshAgent agent;

	private void Start()
	{
		Bus<UnitSpawnEvent>.Raise(new UnitSpawnEvent(this));
	}
	public void Deselect()
	{
		if (decalProjecter != null)
		{
			decalProjecter.gameObject.SetActive(false);
		}
		Bus<UnitDeselectedEvent>.Raise(new UnitDeselectedEvent(this));
	}

	public void MoveTo(Vector3 position)
	{
		agent.SetDestination(position);
	}

	public void Select()
	{
		if (decalProjecter != null)
		{
			decalProjecter.gameObject.SetActive(true);
		}
		Bus<UnitSelectedEvent>.Raise(new UnitSelectedEvent(this));
	}

	private void Awake()
	{
		agent = GetComponent<NavMeshAgent>();
	}
}
