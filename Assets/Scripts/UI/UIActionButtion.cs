using UnityEngine;
using UnityEngine.UI;
//using UnityEngine.UIElements;
public class UIActionButton : MonoBehaviour
{
	[SerializeField]private Image Icon;

	public void SetIcon(Sprite icon)
	{
		if (icon == null)
		{
			Icon.enabled = false;
		}
		else
		{
			Icon.sprite = icon;
			Icon.enabled=true;
		}
	}
}
