using InsaneSystems.RTSStarterKit;
using UnityEngine;
using UnityEngine.AI;

// New custom script 
public class UnitAnimatorManager : MonoBehaviour
{
    private Unit unit = null;
    private Animator animator = null;
    private NavMeshAgent agent = null;
    private void Awake()
    {
        unit = GetComponent<Unit>();
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        if(agent.hasPath && agent.remainingDistance > 1)
        {
            // Play movement animation
            animator.SetFloat("Speed", 1);
        }
        else
        {
            animator.SetFloat("Speed", 0);
        }
    }
}