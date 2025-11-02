using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Worker : MonoBehaviour
{
	[SerializeField]Transform target;
	private NavMeshAgent agent;
	private void Awake()
	{
		agent = GetComponent<NavMeshAgent>();
	}

	private void Update()
	{
		agent.SetDestination(target.position);
	}
}
