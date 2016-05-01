using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Enums;

namespace Battle.Turn
{
	public class SelectMovingPointState
	{
		public static IEnumerator Run(BattleManager battleManager)
		{
			while (battleManager.currentState == CurrentState.SelectMovingPoint)
			{
				// List<GameObject> movableTiles = CheckMovableTiles(selectedUnitObject);
				Dictionary<Vector2, TileWithPath> movableTilesWithPath = PathFinder.CalculatePath(battleManager.selectedUnitObject);
				List<GameObject> movableTiles = new List<GameObject>();
				foreach (KeyValuePair<Vector2, TileWithPath> movableTileWithPath in movableTilesWithPath)
				{
					movableTiles.Add(movableTileWithPath.Value.tile);
				}

				battleManager.tileManager.ChangeTilesToSeletedColor(movableTiles, TileColor.Blue);

				battleManager.rightClicked = false;
				battleManager.cancelClicked = false;
				battleManager.uiManager.EnableCancelButtonUI();

				battleManager.isWaitingUserInput = true;
				battleManager.isSelectedTileByUser = false;
				while (!battleManager.isSelectedTileByUser)
				{
					//yield break 넣으면 코루틴 강제종료
					if (battleManager.rightClicked || battleManager.cancelClicked)
					{
						battleManager.rightClicked = false;
						battleManager.cancelClicked = false;
						battleManager.uiManager.DisableCancelButtonUI();

						battleManager.tileManager.ChangeTilesFromSeletedColorToDefaultColor(movableTiles);

						battleManager.currentState = CurrentState.FocusToUnit;
						battleManager.isWaitingUserInput = false;
						yield break;
					}
					yield return null;
				}
				battleManager.isSelectedTileByUser = false;
				battleManager.isWaitingUserInput = false;


				// FIXME : 어딘가로 옮겨야 할 텐데...
				GameObject destTile = battleManager.tileManager.GetTile(battleManager.selectedTilePosition);
				List<GameObject> destPath = movableTilesWithPath[battleManager.selectedTilePosition].path;
				Vector2 currentTilePos = battleManager.selectedUnitObject.GetComponent<Unit>().GetPosition();
				Vector2 distanceVector = battleManager.selectedTilePosition - currentTilePos;
				int distance = (int)Mathf.Abs(distanceVector.x) + (int)Mathf.Abs(distanceVector.y);
				int totalUseActionPoint = movableTilesWithPath[battleManager.selectedTilePosition].requireActivityPoint;

				battleManager.moveCount += distance;

				battleManager.tileManager.ChangeTilesFromSeletedColorToDefaultColor(movableTiles);
				battleManager.currentState = CurrentState.CheckDestination;
				battleManager.uiManager.DisableCancelButtonUI();
				yield return battleManager.StartCoroutine(CheckDestination(battleManager, movableTiles, destTile, destPath, totalUseActionPoint, distance));
			}
			yield return null;
		}

		private static IEnumerator CheckDestination(BattleManager battleManager, List<GameObject> nearbyTiles, GameObject destTile, List<GameObject> destPath, int totalUseActionPoint, int distance)
		{
			while (battleManager.currentState == CurrentState.CheckDestination)
			{
				// 목표지점만 푸른색으로 표시
				// List<GameObject> destTileList = new List<GameObject>();
				// destTileList.Add(destTile);
				List<GameObject> destTileList = destPath;
				destTileList.Add(destTile);
				battleManager.tileManager.ChangeTilesToSeletedColor(destTileList, TileColor.Blue);
				// UI를 띄우고
				battleManager.uiManager.EnableSelectDirectionUI();
				battleManager.uiManager.SetDestCheckUIAP(battleManager.selectedUnitObject, totalUseActionPoint);

				// 카메라를 옮기고
				Camera.main.transform.position = new Vector3(destTile.transform.position.x, destTile.transform.position.y, -10);
				// 클릭 대기
				battleManager.rightClicked = false;
				battleManager.cancelClicked = false;
				battleManager.uiManager.EnableCancelButtonUI();

				battleManager.isWaitingUserInput = true;
				battleManager.isSelectedDirectionByUser = false;
				while (!battleManager.isSelectedDirectionByUser)
				{
					// 클릭 중 취소하면 돌아감
					// moveCount 되돌리기
					// 카메라 유닛 위치로 원상복구
					// 이동가능 위치 다시 표시해주고
					// UI 숨기고
					if (battleManager.rightClicked || battleManager.cancelClicked)
					{
						battleManager.rightClicked = false;
						battleManager.cancelClicked = false;
						battleManager.uiManager.DisableCancelButtonUI();

						battleManager.moveCount -= distance;
						Camera.main.transform.position = new Vector3(battleManager.selectedUnitObject.transform.position.x, battleManager.selectedUnitObject.transform.position.y, -10);
						battleManager.tileManager.ChangeTilesToSeletedColor(nearbyTiles, TileColor.Blue);
						battleManager.uiManager.DisableSelectDirectionUI();
						battleManager.uiManager.DisableDestCheckUI();
						battleManager.currentState = CurrentState.SelectMovingPoint;
						battleManager.isWaitingUserInput = false;
						yield break;
					}
					yield return null;
				}
				battleManager.isSelectedDirectionByUser = false;
				battleManager.isWaitingUserInput = false;
				battleManager.uiManager.DisableCancelButtonUI();

				// 방향을 클릭하면 그 자리로 이동. MoveToTile 호출
				battleManager.tileManager.ChangeTilesFromSeletedColorToDefaultColor(destTileList);
				battleManager.currentState = CurrentState.MoveToTile;
				battleManager.uiManager.DisableDestCheckUI();
				yield return battleManager.StartCoroutine(battleManager.MoveToTile(destTile, battleManager.selectedDirection, totalUseActionPoint));
			}
			yield return null;
		}
	}
}