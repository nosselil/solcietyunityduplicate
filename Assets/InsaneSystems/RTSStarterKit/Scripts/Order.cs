using UnityEngine;

namespace InsaneSystems.RTSStarterKit
{
	/// <summary> This class describes order to the unit. There some already implemented inherited classes like AttackOrder and MovePositionOrder. You can create your own if you want. </summary>
	[System.Serializable]
	public abstract class Order
	{
		public Unit executor;
		bool isStarted;
		
		public void Execute()
		{
			if (!isStarted)
			{
				isStarted = true;
				StartAction();
			}

			ExecuteAction();
		}
		
		public virtual void StartAction() { }

		protected virtual void ExecuteAction() { End(); }
		
		public virtual void End() { executor.EndCurrentOrder(); }
		public abstract Order Clone();

		protected virtual Vector3 GetActualMovePosition() { return Vector3.zero; }
	}

	[System.Serializable]
	public class AttackOrder : Order
	{
		public Unit attackTarget;
		
		Vector3 moveOffset;

		public override void StartAction()
		{
			if (attackTarget)
				moveOffset = attackTarget.GetNearPoint(true);
		}
		
		protected override void ExecuteAction()
		{
			if (!executor.attackable || !attackTarget)
			{
				End();
				return;
			}

			if (executor.movable)
			{
				if (!executor.attackable.IsFireLineFree(attackTarget) ||
				    !executor.attackable.IsTargetInAttackRange(attackTarget))
					executor.movable.MoveToPosition(GetActualMovePosition());
				else
					executor.movable.Stop();
			}

			// todo extend attackTarget
		}

		public override Order Clone()
		{
			AttackOrder order = new AttackOrder
			{
				attackTarget = attackTarget
			};

			return order;
		}

		protected override Vector3 GetActualMovePosition() { return attackTarget.transform.position + moveOffset; }
	}

	[System.Serializable]
	public class MovePositionOrder : Order
	{
		public Vector3 movePosition;

		protected override void ExecuteAction()
		{
			if (!executor || !executor.movable)
			{
				if (!executor.movable)
					Debug.LogWarning("Movable order given to wrong unit.");

				End();
				return;
			}

			executor.movable.MoveToPosition(movePosition); // todo do it once at start

			var movePosWithSameY = movePosition;
			movePosWithSameY.y = executor.transform.position.y;
			
			if ((executor.transform.position - movePosWithSameY).sqrMagnitude <= executor.movable.sqrDistanceFineToStop)
				End();
		}

		public override Order Clone()
		{
			MovePositionOrder order = new MovePositionOrder
			{
				movePosition = movePosition
			};

			return order;
		}

		protected override Vector3 GetActualMovePosition() { return movePosition; }
	}

	[System.Serializable]
	public class FollowOrder : Order
	{
		public Unit followTarget;

		Vector3 moveOffset;

		public override void StartAction()
		{
			if (followTarget)
				moveOffset = followTarget.GetNearPoint(true);
		}

		protected override void ExecuteAction()
		{
			if (!followTarget)
			{
				End();
				return;
			}

			executor.movable.MoveToPosition(GetActualMovePosition());

			// todo change it to make set movable target to follow target
		}

		public override Order Clone()
		{
			FollowOrder order = new FollowOrder();
			order.followTarget = followTarget;

			return order;
		}

		protected override Vector3 GetActualMovePosition() { return followTarget.transform.position  + moveOffset; }
	}
}