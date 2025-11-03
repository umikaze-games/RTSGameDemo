using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(NavMeshAgent))]
public class Worker : MonoBehaviour,ISelectable
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

	private void Update()
	{
		//agent.SetDestination(target.position);
	}
}
