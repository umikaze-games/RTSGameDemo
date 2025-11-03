using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(NavMeshAgent))]
public class Worker : MonoBehaviour,ISelectable,IMoveable
{
	[SerializeField]private Transform target;
	[SerializeField]private DecalProjector decalProjecter;
	private NavMeshAgent agent;

	public void DeSelect()
	{
		if (decalProjecter != null)
		{
			decalProjecter.gameObject.SetActive(false);
		}
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
	}

	private void Awake()
	{
		agent = GetComponent<NavMeshAgent>();
	}

}
