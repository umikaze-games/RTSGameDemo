using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class ActionUI : MonoBehaviour
{
	[SerializeField] private UIActionButton[] ActionButtons;
	private HashSet<AbstractCommandable> selectedUnits = new(12);
	private void Awake()
	{
		Bus<UnitSelectedEvent>.OnEvent += HandleUnitSelected;
		Bus<UnitDeselectedEvent>.OnEvent += HandleDeUnitSelected;
	}
	private void OnDestroy()
	{
		Bus<UnitSelectedEvent>.OnEvent-= HandleUnitSelected;
		Bus<UnitDeselectedEvent>.OnEvent -= HandleDeUnitSelected;
	}
	private void HandleDeUnitSelected(UnitDeselectedEvent evt)
	{
		if (evt.Unit is AbstractCommandable commandable)
		{
			selectedUnits.Add(commandable);
			RefreshButtons();
		}
	}

	private void HandleUnitSelected(UnitSelectedEvent evt)
	{
		if (evt.Unit is AbstractCommandable commandable)
		{
			selectedUnits.Remove(commandable);
			RefreshButtons();
		}
	}

	private void RefreshButtons()
	{
		HashSet<ActionBase> availableCommands = new(9);

		foreach (AbstractCommandable commandable in selectedUnits)
		{
			availableCommands.AddRange(commandable.AvailableCommands);
		}

		for (int i = 0; i < ActionButtons.Length; i++)
		{
			ActionBase actionForSlot = availableCommands.Where(action => action.Slot == i).FirstOrDefault();

			if (actionForSlot != null)
			{
				ActionButtons[i].SetIcon(actionForSlot.Icon);
			}
			else
			{
				ActionButtons[i].SetIcon(null);
			}
		}

	}

}
