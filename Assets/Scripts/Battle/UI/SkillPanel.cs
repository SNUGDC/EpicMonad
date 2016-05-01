using UnityEngine;

namespace BattleUI
{
	public class SkillPanel : MonoBehaviour
	{
		private BattleManager gameManager;

		public void Start()
		{
			gameManager = FindObjectOfType<BattleManager>();
		}

		public void CallbackSkillIndex(int index)
		{
			gameManager.CallbackSkillIndex(index);
		}

		public void CallbackSkillUICancel()
		{
			gameManager.CallbackSkillUICancel();
		}
	}
}
